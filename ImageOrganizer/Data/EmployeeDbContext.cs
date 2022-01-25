using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace ImageOrganizer.Data
{
    public class EmployeeDbContext : DbContext
    {
        #region Contructor
        public EmployeeDbContext(DbContextOptions<EmployeeDbContext> options) : base(options)
        {
            Database.EnsureCreated();
        }
        #endregion
        #region Public properties
        public DbSet<Employee> Employee { get; set; }
        #endregion
        #region Overridden method
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Employee>().HasData(GetEmployees());
            base.OnModelCreating(modelBuilder);
        }
        #endregion
        #region Private method
        private List<Employee> GetEmployees()
        {
            return new List<Employee>
        {
            new Employee {Id = 100, FirstName ="John", LastName = "Doe"},
            new Employee {Id = 101, FirstName ="Nicole", LastName = "Martha"},
            new Employee {Id = 102, FirstName ="Steve", LastName = "Johnson"},
            new Employee {Id = 103, FirstName ="Thomas", LastName = "Bond"},
        };
        }
        #endregion
    }
}
