using Arbor.AppModel.Mvc;
using Arbor.Hypermedia;

namespace Arbor.HyperMedia.Sample;

public class SampleStartup
{
    public void ConfigureServices(IServiceCollection services)
    {
        //services.AddControllers();

        //services.AddEndpointsApiExplorer();
        //services.AddSwaggerGen();

        //services.AddMvc(options => options.Filters.Add<ValidationActionFilter>());

        services.UseHypermedia();
    }

    public void Configure(IApplicationBuilder app, IWebHostEnvironment _)
    {
        app.UseRouting();

        //app.UseSwagger();

        app.UseEndpoints(builder => builder.MapControllers());
    }
}