using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.DependencyInjection;

namespace Arbor.Hypermedia
{
    public class HtmlHypermediaFormatter : IOutputFormatter
    {
        private static readonly RouteData EmptyRouteData = new();
        private static readonly ActionDescriptor EmptyActionDescriptor = new();

        public bool CanWriteResult(OutputFormatterCanWriteContext context) => context.Object is IMetadata ||
                                                                              (context.ObjectType is { } type &&
                                                                               type.IsAssignableTo(typeof(IMetadata)));

        public async Task WriteAsync(OutputFormatterWriteContext context)
        {
            if (context.Object is not IMetadata metadataEntity)
            {
                //throw new InvalidOperationException("Invalid meta data type");
                context.HttpContext.Response.StatusCode = StatusCodes.Status404NotFound;
                return;
            }

            //var viewResultExecutor = context.HttpContext.RequestServices.GetRequiredService<ViewResultExecutor>();

            var routeData = context.HttpContext.GetRouteData() ?? EmptyRouteData;

            IServiceProvider serviceProvider = context.HttpContext.RequestServices;

            IViewEngine viewEngine = serviceProvider.GetRequiredService<ICompositeViewEngine>();

            const string viewPath = "~/Views/Shared/HyperMediaLayout.cshtml";

            var viewResult = viewEngine.GetView(viewPath, viewPath, false);

            if (viewResult.View is null)
            {
                throw new InvalidOperationException($"Could not find view {viewPath}");
            }

            var entityMetadata = metadataEntity.CreateMetadata();
            var actionContextAccessor = serviceProvider.GetRequiredService<IActionContextAccessor>();

            actionContextAccessor.ActionContext ??= new ActionContext(context.HttpContext, routeData, EmptyActionDescriptor);

            var uriHelper = serviceProvider.GetRequiredService<IUrlHelperFactory>();
            HyperMediaBuilder builder = serviceProvider.GetRequiredService<HyperMediaBuilder>();

            var urlResolver = new UrlResolver(uriHelper.GetUrlHelper(actionContextAccessor.ActionContext));

            var model = await builder.GetControl(entityMetadata, urlResolver);

            var provider = serviceProvider.GetRequiredService<ITempDataProvider>();

            await using TextWriter writer =
                new HttpResponseStreamWriter(context.HttpContext.Response.Body, Encoding.UTF8);

            var viewContext = new ViewContext(actionContextAccessor.ActionContext,
                viewResult.View,
                new ViewDataDictionary<HyperMediaEntity>(new EmptyModelMetadataProvider(), new ModelStateDictionary())
                {
                    Model = model
                },
                new TempDataDictionary(context.HttpContext, provider),
                writer,
                new HtmlHelperOptions());

            await viewResult.View.RenderAsync(viewContext);
        }
    }
}