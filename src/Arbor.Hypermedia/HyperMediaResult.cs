using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using Microsoft.Extensions.DependencyInjection;

namespace Arbor.Hypermedia
{
    public class HyperMediaResult
    {
        private readonly HyperMediaBuilder _hyperMediaBuilder;

        public HyperMediaResult(HyperMediaBuilder hyperMediaBuilder) => _hyperMediaBuilder = hyperMediaBuilder;

        public async Task<IActionResult> ToHyperMediaResult<T>(Controller controller, T? metadataEntity)
            where T : IMetadata
        {
            if (metadataEntity is null)
            {
                return new NotFoundResult();
            }

            IViewEngine viewEngine = controller.HttpContext.RequestServices.GetRequiredService<ICompositeViewEngine>();

            const string viewPath = "~/Views/Shared/HyperMediaLayout.cshtml";

            var viewResult = viewEngine.GetView(viewPath, viewPath, isMainPage: false);

            if (viewResult.View is null)
            {
                throw new InvalidOperationException($"Could not find view {viewPath}");
            }

            var entityMetadata = metadataEntity.CreateMetadata();

            var urlResolver = new UrlResolver(controller.Url);

            var model = await _hyperMediaBuilder.GetControl(entityMetadata, urlResolver);

            return controller.View(viewResult.ViewName, model);
        }
    }
}