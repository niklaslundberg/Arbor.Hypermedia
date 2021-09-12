using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.DependencyInjection;

namespace Arbor.Hypermedia
{
    public class HtmlHyperMediaFormatter: IOutputFormatter
    {
        private ITempDataProvider _tempDataProvider;
        public HtmlHyperMediaFormatter(ITempDataProvider tempDataProvider)
        {
            _tempDataProvider = tempDataProvider;
        }
        private static readonly RouteData EmptyRouteData = new RouteData();
        private static readonly ActionDescriptor EmptyActionDescriptor = new ActionDescriptor();
        public bool CanWriteResult(OutputFormatterCanWriteContext context)
        {
            return context.Object is IMetadata;
        }

        public async Task WriteAsync(OutputFormatterWriteContext context)
        {
            if (context.Object is not IMetadata metadataEntity)
            {
                throw new InvalidOperationException("Invalid meta data type");
            }

            //var viewResultExecutor = context.HttpContext.RequestServices.GetRequiredService<ViewResultExecutor>();


            var routeData = context.HttpContext.GetRouteData() ?? EmptyRouteData;
            var actionContext = new ActionContext(context.HttpContext, routeData, EmptyActionDescriptor);


            IViewEngine viewEngine = context.HttpContext.RequestServices.GetRequiredService<ICompositeViewEngine>();

            const string viewPath = "~/Views/Shared/HyperMediaLayout.cshtml";

            var viewResult = viewEngine.GetView(viewPath, viewPath, isMainPage: false);

            if (viewResult.View is null)
            {
                throw new InvalidOperationException($"Could not find view {viewPath}");
            }

            var entityMetadata = metadataEntity.CreateMetadata();

            var uriHelper = context.HttpContext.RequestServices.GetRequiredService<IUrlHelper>();
            HyperMediaBuilder builder = context.HttpContext.RequestServices.GetRequiredService<HyperMediaBuilder>();

            var urlResolver = new UrlResolver(uriHelper);

            var model = await builder.GetControl(entityMetadata, urlResolver);

            await using TextWriter writer = new HttpResponseStreamWriter(context.HttpContext.Response.Body, Encoding.UTF8);
            var viewContext = new ViewContext(
                actionContext,
                viewResult.View,
                new ViewDataDictionary<HyperMediaEntity>(
                    metadataProvider: new EmptyModelMetadataProvider(),
                    modelState: new ModelStateDictionary())
                {
                    Model = model
                },
                new TempDataDictionary(
                    context.HttpContext,
                    _tempDataProvider),
                writer,
                new HtmlHelperOptions());

            await viewResult.View.RenderAsync(viewContext);
        }
    }
}