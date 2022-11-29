using Arbor.AppModel.Mvc;
using Arbor.Hypermedia;
using Arbor.Hypermedia.Tests;

namespace Arbor.HyperMedia.Sample;

public class SampleStartup
{
    public void ConfigureServices(IServiceCollection services)
    {
        services.AddSingleton<DataStore>();
        services.UseHypermedia();
    }

    public void Configure(IApplicationBuilder app, IWebHostEnvironment _)
    {
        app.UseDeveloperExceptionPage();

        app.UseHttpMethodOverride(new() { FormFieldName = "_method" });

        app.UseRouting();

        app.UseEndpoints(builder => builder.MapControllers());
    }
}