using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using Microsoft.AspNetCore.Mvc.ViewFeatures;

namespace DataTables.NetCore.Util
{
    public static class ViewRenderHelper
    {
        public static async Task<string> RenderViewToStringAsync<TModel>(this Controller controller, string viewName, TModel model, bool partial = false)
        {
            if (string.IsNullOrEmpty(viewName))
            {
                viewName = controller.ControllerContext.ActionDescriptor.ActionName;
            }

            var viewData = new ViewDataDictionary(controller.ViewData);
            viewData.Model = model;

            using (var writer = new StringWriter())
            {
                IViewEngine viewEngine = controller.HttpContext.RequestServices.GetService(typeof(ICompositeViewEngine)) as ICompositeViewEngine;

                var actionContext = new ActionContext(controller.HttpContext, new Microsoft.AspNetCore.Routing.RouteData(), 
                    new Microsoft.AspNetCore.Mvc.Abstractions.ActionDescriptor());

                var viewResult = viewEngine.FindView(actionContext, viewName, !partial);
                if (viewResult.Success == false)
                {
                    return $"A view with the name {viewName} could not be found.";
                }

                var viewContext = new ViewContext(
                    actionContext,
                    viewResult.View,
                    viewData,
                    controller.TempData,
                    writer,
                    new HtmlHelperOptions()
                );

                await viewResult.View.RenderAsync(viewContext);

                return writer.GetStringBuilder().ToString();
            }
        }
    }
}
