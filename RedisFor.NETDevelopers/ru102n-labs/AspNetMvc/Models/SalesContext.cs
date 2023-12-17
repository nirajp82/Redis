using System;
using System.Data.Entity;
using System.Data.Entity.ModelConfiguration.Conventions;

namespace AspNetMvc.Models
{
    public class SalesContext : DbContext
    {
        public DbSet<Employee> Employees { get; set; }

        public DbSet<Sale> Sales { get; set; }

        public SalesContext() : base("SalesContext")
        {            
        }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Conventions.Remove<PluralizingTableNameConvention>();
        }

    }
}