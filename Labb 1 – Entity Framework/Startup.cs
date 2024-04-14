using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

public class Startup
{
    public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
    {
        if (env.IsDevelopment())
        {
            app.UseDeveloperExceptionPage();
        }
        else
        {
            // Konfigurera för produktionsmiljö
            app.UseExceptionHandler("/Home/Error");
            app.UseHsts();
        }

        app.UseHttpsRedirection();
        app.UseStaticFiles();

        app.UseRouting();

        app.UseAuthorization();

        app.UseEndpoints(endpoints =>
        {
            endpoints.MapControllerRoute(
                name: "EmployeeLeaveApplications",
                pattern: "LeaveApplications/EmployeeLeaveApplications/{FkEmployeeId}",
                defaults: new { controller = "LeaveApplications", action = "EmployeeLeaveApplications" }
            );

            // Här inkluderar du dina övriga ruttningsregler för att de ska vara tillgängliga i både utvecklings- och produktionsmiljöer
            endpoints.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}");
        });
    }

}
