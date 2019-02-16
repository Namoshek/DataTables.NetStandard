using System.Threading.Tasks;

namespace DataTables.NetStandard.TemplateMapper
{
    public interface IViewRenderService
    {
        /// <summary>
        /// Renders a Razor view as string asynchronously. Can be used outside of controller context.
        /// </summary>
        /// <param name="viewName">Name of the view.</param>
        /// <param name="model">The model.</param>
        Task<string> RenderRazorToStringAsync(string viewName, object model);
    }
}
