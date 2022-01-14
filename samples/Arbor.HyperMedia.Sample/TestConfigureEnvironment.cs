using Arbor.AppModel.Application;
using Arbor.AppModel.Configuration;

namespace Arbor.HyperMedia.Sample;

public class TestConfigureEnvironment : IConfigureEnvironment
{

    public void Configure(EnvironmentConfiguration environmentConfiguration)
    {
        environmentConfiguration.HttpEnabled = true;
    }
}