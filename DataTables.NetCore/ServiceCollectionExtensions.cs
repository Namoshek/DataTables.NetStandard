using DataTables.NetCore.ViewRenderer;
using Microsoft.Extensions.DependencyInjection;

namespace DataTables.NetCore
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddDataTables(this IServiceCollection services)
        {
            return services.AddScoped<IViewRenderService, ViewRenderService>();
        }
    }
}
