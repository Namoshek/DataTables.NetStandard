using System.Threading.Tasks;

namespace DataTables.NetCore.ViewRenderer
{
    public interface IViewRenderService
    {
        Task<string> RenderToStringAsync(string viewName, object model);
    }
}
