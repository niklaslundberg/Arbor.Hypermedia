using System;

namespace Arbor.Hypermedia
{
    public class HyperMediaLink : IHyperMediaControl
    {
        public HyperMediaLink(Uri href, LinkRelation relation)
        {
            Href = href;
            Relation = relation;
        }

        public Uri Href { get; }

        public LinkRelation Relation { get; }
    }
}