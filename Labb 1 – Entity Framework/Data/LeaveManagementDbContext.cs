using Microsoft.EntityFrameworkCore;
using Labb_1___Entity_Framework.Models;
using Microsoft.VisualStudio.Web.CodeGeneration.EntityFrameworkCore;


namespace Labb_1___Entity_Framework.Data
{
    public class LeaveManagementDbContext : DbContext
    {
        public LeaveManagementDbContext(DbContextOptions<LeaveManagementDbContext> options) : base(options)
        {
        }
        public DbSet<Employee> Employees { get; set; }
        public DbSet<LeaveApplication> LeaveApplications { get; set; }
        public DbSet<LeaveType> LeaveTypes { get; set; }
        

    }
}