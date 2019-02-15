using System.Threading.Tasks;

namespace DataTables.NetStandard.ViewRenderer
{
    public interface IViewRenderService
    {
        Task<string> RenderRazorToStringAsync(string viewName, object model);
    }
}
