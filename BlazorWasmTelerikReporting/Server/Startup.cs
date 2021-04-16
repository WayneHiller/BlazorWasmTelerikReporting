using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using System.IO;
using System.Linq;
using Telerik.Reporting.Cache.File;
using Telerik.Reporting.Services;

namespace BlazorWasmTelerikReporting.Server
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {

            services.AddControllersWithViews();
            services.AddRazorPages()
                    .AddNewtonsoftJson();

            services.TryAddSingleton<IReportServiceConfiguration>(sp => new ReportServiceConfiguration
            {
                ReportingEngineConfiguration = ReportingConfigurationHelper.ResolveConfiguration(sp.GetService<IWebHostEnvironment>()),
                Storage = new FileStorage(),
                ReportSourceResolver = new UriReportSourceResolver(
                                    Path.Combine(sp.GetService<IWebHostEnvironment>()?.ContentRootPath ?? string.Empty, "Reports"))
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if(env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseWebAssemblyDebugging();
            }
            else
            {
                app.UseExceptionHandler("/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseBlazorFrameworkFiles();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseEndpoints(endpoints => {
                endpoints.MapRazorPages();
                endpoints.MapControllers();
                endpoints.MapFallbackToFile("index.html");
            });
        }
    }

    static class ReportingConfigurationHelper
    {
        public static IConfiguration ResolveConfiguration(IWebHostEnvironment? environment)
        {
            var cfg = Path.Combine(environment?.ContentRootPath ?? string.Empty, "reportingAppSettings.json");
            return new ConfigurationBuilder()
                .AddJsonFile(cfg, true)
                .Build();
        }
    }
}
