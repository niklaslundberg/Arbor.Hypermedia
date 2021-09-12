using Microsoft.AspNetCore.Mvc.ApplicationParts;
using Microsoft.AspNetCore.Mvc.Razor.RuntimeCompilation;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;

namespace Arbor.Hypermedia
{
    public static class HypermediaRegistrationExtensions
    {
        public static IServiceCollection UseHypermedia(this IServiceCollection services)
        {
            services.AddScoped<HyperMediaBuilder>();
            services.AddScoped<HyperMediaResult>();

            var thisAssembly = typeof(HypermediaRegistrationExtensions).Assembly;

            services.AddMvc(options =>
                {
                    //options.OutputFormatters.Add(new HtmlHypermediaFormatter()); // TODO make formatter use dependencies
                })

                .ConfigureApplicationPartManager(
                    applicationPartManager =>
                        applicationPartManager.ApplicationParts.Add(new AssemblyPart(thisAssembly)))
                .AddRazorRuntimeCompilation();

            services.Configure<MvcRazorRuntimeCompilationOptions>(
                options => options.FileProviders.Add(new EmbeddedFileProvider(thisAssembly)));

            return services;
        }
    }
}