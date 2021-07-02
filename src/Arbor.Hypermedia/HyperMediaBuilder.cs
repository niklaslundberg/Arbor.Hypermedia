using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Arbor.App.Extensions.ExtensionMethods;
using Arbor.ModelBinding.Primitives;

namespace Arbor.Hypermedia
{
    public class HyperMediaBuilder
    {
        public async Task<HyperMediaEntity> GetControl<T>(T metadata, IUrlResolver urlResolver) where T : EntityMetadata
        {
            var hyperMediaControls = GetControls(metadata, urlResolver);

            var hyperMediaEntity = new HyperMediaEntity(metadata.Entity.Context.Id, metadata.Entity.GetType().Name, hyperMediaControls);

            return hyperMediaEntity;
        }

        public IReadOnlyCollection<IHyperMediaControl> GetControls(EntityMetadata metadata, IUrlResolver urlResolver)
        {
            var hyperMediaControls = new List<IHyperMediaControl> { };
            if (metadata.RouteMethod == CustomHttpMethod.Get)
            {
                var selfUri = urlResolver.GetUrl(metadata);
                hyperMediaControls.Add(new HyperMediaLink(selfUri, LinkRelation.Self));
            }

            var properties = new Dictionary<string, string>();

            foreach (var item in Enumerable.Where<PropertyInfo>(metadata.Entity.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance),
                                              property => property.PropertyType.IsPrimitive))
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