### Step 1: Install Docker

Make sure you have Docker installed on your system. You can download and install Docker from the official website: [Docker Install](https://docs.docker.com/get-docker/)

### Step 2: Pull the Redis Docker Image

Open a terminal and run the following command to pull the official Redis Docker image from Docker Hub:

```bash
docker pull redis
```

### Step 3: Run Redis Container

Now, you can run a Redis container using the pulled image. The following command will start a Redis server in the background:

```bash
docker run -d --name my-redis-container -p 6379:6379 redis
```

- `-d`: Run the container in the background.
- `--name my-redis-container`: Assign a custom name to the container.
- `-p 6379:6379`: Map the container's Redis port (6379) to the host machine's port (6379).

### Step 4: Verify Redis Container is Running

You can check if the Redis container is running with the following command:

```bash
docker ps
```

### Step 5: Run Redis CLI

To run the Redis CLI and connect to the Redis server in the Docker container, use the following command:

```bash
docker exec -it my-redis-container redis-cli
```

- `-it`: Interactive mode.

Now, you should be in the Redis CLI, and you can start interacting with your Redis server.

### Step 6: Test Redis Server

Inside the Redis CLI, you can test if the server is working by running a simple command:

```bash
ping
```

You should receive a "PONG" response, indicating that the Redis server is responding.

### Step 7: Stop and Remove the Redis Container

When you are done, you can stop and remove the Redis container:

```bash
docker stop my-redis-container
docker rm my-redis-container
```

Now you have successfully set up and run a Redis server and CLI using Docker. Adjust the container name and ports as needed for your specific requirements.
