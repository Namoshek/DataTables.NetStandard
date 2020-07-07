using AutoMapper;
using DataTables.NetStandard.Sample.DataTables;
using DataTables.NetStandard.Sample.DataTables.ViewModels;
using DataTables.NetStandard.TemplateMapper;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace DataTables.NetStandard.Sample
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddDbContext<SampleDbContext>(options =>
            {
                options.UseSqlServer(Configuration.GetConnectionString("Database"));
            });

            services.AddControllersWithViews();

            services.AddDataTablesTemplateMapper();
            services.AddScoped<PersonDataTable>();

            // We need to register custom services we want to use in the mapping profile.
            services.AddTransient<IViewRenderService, ViewRenderService>();

            // Building the service provider early to get the IViewRenderService is a hack that is necessary to get access 
            // to the Razor partial compiler in the DefaultMappingProfile. As the IViewRenderService depends on services
            // from Microsoft.AspNetCore.Mvc.Razor, it is necessary to configure MVC with services.AddMvc() before
            // configuring the Mapper like this.
            services.AddScoped(provider => new MapperConfiguration(config =>
            {
                config.AddProfile(new DefaultMappingProfile(provider.GetService<IViewRenderService>()));
            }).CreateMapper());
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, SampleDbContext dbContext)
        {
            dbContext.Database.Migrate();

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Persons/Error");
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Persons}/{action=Index}/{id?}");
            });
        }
    }
}
