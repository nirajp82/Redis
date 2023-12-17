using System;
using System.Collections.Generic;
using System.Data.Entity.Migrations;
using System.Linq;
using System.Web;
using AspNetMvc.Models;
using AspNetMvc.Repository.Infrastructure;

namespace AspNetMvc.Repository
{
    internal sealed class Configuration : DbMigrationsConfiguration<SalesContext>
    {
        public Configuration()
        {
            AutomaticMigrationsEnabled = false;
            AutomaticMigrationDataLossAllowed = true;
        }

        protected override void Seed(SalesContext context)
        {
            Console.WriteLine("Seeding. . .");
            var names = new[] { "Alice", "Bob", "Carlos", "Dan", "Yves" };
            var random = new Random();

            foreach (var name in names)
            {
                var employee = new Employee { Name = name };
                context.Employees.Add(employee);
            }

            Console.WriteLine("Saving Employees");
            context.SaveChanges();

            foreach (var name in names)
            {
                var employee = context.Employees.First(x => x.Name == name);
                for (var i = 0; i < 10000; i++)
                {
                    employee.Sales.Add(new Sale { Total = random.Next(1000, 30000) });
                }

                context.SaveChanges();
            }
        }
    }
}