using DataTables.NetStandard.ViewRenderer;
using Microsoft.Extensions.DependencyInjection;

namespace DataTables.NetStandard
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddDataTables(this IServiceCollection services)
        {
            return services.AddScoped<IViewRenderService, ViewRenderService>();
        }
    }
}
