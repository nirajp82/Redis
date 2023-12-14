using AspNetCoreRedis.DataModel;
using AspNetCoreRedis.Repository.Infrastructure;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using System.Diagnostics;

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

        //[HttpGet("top")]
        //public async Task<Dictionary<string, object>> GetTopSalesperson()
        //{
        //    var stopwatch = Stopwatch.StartNew();

        //    // TODO Section 3.2 step 4
        //    // add cache check here

        //    // end Section 3.2 step 4

        //    var topSalesperson = await _salesDb.Employees.Select(x => new
        //    {
        //        Employee = x,
        //        sumSales = x.Sales
        //        .Sum(x => x.Total)
        //    }).OrderByDescending(x => x.sumSales)
        //        .FirstAsync();
        //    stopwatch.Stop();

        //    // TODO Section 3.2 step 3
        //    // add cache insert here

        //    // End Section 3.2 step 3

        //    return new Dictionary<string, object>()
        //{
        //    { "sum_sales", topSalesperson.sumSales },
        //    { "employee_name", topSalesperson.Employee.Name },
        //    { "time", stopwatch.ElapsedMilliseconds }
        //};
        //}

        //[HttpGet("average/{id}")]
        //public async Task<Dictionary<string, double>> GetAverage([FromRoute] int id)
        //{
        //    var stopwatch = Stopwatch.StartNew();

        //    // TODO Section 3.2 step 5
        //    // add caching logic here

        //    // end Section 3.2 step 5

        //    var avg = await _salesDb.Employees.Include(x => x.Sales).Where(x => x.EmployeeId == id).Select(x => x.Sales.Average(y => y.Total)).FirstAsync();

        //    // TODO Section 3.2 step 6
        //    // add cache set here

        //    // end Section 3.2 step 6

        //    stopwatch.Stop();
        //    return new Dictionary<string, double>
        //{
        //    { "average", avg },
        //    { "elapsed", stopwatch.ElapsedMilliseconds }
        //};
        //}

        //[HttpGet("totalSales")]
        //public async Task<Dictionary<string, long>> GetTotalSales()
        //{
        //    var stopwatch = Stopwatch.StartNew();

        //    // TODO Section 3.2 step 7
        //    // add caching logic here

        //    // end Section 3.2 step 7

        //    var totalSales = await _salesDb.Sales.SumAsync(x => x.Total);

        //    // TODO Section 3.2 step 8
        //    // add cache set here

        //    // end Section 3.2 step 8

        //    stopwatch.Stop();
        //    return new Dictionary<string, long>()
        //{
        //    { "Total Sales", totalSales },
        //    { "elapsed", stopwatch.ElapsedMilliseconds }
        //};
        //}
    }
}
