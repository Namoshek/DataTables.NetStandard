using System.Threading.Tasks;

namespace DataTables.NetCore.ViewRenderer
{
    public interface IViewRenderService
    {
        Task<string> RenderRazorToStringAsync(string viewName, object model);
    }
}
