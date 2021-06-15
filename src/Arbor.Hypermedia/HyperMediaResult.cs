using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.Extensions.DependencyInjection;

namespace Arbor.Hypermedia
{
    public class HyperMediaResult
    {
        private readonly HyperMediaBuilder _hyperMediaBuilder;

        public HyperMediaResult(HyperMediaBuilder hyperMediaBuilder) => _hyperMediaBuilder = hyperMediaBuilder;

        public async Task<IActionResult> ToHyperMediaResult<T>(Controller controller, T? identifiable)
            where T : IMetadata
        {
            if (identifiable is null)
            {
                return new NotFoundResult();
            }

            IViewEngine viewEngine = controller.HttpContext.RequestServices.GetRequiredService<ICompositeViewEngine>();

            const string viewPath = "~/Views/Shared/HyperMediaLayout.cshtml";

            var viewResult = viewEngine.GetView(viewPath, viewPath, false);

            var entityMetadata = identifiable.CreateMetadata();

            await using TextWriter writer = new StreamWriter(controller.HttpContext.Response.Body, leaveOpen: true);

            var urlResolver = new UrlResolver(controller.Url);

            controller.ViewData.Model = await _hyperMediaBuilder.GetControl(entityMetadata, urlResolver);

            ViewContext viewContext =
                new(controller.ControllerContext, viewResult.View, controller.ViewData, controller.TempData, writer,
                    new HtmlHelperOptions());

            await viewResult.View.RenderAsync(viewContext);

            await writer.FlushAsync();

            return new StatusCodeResult(StatusCodes.Status200OK);
        }
    }
}