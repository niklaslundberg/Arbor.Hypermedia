using System.Collections.Generic;
using System.Collections.Immutable;
using Arbor.AppModel.ExtensionMethods;

namespace Arbor.Hypermedia
{
    public abstract record EntityMetadata
    {
        protected EntityMetadata(IEntity entity,
            string routeName,
            string routeParameterName,
            CustomHttpMethod routeMethod,
            IEnumerable<EntityMetadata>? actions = null)
        {
            Entity = entity;
            RouteName = routeName;
            RouteParameterName = routeParameterName;
            RouteMethod = routeMethod;
            Actions = actions.SafeToImmutableArray();
        }

        public IEntity Entity { get; }

        public string RouteName { get; }

        public ImmutableArray<EntityMetadata> Actions { get; }

        public string RouteParameterName { get; }

        public CustomHttpMethod RouteMethod { get; }
    }
}