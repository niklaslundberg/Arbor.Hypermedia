using System;

namespace Arbor.Hypermedia
{
    public interface IUrlResolver
    {
        Uri GetUrl(EntityMetadata metadata);
    }
}