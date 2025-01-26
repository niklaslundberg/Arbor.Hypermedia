using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;

namespace Arbor.Hypermedia.Tests
{
    public class TestStartup
    {
        public void ConfigureServices(IServiceCollection services) => services.UseHypermedia();

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            app.UseRouting();

            app.UseEndpoints(builder => builder.MapControllers());
        }
    }
}