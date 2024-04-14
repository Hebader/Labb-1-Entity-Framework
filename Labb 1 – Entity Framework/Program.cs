using Labb_1___Entity_Framework.Data;
using Labb_1___Entity_Framework.Models;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;

internal class Program
{
    private static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);
        var connectionString = builder.Configuration.GetConnectionString("SchoolConnection") ?? throw new InvalidOperationException("Connection string 'SchoolConnection' not found.");
        builder.Services.AddDbContext<LeaveManagementDbContext>(options =>
            options.UseSqlServer(connectionString));



        // Lägg till MVC-tjänster
        builder.Services.AddControllersWithViews();

        var app = builder.Build();




        // Configure the HTTP request pipeline.
        if (!app.Environment.IsDevelopment())
        {
            app.UseExceptionHandler("/Home/Error");
            // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
            app.UseHsts();
        }

        app.UseHttpsRedirection();
        app.UseStaticFiles();

        app.UseRouting();

        ////app.UseAuthorization();

        app.MapControllerRoute(
            name: "default",
            pattern: "{controller=Home}/{action=Index}/{id?}");

        // Hämta databaskontexten
        using (var scope = app.Services.CreateScope())
        {
            var services = scope.ServiceProvider;

            try
            {
                // Hämta databaskontexten
                var dbContext = services.GetRequiredService<LeaveManagementDbContext>();

                // Kör SeedData-metoden för att fylla i databasen med testdata
                SeedData(dbContext);
            }
            catch (Exception ex)
            {
                // Hantera eventuella fel
                var logger = services.GetRequiredService<ILogger<Program>>();
                logger.LogError(ex, "Ett fel uppstod vid seedning av databasen.");
            }
        }

        app.Run();
    }
    public static void SeedData(LeaveManagementDbContext dbContext)
    {
        // Kontrollera om SeedData redan har körts genom att använda en variabel
        bool isSeeded = dbContext.Employees.Any(); // Om det finns anställda antas SeedData ha körts

        // Om SeedData inte har körts
        if (!isSeeded)
        {
            // Lägg till anställda
            var employee1 = new Employee { EmployeeName = "John Doe", Department = "HR" };
            var employee2 = new Employee { EmployeeName = "Jane Smith", Department = "IT" };
            var employee3 = new Employee { EmployeeName = "John A", Department = "HR" };
            dbContext.Employees.AddRange(employee1, employee2, employee3);
            dbContext.SaveChanges();

            // Skapa ledighetstyper
            var leaveType1 = new LeaveType { LeaveTypeName = "Vacation" };
            var leaveType2 = new LeaveType { LeaveTypeName = "Sick" };
            dbContext.LeaveTypes.AddRange(leaveType1, leaveType2);
            dbContext.SaveChanges();

            // Skapa ledighetsansökningar och koppla dem till anställda och ledighetstyper
            var leaveApplication1 = new LeaveApplication { FkEmployeeId = employee1.EmployeeId, FkLeaveTypeId = leaveType1.LeaveTypeId, StartDate = DateTime.Now.AddDays(5), EndDate = DateTime.Now.AddDays(10), CreatedAt = DateTime.Now };
            var leaveApplication2 = new LeaveApplication { FkEmployeeId = employee2.EmployeeId, FkLeaveTypeId = leaveType2.LeaveTypeId, StartDate = DateTime.Now.AddDays(3), EndDate = DateTime.Now.AddDays(7), CreatedAt = DateTime.Now };
            dbContext.LeaveApplications.AddRange(leaveApplication1, leaveApplication2);
            dbContext.SaveChanges();


        }
    }
}
