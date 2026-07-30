using System;
using System.Collections.Generic;

namespace Arbor.Hypermedia
{
    public interface IEntityDescriptor
    {
        Type EntityType { get; }

        IReadOnlyDictionary<string, string?> GetPrimitiveProperties(IEntity entity);

        IEnumerable<HyperMediaFormField> GetFormFields(IEntity entity, CustomHttpMethod routeMethod);
    }
}
