using AspNetMvc.Cache;
using AspNetMvc.Models;
using AspNetMvc.Repository.Infrastructure;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Web.Caching;
using System.Web.Http;
using System.Web.Http.Description;

namespace AspNetMvc.Controllers
{
    public class EmployeeController : ApiController
    {
        private SalesContext _dbSalesContext = new SalesContext();

        // GET: api/Employees
        public IQueryable<Employee> GetEmployees()
        {
            return _dbSalesContext.Employees;
        }

        // GET: api/Employees/5
        [ResponseType(typeof(Employee))]
        public async Task<IHttpActionResult> GetEmployee(int id)
        {
            Employee employee = await _dbSalesContext.Employees.FindAsync(id);
            if (employee == null)
            {
                return NotFound();
            }

            return Ok(employee);
        }

        // PUT: api/Employees/5
        [ResponseType(typeof(void))]
        public async Task<IHttpActionResult> PutEmployee(int id, Employee employee)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (id != employee.EmployeeId)
            {
                return BadRequest();
            }

            _dbSalesContext.Entry(employee).State = EntityState.Modified;

            try
            {
                await _dbSalesContext.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!EmployeeExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return StatusCode(HttpStatusCode.NoContent);
        }

        // POST: api/Employees
        [ResponseType(typeof(Employee))]
        public async Task<IHttpActionResult> PostEmployee(Employee employee)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            _dbSalesContext.Employees.Add(employee);
            await _dbSalesContext.SaveChangesAsync();

            return CreatedAtRoute("DefaultApi", new { id = employee.EmployeeId }, employee);
        }

        // DELETE: api/Employees/5
        [ResponseType(typeof(Employee))]
        public async Task<IHttpActionResult> DeleteEmployee(int id)
        {
            Employee employee = await _dbSalesContext.Employees.FindAsync(id);
            if (employee == null)
            {
                return NotFound();
            }

            _dbSalesContext.Employees.Remove(employee);
            await _dbSalesContext.SaveChangesAsync();

            return Ok(employee);
        }

        [ResponseType(typeof(Dictionary<string, double>))]
        public async Task<IHttpActionResult> GetAverage(int id)
        {
            try
            {
                var stopwatch = Stopwatch.StartNew();
                var key = $"employee:{id}:avg";
                var dbCache = RedisCache.GetDatabase();
                var empAvg = (double?)await dbCache.StringGetSetExpiryAsync($"average:{id}", TimeSpan.FromHours(1));
                if (empAvg.HasValue)
                {
                    stopwatch.Stop();
                }
                else
                {
                    var avg = await _dbSalesContext.Employees.Include(x => x.Sales)
                                .Where(dbe => dbe.EmployeeId == id)
                                .Select(x => x.Sales.Average(dbs => dbs.Total))
                                .FirstOrDefaultAsync();
                    await dbCache.StringSetAsync(key, avg.ToString(CultureInfo.InvariantCulture), TimeSpan.FromHours(1));
                    empAvg = avg;
                    stopwatch.Stop();
                }
                var result = new Dictionary<string, double>
                {
                    {"average", empAvg.GetValueOrDefault() },
                    {"elapsed", stopwatch.ElapsedMilliseconds }
                };
                return Ok(result);
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }

        [ResponseType(typeof(Dictionary<string, long>))]
        public async Task<IHttpActionResult> GetTotalSales()
        {
            var stopwatch = Stopwatch.StartNew();
            string topEmpName = string.Empty;
            string topEmpSales = string.Empty;
            var dbCache = RedisCache.GetDatabase();

            var topName = dbCache.StringGetAsync("top:name");
            var topSales = dbCache.StringGetAsync("top:sales");
            await Task.WhenAll(topName, topSales);

            if (!string.IsNullOrWhiteSpace(topName.Result) && !string.IsNullOrWhiteSpace(topSales.Result))
            {
                topEmpName = topName.Result;
                topEmpSales = topSales.Result;
            }
            else
            {
                var topSalesperson = await _dbSalesContext.Employees.Select(dbe => new
                {
                    Employee = dbe,
                    sumSales = dbe.Sales.Sum(x => x.Total)
                }).OrderByDescending(x => x.sumSales).FirstAsync();

                stopwatch.Stop();

                topEmpName = topSalesperson.Employee.Name;
                topEmpSales = topSalesperson.sumSales.ToString();

                var topNameInsert = dbCache.StringSetAsync("top:name", topEmpName, TimeSpan.FromMinutes(5));
                var topSalesInsert = dbCache.StringSetAsync("top:sales", topEmpSales, TimeSpan.FromMinutes(5));
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

        [ResponseType(typeof(Dictionary<string, object>))]
        public async Task<IHttpActionResult> GetTopSalesperson()
        {
            var stopwatch = Stopwatch.StartNew();
            string topEmpName = string.Empty;
            string topEmpSales = string.Empty;
            var dbCache = RedisCache.GetDatabase();

            var topName = dbCache.StringGetAsync("top:name");
            var topSales = dbCache.StringGetAsync("top:sales");
            await Task.WhenAll(topName, topSales);

            if (!string.IsNullOrWhiteSpace(topName.Result) && !string.IsNullOrWhiteSpace(topSales.Result))
            {
                topEmpName = topName.Result;
                topEmpSales = topSales.Result;
            }
            else
            {
                var topSalesperson = await _dbSalesContext.Employees.Select(e => new
                {
                    Employee = e,
                    sumSales = e.Sales.Sum(s => s.Total)
                }).OrderByDescending(x => x.sumSales)
                  .FirstAsync();

                stopwatch.Stop();

                topEmpName = topSalesperson.Employee.Name;
                topEmpSales = topSalesperson.sumSales.ToString();

                var topNameInsert = dbCache.StringSetAsync("top:name", topEmpName, TimeSpan.FromHours(1));
                var topSalesInsert = dbCache.StringSetAsync("top:sales", topEmpSales, TimeSpan.FromHours(1));
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

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _dbSalesContext.Dispose();
            }
            base.Dispose(disposing);
        }

        private bool EmployeeExists(int id)
        {
            return _dbSalesContext.Employees.Count(e => e.EmployeeId == id) > 0;
        }
    }
}
