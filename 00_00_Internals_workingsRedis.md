https://betterprogramming.pub/internals-workings-of-redis-718f5871be84

# An In-Depth Look Into the Internal Workings of Redis

## How does this dictionary server perform with high throughput and low latency?


In this article, I’ll cover the following things to show the internal workings of Redis:

Overview of the basic definition and the reasons for its high performance
Describe blocking I/O, non-blocking I/O, and I/O multiplexing with async I/O
Redis event-loop algorithm with code
Introduction
I have been using Redis for some time in production systems and have always been amazed by its performance. Lately, I have been reckoning with the reasons. I read some answers and posts that gave me high-level insights and enough points to kickstart my expedition.

Before we deep-dive into the hows and whys, let me define Redis. Redis is a Remote Dictionary Server. It is a TCP server providing in-memory data structures like dictionaries, sets, etc. Redis has many uses like caching, session storage, real-time data stores, streaming engine, etc. Many tech organisations have been using Redis because it delivers high throughput with low latency (HTLL).

Upon reading a few posts, I could drill into the following reasons for its high-throughput-low-latency performance.

Redis stores all of its data in the main memory (RAM) and periodically stores it in the disk. RAM-based access is fast compared to disk-based access.
Redis is a single-threaded implementation of an event-driven system enabled by the I/O multiplexing variant for processing all the connections.
Redis uses highly efficient data structures like skip-lists, simple dynamic strings, etc.
In this article, we will highlight the second point because that’s a significant contributor to high throughput and low-level latency.

Background
Suppose we want to design a web server that can handle concurrent clients. The first possible — and brute-force solution — for this client-server model is to make a client create a connection to the server’s socket. After this, the server listens to the connection on a socket, accepts it, and handles it.

To make it concurrent, we can opt for a multithreaded implementation where every connection creates a new thread, and that thread handles the entire journey of that connection. One possible optimisation is to use a thread pool and assign a connection to it.

The drawbacks of this approach are the following:

Multiple threads would be triggered, which will cause performance issues because of huge context switching and high memory usage. In this case, the CPU will spend most of its time switching, scheduling, maintaining thread life cycle, etc.
Each thread will be busy waiting on the connections for the clients to send data and perform disk I/O operations. In this case, the CPU will spend the remaining time waiting for I/O.
One thread per connection is blocked on I/O in the above case. It cannot proceed until it receives data. I/O is always slower than network I/O, and disk I/O is outside the CPU’s domain, where it cannot do anything except wait. This kind of situation is termed blocking I/O.

Note: I/O refers to Input/Output in the computing world to denote how communication happens between systems. It is everything that happens outside of the CPU’s domains.

Blocking I/O
With the blocking I/O, when the client makes a connection request to the server, the socket and its thread are blocked from processing that connection until some read data appears. This data is placed in the network buffer until it is all read and ready for processing. Until the operation is complete, the server can do nothing but wait.

With blocking I/O, a client makes a connection request, and the server socket and thread handling processing that request is blocked on socket I/O until some data appears. In actuality, the thread calls read(2) operation with fd of the socket and buf. The kernel writes the data from the socket file descriptor to a buffer until it is ready for processing. Until this operation is complete, the thread is blocked.


As mentioned above, the one-thread-per-connection with a blocking I/O approach is not ideal for many concurrent connections. How can we make our server cater to a large number of clients?

Non-Blocking I/O
Now let’s change the approach. Instead of one thread per connection, let’s have a single thread that will accept connections in a non-blocking way. But how? Luckily, there is a way to make this single thread stay unblocked.

By setting the socket’s fd with O_NONBLOCK flag, we can make this possible. Now, if the thread calls accept() on that fd and there is no data available on that fd, it will get an error EAGAIN or EWOULDBLOCK. This error depicts the non-blocking nature of the I/O.

Upon getting the error, the thread will poll again to see if there is any data available on that fd. Now, a new activity happens on that socket, and then accept() returns a new cfd file descriptor for every new connection. We can enqueue such cfd for all the new connections getting accepted and perform read() on those cfd.

The above solution is polling. It keeps the thread busy-wait by continuously making it check the error codes for all fdand cfd, and try again. This causes expensive CPU time and wasted cycles. This is a non-blocking operation but a synchronous one.


I/O multiplexing
We can use another alternative, which is I/O multiplexing. With I/O multiplexing, we can monitor multiple fd with a single blocking call. But how?

We can use select() call, which monitors multiple fd, and every select() call will return the number of fd that are ready to accept/read/write. When this retval is non-zero, we have to explicitly check for all the fd which are ready for read/write.

The select() allows multiple sockets fd to be monitored in a non-blocking way and returned if multiple sockets are ready for different operations. Since we still don’t know exactly which fd are ready for the operation, we have to run a loop to check the “readiness.” Still, it doesn’t solve the problem of busy-wait .

This is non-blocking I/O multiplexing but a synchronous one.

Async I/O
All the above approaches are synchronous and do not solve the problem efficiently. If we can get the exact fd that’s ready instead of only the number of “ready” sockets, we won’t have busy-wait . Because the main single thread can then execute the operations on those available and “ready” fd, instead of just waiting. CPU cycles are getting used in a useful manner, and they are not wasted. But how can we achieve this?

Luckily, there is another API, epoll(), that can solve this problem for good. It will return the available sockets, and then we can loop through them and perform the operations. This is similar to an event-driven system. The main thread is preparing the events for operating/processing/handling.

Unfortunately, due to the scope of this article, I won’t be able to cover epoll in detail. But you can read this amazing and detailed blog by Cindy Sridharan.

Here’s a visual summary of the above discussion:


Redis Event Loop
Redis uses the same approach of implementing a single thread and event loop like node.js. Redis accepts TCP connections in an async manner, then handles each accepted connection in the event loop. It uses epoll() for knowing the fd which are available and ready for the read/write operation.

The primary functions of the event loop are the following:

Accept new client connections
Respond to commands from existing connections
Let me briefly explain the algorithm of this event loop.

Initialises and registers socket connection type
Initialises server’s event loop server.el
Binds the port and addr and initialises the socket listeners
Registers accept handlers of the connections
Traverse the “ready” events from the event loop enqueued by epoll endpoints
Handle/process those events by the registered handlers
Let’s deep-dive into each part of the algorithm.

The main logic resides in server.c. Yes, Redis is a written C language.

And many different files like

connection.c and connection.h
socket.c
anet.c
ae.c and ae.h
ae_epoll.c
are important and relevant to understand socket networking in Redis and event loops.

Initialisation and Registration of Socket
The main func in server.c initialises and registers the connection type socket known as CT_Socket. This is an important step because it has a lot of functional pointers that handle, read and write data on the socket. To initialise and register, the main func calls connTypeInitialize() of connection.c, which internally calls connTypeRegister(&CT_Socket).

/* server.c */
int main() {
  ...
  
  connTypeInitialize()
  ...
}


/* connection.c */
int connTypeRegister(ConnectionType *ct) {
    ...
    ConnectionType *tmpct;
    int type;

    /* find an empty slot to store the new connection type */
    for (type = 0; type < CONN_TYPE_MAX; type++) {
        tmpct = connTypes[type];
        if (!tmpct)
            break;
        ...
    }

    connTypes[type] = ct;
    ...
    return C_OK;
}
As I mentioned above, ConnTypeSocket has a lot of important functional pointers, which consist of event handlers, processors, etc. I have listed a few important ones. For more details, you can check socket.c.

static ConnectionType CT_Socket = {
     ...

    /* ae & accept & listen & error & address handler */
    .ae_handler = connSocketEventHandler,
    .accept_handler = connSocketAcceptHandler,
    .addr = connSocketAddr,
    .listen = connSocketListen,

    /* create/shutdown/close connection */
    .conn_create = connCreateSocket,
    .conn_create_accepted = connCreateAcceptedSocket,
    ...

    /* connect & accept */
    .connect = connSocketConnect,
    .accept = connSocketAccept,

    /* IO */
    .write = connSocketWrite,
    .writev = connSocketWritev,
    .read = connSocketRead,
    .set_write_handler = connSocketSetWriteHandler,
    .set_read_handler = connSocketSetReadHandler,
    ...
    /* pending data */
    .has_pending_data = NULL,
    .process_pending_data = NULL,
}; 
Initialisation of Redis event loop
Redis event loop is defined by a variable aeEventLoop *el of server struct variable. For more details of the struct, you can read this article.

To initialise eventLoop, the initServer() func is called by main(). In this function, server.el is initialised by calling aeCreateEventLoop() defined in ae.c.

/* server.c */
void initServer(void) {
  ...

   server.el = aeCreateEventLoop(server.maxclients+CONFIG_FDSET_INCR);

  ...

}
Here’s some important aeEventLoop fields defined in ae.h:

typedef struct aeEventLoop
{
    int maxfd;
    long long timeEventNextId;
    aeFileEvent events[AE_SETSIZE]; /* Registered events */
    aeFiredEvent fired[AE_SETSIZE]; /* Fired events */
    aeTimeEvent *timeEventHead;
    int stop;
    void *apidata; /* This is used for polling API specific data */
    aeBeforeSleepProc *beforesleep;
} aeEventLoop;
aeCreateEventLoop calls aeApiCreate which mallocs aeApiState that has two fields — epfd that holds the epoll file descriptor returned by a call from epoll_create and events that is of type struct epoll_event. These are defined by the Linux epoll library.

aeCreateEventLoop only initialises server.el and doesn’t register events to wait on and handlers for handling the ready events.

Registering Events With eventLoop
In Redis, there are two types of events, fileEvents and timedEvents. To register fileEvents , we call aeCreateFileEvent of ae.c. This func accepts eventLoop, the file descriptor of the event, the handler to handle the event, and the clientData to send if there are any. It stores this information in an eventLoop -> events array.

This registration is done by calling aeApiAddEvent() of ae_epoll.c.

In a later section, we will cover the implementation of ae_epoll.c. But for now, remember that to register any file event, we need to use aeCreateFileEvent. Here’s what the code looks like:

int aeCreateFileEvent(aeEventLoop *eventLoop, int fd, int mask,
        aeFileProc *proc, void *clientData)
{
    if (fd >= eventLoop->setsize) {
        errno = ERANGE;
        return AE_ERR;
    }
    aeFileEvent *fe = &eventLoop->events[fd];
    
    /* add the file descriptor to event-loop */
    if (aeApiAddEvent(eventLoop, fd, mask) == -1)
        return AE_ERR;
    fe->mask |= mask;
    
    /* sets the read handler */
    if (mask & AE_READABLE) fe->rfileProc = proc;

    /* sets the write handler */
    if (mask & AE_WRITABLE) fe->wfileProc = proc;

    fe->clientData = clientData;
    if (fd > eventLoop->maxfd)
        eventLoop->maxfd = fd;
    return AE_OK;
}
Initialisation of Listeners
To accept new connections, the following steps are required:

Initialise the listeners. Bind and start listening to the sockets
Register accept handlers for accepting new connections.
Registration of read handlers for the accepted connections.
Creating posts of the server’s event loop, main, initialises socket listeners based on the connection type. For each connectionType, there is a listener. A few examples of connection types are TCP Socket, TLS socket, UNIX socket, etc. The connListener is defined as a struct in connection.h .

   /* server.h */
struct redisServer {
...   
  
connListener listeners[CONN_TYPE_MAX]

...
}

/* connection.h */
/* Setup a listener by a connection type */
struct connListener {
    int fd[CONFIG_BINDADDR_MAX];
    int count;
    char **bindaddr;
    int bindaddr_count;
    int port;
    ConnectionType *ct; /* important, it has all the functionality for the conn*/
    void *priv; 
};
Initialisation of the Listeners
The main func calls initListeners() implemented in server.c .

void initListeners() {
    /* Setup listeners from server config for TCP/TLS/Unix */
    int conn_index;
    connListener *listener;  // defined in connection.h
    if (server.port != 0) {
        conn_index = connectionIndexByType(CONN_TYPE_SOCKET);
        ...
        listener = &server.listeners[conn_index];
        listener->bindaddr = server.bindaddr;
        listener->bindaddr_count = server.bindaddr_count;
        listener->port = server.port;
        listener->ct = connectionByType(CONN_TYPE_SOCKET);
    }

    ...

    /* create all the configured listener, and add handler to start to accept */
    int listen_fds = 0;
    for (int j = 0; j < CONN_TYPE_MAX; j++) {
        listener = &server.listeners[j];
          ...
        
        /* bind and listen*/ // ----- step 1
        if (connListen(listener) == C_ERR) { 
             ...
        }
        
        /* register socket accept handler */  // ---- step 2
        if (createSocketAcceptHandler(listener, connAcceptHandler(listener->ct)) != C_OK)
  
          ...

       listen_fds += listener->count;
    }
        ...
}
connListen internally calls anetTcpServer in anet.c via listenToPort()

anetTcpServer binds address to the socket and starts listening on s. It later returns this s to the caller listenToPort(), which stores this in listener.fd array. listenToPort() also sets every listening socket as O_NONBLOCK.

/* anet.c */
/* binds the addr and listens on the socket s fd */
static int anetListen(char *err, int s, struct sockaddr *sa, socklen_t len, int backlog) {
    if (bind(s,sa,len) == -1) {
        anetSetError(err, "bind: %s", strerror(errno));
        close(s);
        return ANET_ERR;
    }

    if (listen(s, backlog) == -1) {
        anetSetError(err, "listen: %s", strerror(errno));
        close(s);
        return ANET_ERR;
    }
    return ANET_OK;
}


/* server.c */

int listenToPort(connListener *sfd) {
    int j;
    int port = sfd->port;
    char **bindaddr = sfd->bindaddr;

    /* If we have no bind address, we don't listen on a TCP socket */
    if (sfd->bindaddr_count == 0) return C_OK;

    for (j = 0; j < sfd->bindaddr_count; j++) {
        char* addr = bindaddr[j];
            ...
          /* Bind IPv4 address to a socket fd */
          sfd->fd[sfd->count] = anetTcpServer(server.neterr,port,addr,server.tcp_backlog);


        if (sfd->fd[sfd->count] == ANET_ERR) {
              ...
            /* Rollback successful listens before exiting */
            closeListener(sfd);
            return C_ERR;
        }

        if (server.socket_mark_id > 0) anetSetSockMarkId(NULL, sfd->fd[sfd->count], server.socket_mark_id);

        // setting up this socket/file descriptor as non-blocking
        anetNonBlock(NULL,sfd->fd[sfd->count]); 
        ...
        sfd->count++;
    }
    return C_OK;
}
Registration of Accept Handlers
Once the listeners are set up for each bind_addr, it is time to register accept_handlers for each listening socket in the event-loop server.el, which is done by createSocketAcceptHandler. It registers an accept_handler with the file descriptor using a mask value of AE_READABLE in the event loop by calling aeCreateFileEvent of ae.c.

/* Create an event handler for accepting new connections in TCP or TLS domain sockets.
 * This works atomically for all socket fds */
int createSocketAcceptHandler(connListener *sfd, aeFileProc *accept_handler) {
    int j;

    for (j = 0; j < sfd->count; j++) {
        if (aeCreateFileEvent(server.el, sfd->fd[j], AE_READABLE, accept_handler,sfd) == AE_ERR) {
            /* Rollback */
            for (j = j-1; j >= 0; j--) aeDeleteFileEvent(server.el, sfd->fd[j], AE_READABLE);
            return C_ERR;
        }
    }
    return C_OK;
}
Now, let’s see the implementation of accept_handler . accept_handler is called when the socket is ready to accept. It accepts the connection on fd , the anetTcpAccept() returns a new socket cfd upon every new connection.

/* socket.c */
/* accept_handler registered in the eventloop by aeCreateFileEvent() */

static void connSocketAcceptHandler(aeEventLoop *el, int fd, void *privdata, int mask) {
    int cport, cfd, max = MAX_ACCEPTS_PER_CALL;
    ...
    while(max--) {

        /* accept() returns a new socket `cfd` upon accepting a conn on `fd`*/
        cfd = anetTcpAccept(server.neterr, fd, cip, sizeof(cip), &cport);

        if (cfd == ANET_ERR) {
            if (errno != EWOULDBLOCK)
                serverLog(LL_WARNING,
                    "Accepting client connection: %s", server.neterr);
            return;
        }
        serverLog(LL_VERBOSE,"Accepted %s:%d", cip, cport);
        /* registers read handlers */
        acceptCommonHandler(connCreateAcceptedSocket(cfd, NULL),0,cip);
    }
}
Registration of Read Handlers
The read handler (used for connections ready to read) can’t be registered when the server boots since the connections aren’t established. These events are set up after a connection is accepted.

The func acceptCommonHandler() registers the read handler by calling the following:

/* connection.h*/
/* Register a read handler, to be called when the connection is readable.
 * If NULL, the existing handler is removed.
 */
static inline int connSetReadHandler(connection *conn, ConnectionCallbackFunc func) {
    return conn->type->set_read_handler(conn, func);
}
The func set_read_handler is a functional ptr of connSocketSetReadHandler. As you can see, we are also creating a file called Event in the eventLoop.


/* socket.c */
/* read handler for the accepted connection */
static int connSocketSetReadHandler(connection *conn, ConnectionCallbackFunc func) {
    if (func == conn->read_handler) return C_OK;
    conn->read_handler = func;
    if (!conn->read_handler)
        aeDeleteFileEvent(server.el,conn->fd,AE_READABLE);
    else
        if (aeCreateFileEvent(server.el,conn->fd,
                    AE_READABLE,conn->type->ae_handler,conn) == AE_ERR) return C_ERR;
    return C_OK;
}
The ConnectionCallBackFunc func arg of this function is actually readQueryFromClient() which internally calls connRead()

Similarly, write handlers are also registered.

Looping Through Event Loop
We have looked at the registration process. It is now time to see how the event loop is traversed. Once the listeners are initialised and handlers are registered, the main() func calls aeMain(server.el) with this code:

/* server.c */
int main() {
  ...
  aeMain(server.el)
}
aeMain() loops infinitely and calls aeProcessEvent().

/* ae.c */
void aeMain(aeEventLoop *eventLoop) {
    eventLoop->stop = 0;
    while (!eventLoop->stop) {
        aeProcessEvents(eventLoop, AE_ALL_EVENTS|
                                   AE_CALL_BEFORE_SLEEP|
                                   AE_CALL_AFTER_SLEEP);
    }
}


/* Process every pending time event, then every pending file event
 * (that may be registered by time event callbacks just processed).*/
int aeProcessEvents(aeEventLoop *eventLoop, int flags)
{
    int processed = 0, numevents;
              ...

        /* Call the multiplexing API, will return only on timeout or when
         * some event fires. */
        numevents = aeApiPoll(eventLoop, tvp);

        for (j = 0; j < numevents; j++) {
            int fd = eventLoop->fired[j].fd;
            aeFileEvent *fe = &eventLoop->events[fd];
            ...
            int fired = 0; /* Number of events fired for current fd. */

            if (!invert && fe->mask & mask & AE_READABLE) {
                fe->rfileProc(eventLoop,fd,fe->clientData,mask);
                fired++;
                fe = &eventLoop->events[fd]; /* Refresh in case of resize. */
            }

            /* Fire the writable event. */
            if (fe->mask & mask & AE_WRITABLE) {
                if (!fired || fe->wfileProc != fe->rfileProc) {
                    fe->wfileProc(eventLoop,fd,fe->clientData,mask);
                    fired++;
                }
            }

             ...
            processed++;
        }
    }

    return processed; /* return the number of processed file/time events */
}
aeProcessEvents() inherently calls aeApiPoll() which returns numevents, the number of events ready for read/write. When aeApiPoll() is called, it makes a blocking call to epoll_wait() on the epoll descriptor. This descriptor sets the ready events in el -> fired.

/* ae_epoll.c */
static int aeApiPoll(aeEventLoop *eventLoop, struct timeval *tvp) {
    aeApiState *state = eventLoop->apidata;
    int retval, numevents = 0;

    /* epoll_wait is a blocking call which returns the num of ready events 
       and set the file descriptors in state -> events.
       To avoid prolonged blocking, 'timeout' is passed. Once the 
       timeout expires, epoll_wait returns immediately, passing control to 
       the caller.
    */
    retval = epoll_wait(state->epfd,state->events,eventLoop->setsize,
            tvp ? (tvp->tv_sec*1000 + (tvp->tv_usec + 999)/1000) : -1);
    
       ...
    numevents = retval;
        for (j = 0; j < numevents; j++) {
            struct epoll_event *e = state->events+j;
              ...
            eventLoop->fired[j].fd = e->data.fd;
        }
    }
    ... 

    return numevents;
}
You can see these are read by aeProcessEvents().

For example, if a client requests a connection, then aeApiPoll will notice it and populate the eventLoop->fired table with an entry of the descriptor. This entry is called the listening descriptor, and the mask is AE_READABLE.

Read the Ready Events
While traversing the events, aeProcessEvents() checks the mask for each fired event, and based on the set bits, it calls the respective handler.


aeProcessEvents() {
  ...
  aeFileEvent *fe = &eventLoop->events[fd];
  fe->rfileProc(eventLoop,fd,fe->clientData,mask); 
  ...
}
The rfileProc is set when the read handler is registered by connSocketSetReadHandler which calls aeCreateFileEvent.

int aeCreateFileEvent(aeEventLoop *eventLoop, int fd, int mask,
        aeFileProc *proc, void *clientData)
{
    ...
    aeFileEvent *fe = &eventLoop->events[fd];
    if (mask & AE_READABLE) fe->rfileProc = proc;
    ...
}
Summary
This article covered blocking, non-blocking I/O, and the Redis event loop.

Redis has everything running in a single thread in a non-blocking fashion.

First, it initialises the server’s event loop, server.el, then it binds the server address with the port and initialises the listeners sfd. While initialising, it registers the events with accept_handler in the event loop and uses the AE_READABLE mask by calling aeCreateFileEvent.
When a connection is accepted, it returns a new socket cfd which is registered with the read event loop by calling aeCreateFileEvent. This uses a different handler.
Redis then uses system calls like epoll(), epoll_wait(), etc., for getting the ready events. It processes all the events synchronously by triggering their respective registered handlers and continues this process until stopped.
References
Why Redis is so fast
blocking-and-non-blocking-io
blocking and non-blocking and epoll
Redis event library
Redis Source Code on Github
Tale of socket and client
Thoughts on Redis
Redis Event Loop
Epoll Madness
Lots of pages related to bind(), listen(), accept(), epoll(), epoll_create(), epoll_wait(), read(), write(), select()
