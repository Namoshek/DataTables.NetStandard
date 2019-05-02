using AutoMapper;
using DataTables.NetStandard.Sample.DataTables;
using DataTables.NetStandard.Sample.DataTables.ViewModels;
using DataTables.NetStandard.TemplateMapper;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

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
                options.UseSqlServer("Server=(localdb)\\mssqllocaldb;Database=DataTables.NetStandard.Sample;Trusted_Connection=True;");
            });

            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_2);

            services.AddDataTablesTemplateMapper();
            services.AddScoped<PersonDataTable>();

            // Building the service provider early to get the IViewRenderService is a hack that is necessary to get access 
            // to the Razor partial compiler in the DefaultMappingProfile. As the IViewRenderService depends on services
            // from Microsoft.AspNetCore.Mvc.Razor, it is necessary to configure MVC with services.AddMvc() before
            // configuring the Mapper like this.
            Mapper.Initialize(m =>
            {
                m.AddProfile(new DefaultMappingProfile(services.BuildServiceProvider().GetService<IViewRenderService>()));
            });
        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
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

            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller=Persons}/{action=Index}/{id?}");
            });
        }
    }
}
