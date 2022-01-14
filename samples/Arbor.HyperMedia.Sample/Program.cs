
using System.Reflection;
using Arbor.HyperMedia.Sample;
using Arbor.Hypermedia.Tests;
using Arbor.Primitives;

return await Arbor.AppModel.AppStarter<SampleStartup>.StartAsync(args,
    EnvironmentVariables.GetEnvironmentVariables().Variables,
    assemblies: new Assembly[] { typeof(SampleStartup).Assembly, typeof(TodoController).Assembly });