using Arbor.AspNetCore.Mvc.Formatting.HtmlForms;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApplicationParts;
using Microsoft.AspNetCore.Mvc.Infrastructure;
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

            services.AddMvc().RegisterHypermediaAssembly().AddRazorRuntimeCompilation();

            services.AddHypermediaFileProvider();
            services.AddHypermediaOutputFormatter();
            services.AddXwwwUrlEncodedFormatter();

            services.AddSingleton<IActionContextAccessor, ActionContextAccessor>();

            return services;
        }

        private static IServiceCollection AddHypermediaFileProvider(this IServiceCollection services)
        {
            var thisAssembly = typeof(HypermediaRegistrationExtensions).Assembly;

            return services.Configure<MvcRazorRuntimeCompilationOptions>(options =>
                options.FileProviders.Add(new EmbeddedFileProvider(thisAssembly)));
        }

        public static IMvcBuilder RegisterHypermediaAssembly(this IMvcBuilder options)
        {
            var thisAssembly = typeof(HypermediaRegistrationExtensions).Assembly;

            return options.ConfigureApplicationPartManager(applicationPartManager =>
                applicationPartManager.ApplicationParts.Add(new AssemblyPart(thisAssembly)));
        }

        public static IServiceCollection AddHypermediaOutputFormatter(this IServiceCollection serviceCollection) =>
            serviceCollection.Configure<MvcOptions>(options =>
                options.OutputFormatters.Insert(0, new HtmlHypermediaFormatter()));
        public static IServiceCollection AddXwwwUrlEncodedFormatter(this IServiceCollection serviceCollection) =>
            serviceCollection.Configure<MvcOptions>(options =>
                options.InputFormatters.Insert(0, new XWwwFormUrlEncodedFormatter()));
    }
}