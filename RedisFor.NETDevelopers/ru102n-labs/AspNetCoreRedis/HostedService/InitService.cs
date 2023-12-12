using AspNetCoreRedis.DataModel;
using AspNetCoreRedis.Repository.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;

namespace AspNetCoreRedis.HostedService
{
    public class InitService : IHostedService
    {
        private readonly IServiceScopeFactory _scopeFactory;

        public InitService(IServiceScopeFactory scopeFactory)
        {
            _scopeFactory = scopeFactory;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            using var scope = _scopeFactory.CreateScope();
            var salesDb = scope.ServiceProvider.GetRequiredService<SalesContext>();
            await ClearCache(scope, salesDb, cancellationToken);
            await ReseedDatabase(salesDb, cancellationToken);
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }


        async Task ClearCache(IServiceScope scope, SalesContext salesDb, CancellationToken cancellationToken)
        {
            var cache = scope.ServiceProvider.GetRequiredService<IDistributedCache>();
            var cachePipe = new List<Task>
            {
                cache.RemoveAsync("top:sales", cancellationToken),
                cache.RemoveAsync("top:name", cancellationToken),
                cache.RemoveAsync("totalSales", cancellationToken)
            };
            cachePipe.AddRange(salesDb.Employees.Select(emp => cache.RemoveAsync($"emp:{emp.EmployeeId}:avg", cancellationToken)));
            await Task.WhenAll(cachePipe);
        }

        async Task ReseedDatabase(SalesContext salesDb, CancellationToken cancellationToken)
        {
            await salesDb.Database.ExecuteSqlRawAsync("DELETE FROM Employees", cancellationToken);
            await salesDb.Database.ExecuteSqlRawAsync("DELETE FROM Sales", cancellationToken);

            var names = new[] { "Alice", "Bob", "Carlos", "Dan", "Yves" };
            foreach (var name in names)
            {
                var employee = new Employee { Name = name };
                salesDb.Employees.Add(employee);
            }
            await salesDb.SaveChangesAsync(cancellationToken);

            var random = new Random();
            foreach (var name in names)
            {
                var employee = salesDb.Employees.First(x => x.Name == name);
                for (var i = 0; i < 10000; i++)
                {
                    employee.Sales.Add(new Sale() { Total = random.Next(1000, 30000) });
                }
            }
            await salesDb.SaveChangesAsync(cancellationToken);
        }
    }
}
