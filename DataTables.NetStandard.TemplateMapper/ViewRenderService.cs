using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Routing;
using Scriban;

namespace DataTables.NetStandard.TemplateMapper
{
    public class ViewRenderService : IViewRenderService
    {
        protected readonly IRazorViewEngine _razorViewEngine;
        protected readonly ITempDataProvider _tempDataProvider;
        protected readonly IServiceProvider _serviceProvider;

        public ViewRenderService(IRazorViewEngine razorViewEngine,
            ITempDataProvider tempDataProvider,
            IServiceProvider serviceProvider)
        {
            _razorViewEngine = razorViewEngine;
            _tempDataProvider = tempDataProvider;
            _serviceProvider = serviceProvider;
        }

        /// <summary>
        /// Renders a Razor view as string asynchronously. Can be used outside of controller context.
        /// </summary>
        /// <param name="viewName">Name of the view.</param>
        /// <param name="model">The model.</param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        public async Task<string> RenderRazorToStringAsync(string viewName, object model)
        {
            if (string.IsNullOrWhiteSpace(viewName))
            {
                throw new ArgumentException($"{nameof(viewName)} cannot be null or whitespace.");
            }

            var httpContext = new DefaultHttpContext { RequestServices = _serviceProvider };
            var actionContext = new ActionContext(httpContext, new RouteData(), new ActionDescriptor());

            using (var sw = new StringWriter())
            {
                var viewResult = _razorViewEngine.FindView(actionContext, viewName, false);
                if (!viewResult.Success || viewResult.View == null)
                {
                    throw new ArgumentNullException($"{viewName} does not match any available view.");
                }

                var viewDictionary = new ViewDataDictionary(new EmptyModelMetadataProvider(), new ModelStateDictionary())
                {
                    Model = model
                };

                var viewContext = new ViewContext(
                    actionContext,
                    viewResult.View,
                    viewDictionary,
                    new TempDataDictionary(actionContext.HttpContext, _tempDataProvider),
                    sw,
                    new HtmlHelperOptions()
                );

                await viewResult.View.RenderAsync(viewContext);

                return sw.ToString();
            }
        }

        /// <summary>
        /// Renders a liquid template file using the given data to a string.
        /// </summary>
        /// <param name="viewName">Name of the view.</param>
        /// <param name="model">The model.</param>
        public static string RenderLiquidTemplateFileWithData(string viewName, object model)
        {
            var view = File.ReadAllText(Directory.GetCurrentDirectory() + "/Views/" + viewName);

            return RenderLiquidTemplateWithData(view, model);
        }

        /// <summary>
        /// Renders a liquid template using the given data to a string.
        /// </summary>
        /// <param name="template">The template.</param>
        /// <param name="model">The model.</param>
        /// <returns></returns>
        public static string RenderLiquidTemplateWithData(string template, object model)
        {
            var parsedTemplate = Template.Parse(template);

            return parsedTemplate.Render(model);
        }
    }
}
