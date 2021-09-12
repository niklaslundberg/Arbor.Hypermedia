using Arbor.App.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;

namespace Arbor.Hypermedia
{
    public class HyperMediaModule : IModule
    {
        public IServiceCollection Register(IServiceCollection builder) =>
            builder.AddScoped<HyperMediaBuilder>();
    }
}