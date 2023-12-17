using AspNetCoreRedis.DataModel;
using AspNetCoreRedis.Repository.Infrastructure;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using System.Diagnostics;
using System.Globalization;

namespace AspNetCoreRedis.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EmployeeController : ControllerBase
    {
        private readonly SalesContext _salesDb;
        private readonly IDistributedCache _cache;

        public EmployeeController(SalesContext salesContext, IDistributedCache cache)
        {
            _salesDb = salesContext;
            _cache = cache;
        }

        [HttpGet("all")]
        [ProducesResponseType(typeof(IEnumerable<Object>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetEmployees()
        {
            var employees = await _salesDb.Employees.Select(emp =>
                            new
                            {
                                employee = emp,
                                sumSales = emp.Sales.Sum(s => s.Total)
                            }).OrderByDescending(e => e.sumSales).ToListAsync();
            if (employees?.Any() == true)
                return Ok(employees);
            else
                return NoContent();
        }

        [HttpGet("top")]
        [ProducesResponseType(typeof(Dictionary<string, string>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetTopSalesperson()
        {
            var stopwatch = Stopwatch.StartNew();
            string topEmpName = string.Empty;
            string topEmpSales = string.Empty;

            var topName = _cache.GetStringAsync("top:name");
            var topSales = _cache.GetStringAsync("top:sales");
            await Task.WhenAll(topName, topSales);

            if (!string.IsNullOrWhiteSpace(topName.Result) && !string.IsNullOrWhiteSpace(topSales.Result))
            {
                topEmpName = topName.Result;
                topEmpSales = topSales.Result;
            }
            else
            {
                var topSalesperson = await _salesDb.Employees.Select(x => new
                {
                    Employee = x,
                    sumSales = x.Sales.Sum(x => x.Total)
                }).OrderByDescending(x => x.sumSales)
                  .FirstAsync();

                stopwatch.Stop();

                topEmpName = topSalesperson.Employee.Name;
                topEmpSales = topSalesperson.sumSales.ToString();

                var cacheOptions = new DistributedCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5),
                };
                var topNameInsert = _cache.SetStringAsync("top:name", topEmpName, cacheOptions);
                var topSalesInsert = _cache.SetStringAsync("top:sales", topEmpSales, cacheOptions);
                await Task.WhenAll(topSalesInsert, topNameInsert);
            }
            var result = new Dictionary<string, string>()
                            {
                                { "employee_name", topEmpName},
                                { "sum_sales",  topEmpSales},
                                { "time", stopwatch.ElapsedMilliseconds.ToString() }
                            };
            return Ok(result);
        }

        [HttpGet("average/{empId}")]
        [ProducesResponseType(typeof(Dictionary<string, double>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetAverage([FromRoute] int empId)
        {
            var stopwatch = Stopwatch.StartNew();
            var key = $"employee:{empId}:avg";
            var empAvg = await _cache.GetStringAsync(key) ?? "";
            if (!string.IsNullOrWhiteSpace(empAvg))
            {
                stopwatch.Stop();
            }
            else
            {
                var avg = await _salesDb.Employees.Include(x => x.Sales)
                            .Where(dbe => dbe.EmployeeId == empId)
                            .Select(x => x.Sales.Average(dbs => dbs.Total))
                            .FirstOrDefaultAsync();
                await _cache.SetStringAsync(key, avg.ToString(CultureInfo.InvariantCulture),
                     new DistributedCacheEntryOptions
                     { SlidingExpiration = TimeSpan.FromMinutes(30) });
                empAvg = avg.ToString();
                stopwatch.Stop();
            }
            var result = new Dictionary<string, double>
            {
                {"average", double.Parse(empAvg) },
                {"elapsed", stopwatch.ElapsedMilliseconds }
            };
            return Ok(result);
        }

        [HttpGet("totalSales")]
        [ProducesResponseType(typeof(Dictionary<string, long>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetTotalSales()
        {
            var stopwatch = Stopwatch.StartNew();
            long totalSales = 0;
            var cacheResult = await _cache.GetStringAsync("totalSales");
            if (!string.IsNullOrWhiteSpace(cacheResult))
            {
                totalSales = long.Parse(cacheResult);
                stopwatch.Stop();
            }
            else
            {
                totalSales = await _salesDb.Sales.SumAsync(dbs => dbs.Total);
                await _cache.SetStringAsync("totalSales", totalSales.ToString(), new DistributedCacheEntryOptions
                {
                    AbsoluteExpiration = DateTime.Today.AddDays(1)
                });
                stopwatch.Stop();
            }

            var result = new Dictionary<string, long>()
            {
                { "Total Sales", totalSales },
                { "elapsed", stopwatch.ElapsedMilliseconds }
            };
            return Ok(result);
        }
    }
}
