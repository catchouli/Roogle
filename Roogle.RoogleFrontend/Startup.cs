using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Roogle.RoogleSpider.Db;
using System.IO;
using Roogle.RoogleFrontend.Services;

namespace Roogle.RoogleFrontend
{
  public class Startup
  {
    public Startup(IConfiguration configuration)
    {
      Configuration = configuration;
    }

    public IConfiguration Configuration { get; }

    // This method gets called by the runtime. Use this method to add services to the container.
    public void ConfigureServices(IServiceCollection services)
    {
      // Add mysql
      var connString = Configuration.GetConnectionString("DefaultConnection");
      var dbUser = File.ReadAllText("/run/secrets/roogle_db_user");
      var dbPass = File.ReadAllText("/run/secrets/roogle_db_pass");
      connString = $"{connString};user={dbUser};password={dbPass}";
      services.AddDbContext<RoogleSpiderDbContext>(options =>
        options.UseMySql(connString, ServerVersion.AutoDetect(connString)));

      services.AddScoped<ISearchService, MySQLSearchService>();

      services.AddControllersWithViews();
    }

    // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
    public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
    {
      if (env.IsDevelopment())
      {
        app.UseDeveloperExceptionPage();
      }
      else
      {
        app.UseExceptionHandler("/Home/Error");
      }
      app.UseStaticFiles();

      app.UseRouting();

      app.UseAuthorization();

      app.UseEndpoints(endpoints =>
      {
        endpoints.MapControllerRoute(
                  name: "default",
                  pattern: "{controller=Home}/{action=Index}/{id?}");
      });
    }
  }
}
