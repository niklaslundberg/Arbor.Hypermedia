using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Arbor.AppModel.ExtensionMethods;
using Arbor.ModelBinding.Primitives;

namespace Arbor.Hypermedia
{
    public class HyperMediaBuilder
    {
        public async Task<HyperMediaEntity> GetControl<T>(T metadata, IUrlResolver urlResolver) where T : EntityMetadata
        {
            var hyperMediaControls = new List<IHyperMediaControl>();

            var hyperMediaEntity = new HyperMediaEntity(metadata.Entity.Context.Id, metadata.Entity.GetType().Name, urlResolver.GetUrl(metadata), hyperMediaControls);

            hyperMediaControls.AddRange(GetControls(metadata, urlResolver, hyperMediaEntity));

            return hyperMediaEntity;
        }

        public IReadOnlyCollection<IHyperMediaControl> GetControls(EntityMetadata metadata, IUrlResolver urlResolver, HyperMediaEntity? parent = null)
        {
            var hyperMediaControls = new List<IHyperMediaControl> { };
            if (metadata.RouteMethod == CustomHttpMethod.Get)
            {
                var selfUri = urlResolver.GetUrl(metadata);
                hyperMediaControls.Add(new HyperMediaLink(selfUri, LinkRelation.Self));
            }

            var properties = new Dictionary<string, string>();

            foreach (var item in metadata.Entity.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance).Where(property => property.PropertyType.IsPrimitive))
            {
                string? value = item.GetValue(metadata.Entity)?.ToString();

                if (value is { })
                {
                    properties.Add(item.Name, value);
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
            foreach (var item in metadata.Entity.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance))
            {
                if (metadata.RouteMethod == CustomHttpMethod.Put && item.Name == "Id")
                {
                    continue;
                }
                if (item.PropertyType.Closes(typeof(ValueObjectBase<>)))
                {
                    yield return new StringFormField(item.Name, item.GetValue(metadata.Entity)?.ToString());
                }
                else if (item.PropertyType.IsAssignableTo(typeof(DateTime?)))
                {
                    yield return new DateFormField(item.Name);
                }
                else if (item.PropertyType == typeof(EntityContext))
                {

                }
                else
                {
                    yield return new StringFormField(item.Name);
                }
            }
        }
    }
}