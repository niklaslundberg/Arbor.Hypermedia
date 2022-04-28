using System;
using System.Collections.Generic;

namespace Arbor.Hypermedia
{
    public class HyperMediaEntity : IHyperMediaControl
    {
        public HyperMediaEntity(string id, string context, Uri href, IEnumerable<IHyperMediaControl> controls, HyperMediaEntity? parent = null)
        {
            Id = id;
            Context = context;
            Href = href;
            Controls = controls;
            Parent = parent;
        }

        public string Id { get; }

        public string Context { get; }

        public Uri Href { get; }

        public IEnumerable<IHyperMediaControl> Controls { get; }

        public HyperMediaEntity? Parent { get; }
    }
}