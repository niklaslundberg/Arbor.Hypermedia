using System.Collections.Generic;
using System.Threading.Tasks;

namespace Arbor.Hypermedia
{
    public class HyperMediaBuilder
    {
        private readonly EntityDescriptorRegistry _descriptorRegistry;

        public HyperMediaBuilder(EntityDescriptorRegistry descriptorRegistry)
        {
            _descriptorRegistry = descriptorRegistry;
        }

        public async Task<HyperMediaEntity> GetControl<T>(T metadata, IUrlResolver urlResolver) where T : EntityMetadata
        {
            var hyperMediaControls = new List<IHyperMediaControl>();

            var hyperMediaEntity = new HyperMediaEntity(metadata.Entity.Context.Id, metadata.Entity.GetType().Name, urlResolver.GetUrl(metadata), hyperMediaControls);

            hyperMediaControls.AddRange(GetControls(metadata, urlResolver, hyperMediaEntity));

            return hyperMediaEntity;
        }

        public IReadOnlyCollection<IHyperMediaControl> GetControls(EntityMetadata metadata, IUrlResolver urlResolver, HyperMediaEntity? parent = null)
        {
            var hyperMediaControls = new List<IHyperMediaControl>();
            if (metadata.RouteMethod == CustomHttpMethod.Get)
            {
                var selfUri = urlResolver.GetUrl(metadata);
                hyperMediaControls.Add(new HyperMediaLink(selfUri, LinkRelation.Self));
            }

            var descriptor = _descriptorRegistry.GetDescriptor(metadata.Entity.GetType());
            var primitiveProps = descriptor.GetPrimitiveProperties(metadata.Entity);
            var properties = new System.Collections.Generic.Dictionary<string, string>(primitiveProps.Count);
            foreach (var kvp in primitiveProps)
            {
                if (kvp.Value is not null)
                {
                    properties[kvp.Key] = kvp.Value;
                }
            }

            hyperMediaControls.Add(new ObjectControl(properties));

            foreach (var action in metadata.Actions)
            {
                var actionUrl = urlResolver.GetUrl(action);

                hyperMediaControls.Add(new HyperMediaForm(GetFields(action),
                    action.RouteMethod,
                    actionUrl,
                    new LinkRelation(action.RouteName)));

                hyperMediaControls.AddRange(GetControls(action, urlResolver));
            }

            foreach (var action in metadata.Items)
            {
                var controls = new List<IHyperMediaControl>();
                var hyperMediaControl = new HyperMediaEntity(action.Entity.Context.Id, action.Entity.GetType().Name, urlResolver.GetUrl(action), controls, parent);
                controls.AddRange(GetControls(action, urlResolver, hyperMediaControl));
                hyperMediaControls.Add(hyperMediaControl);
            }

            return hyperMediaControls;
        }

        private IEnumerable<HyperMediaFormField> GetFields(EntityMetadata metadata)
        {
            var descriptor = _descriptorRegistry.GetDescriptor(metadata.Entity.GetType());
            return descriptor.GetFormFields(metadata.Entity, metadata.RouteMethod);
        }
    }
}