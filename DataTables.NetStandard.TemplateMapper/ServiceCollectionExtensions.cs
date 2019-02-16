using Microsoft.Extensions.DependencyInjection;

namespace DataTables.NetStandard.TemplateMapper
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddDataTablesTemplateMapper(this IServiceCollection services)
        {
            return services.AddScoped<IViewRenderService, ViewRenderService>();
        }
    }
}
