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
        public async Task<HyperMediaEntity> GetControl<T>(T metadata, IUrlResolver urlResolver, bool getNext = true) where T : EntityMetadata
        {
            var hyperMediaControls = new List<IHyperMediaControl>();

            var hyperMediaEntity = new HyperMediaEntity(metadata.Entity.Context.Id, metadata.Entity.GetType().Name, urlResolver.GetUrl(metadata), hyperMediaControls);

            hyperMediaControls.AddRange(GetControls(metadata, urlResolver, hyperMediaEntity, getNext: getNext));

            return hyperMediaEntity;
        }

        public IReadOnlyCollection<IHyperMediaControl> GetControls(EntityMetadata metadata, IUrlResolver urlResolver, HyperMediaEntity? parent = null, bool getNext = true)
        {
            var hyperMediaControls = new List<IHyperMediaControl> { };
            if (metadata.RouteMethod == CustomHttpMethod.Get)
            {
                var selfUri = urlResolver.GetUrl(metadata);
                hyperMediaControls.Add(new HyperMediaLink(selfUri, LinkRelation.Self));
            }


            var properties = new Dictionary<string, string>();


            if (metadata.Entity is { })
            {
                foreach (var item in metadata.Entity.GetType()
                             .GetProperties(BindingFlags.Public | BindingFlags.Instance).Where(property =>
                                 property.PropertyType.IsPrimitive || property.PropertyType == typeof(string)))
                {
                    object? objectValue = item.GetValue(metadata.Entity);

                    string? value = objectValue as string ?? objectValue?.ToString();

                    if (value is { })
                    {
                        properties.Add(item.Name, value);
                    }
                }
            }

            if (properties.Any())
            {
                hyperMediaControls.Add(new ObjectControl(properties));
            }

            if (getNext)
            {
                foreach (var action in metadata.Actions)
                {
                    var actionUrl = urlResolver.GetUrl(action);

                    hyperMediaControls.Add(new HyperMediaForm(GetFields(action),
                        action.RouteMethod,
                        actionUrl,
                        new LinkRelation(action.RouteName)));

                    hyperMediaControls.AddRange(GetControls(action, urlResolver, getNext: getNext));
                }
            }

            foreach (var action in metadata.Items)
            {
                var controls = new List<IHyperMediaControl>();
                var hyperMediaControl = new HyperMediaEntity(action.Entity.Context.Id, action.Entity.GetType().Name, urlResolver.GetUrl(action), controls, parent);

                if (getNext)
                {
                    controls.AddRange(GetControls(action, urlResolver, hyperMediaControl, getNext: getNext));
                }

                hyperMediaControls.Add(hyperMediaControl);
            }

            return hyperMediaControls;
        }

        private IEnumerable<HyperMediaFormField> GetFields(EntityMetadata metadata)
        {
            if (metadata.Entity is null)
            {
                yield break;
            }

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
                    yield return new StringFormField(item.Name, item.GetValue(metadata.Entity)?.ToString());
                }
            }
        }
    }
}