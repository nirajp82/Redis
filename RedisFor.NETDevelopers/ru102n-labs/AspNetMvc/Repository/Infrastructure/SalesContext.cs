using AspNetMvc.Models;
using System.Data.Entity;
using System.Data.Entity.ModelConfiguration.Conventions;

namespace AspNetMvc.Repository.Infrastructure
{
    public class SalesContext : DbContext
    {
        public DbSet<Employee> Employees { get; set; }
        public DbSet<Sale> Sales { get; set; }
        public string DbPath { get; set; }

        public SalesContext()
        {
        }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Conventions.Remove<PluralizingTableNameConvention>();
        }
    }
}