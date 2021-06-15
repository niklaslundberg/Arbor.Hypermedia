using System.Collections.Generic;

namespace Arbor.Hypermedia
{
    public class HyperMediaEntity : IHyperMediaControl
    {
        public HyperMediaEntity(string id, string context, IEnumerable<IHyperMediaControl> controls)
        {
            Id = id;
            Context = context;
            Controls = controls;
        }

        public string Id { get; }

        public string Context { get; }

        public IEnumerable<IHyperMediaControl> Controls { get; }
    }
}