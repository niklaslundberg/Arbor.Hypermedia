using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace Arbor.Hypermedia
{
    public sealed class EntityDescriptorRegistry
    {
        private readonly IReadOnlyDictionary<Type, IEntityDescriptor> _registered;
        private readonly ConcurrentDictionary<Type, IEntityDescriptor> _reflectionCache = new();

        public EntityDescriptorRegistry(IEnumerable<IEntityDescriptor> descriptors)
        {
            _registered = descriptors.ToDictionary(d => d.EntityType);
        }

        public IEntityDescriptor GetDescriptor(Type entityType)
        {
            if (_registered.TryGetValue(entityType, out var descriptor))
            {
                return descriptor;
            }

            return _reflectionCache.GetOrAdd(entityType, static t => new CachedReflectionEntityDescriptor(t));
        }
    }
}
