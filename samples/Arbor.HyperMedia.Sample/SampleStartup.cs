using Arbor.AppModel.Mvc;
using Arbor.Hypermedia;

namespace Arbor.HyperMedia.Sample;

public class SampleStartup
{
    public void ConfigureServices(IServiceCollection services)
    {
        services.UseHypermedia();
    }

    public void Configure(IApplicationBuilder app, IWebHostEnvironment _)
    {
        app.UseRouting();

        app.UseEndpoints(builder => builder.MapControllers());
    }
}