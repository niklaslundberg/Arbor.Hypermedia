using System.Collections.Generic;

namespace Arbor.Hypermedia
{
    public class ObjectControl : IHyperMediaControl
    {
        public ObjectControl(Dictionary<string, string> properties) => Properties = properties;

        public Dictionary<string, string> Properties { get; }
    }
}