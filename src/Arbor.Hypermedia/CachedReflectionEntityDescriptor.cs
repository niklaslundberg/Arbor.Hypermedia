using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Arbor.AppModel.ExtensionMethods;
using Arbor.ModelBinding.Primitives;

namespace Arbor.Hypermedia
{
    internal sealed class CachedReflectionEntityDescriptor : IEntityDescriptor
    {
        private readonly IReadOnlyList<(string Name, Func<IEntity, string?> Getter)> _primitiveGetters;
        private readonly IReadOnlyList<FormFieldDescriptor> _formFieldDescriptors;

        public Type EntityType { get; }

        internal CachedReflectionEntityDescriptor(Type entityType)
        {
            EntityType = entityType;

            var props = entityType.GetProperties(BindingFlags.Public | BindingFlags.Instance);

            _primitiveGetters = props
                .Where(p => p.PropertyType.IsPrimitive)
                .Select(p => (p.Name, BuildStringGetter(entityType, p)))
                .ToList();

            _formFieldDescriptors = props
                .Select(p => BuildFormFieldDescriptor(entityType, p))
                .Where(d => d.Kind != FormFieldKind.Skip)
                .ToList();
        }

        public IReadOnlyDictionary<string, string?> GetPrimitiveProperties(IEntity entity)
        {
            var result = new Dictionary<string, string?>(_primitiveGetters.Count);
            foreach (var (name, getter) in _primitiveGetters)
            {
                var value = getter(entity);
                if (value is not null)
                {
                    result[name] = value;
                }
            }

            return result;
        }

        public IEnumerable<HyperMediaFormField> GetFormFields(IEntity entity, CustomHttpMethod routeMethod)
        {
            foreach (var descriptor in _formFieldDescriptors)
            {
                if (routeMethod == CustomHttpMethod.Put && descriptor.Name == "Id")
                {
                    continue;
                }

                yield return descriptor.Kind switch
                {
                    FormFieldKind.ValueObject => new StringFormField(descriptor.Name, descriptor.ValueGetter!(entity)),
                    FormFieldKind.DateTime => new DateFormField(descriptor.Name),
                    _ => new StringFormField(descriptor.Name)
                };
            }
        }

        private static FormFieldDescriptor BuildFormFieldDescriptor(Type entityType, PropertyInfo property)
        {
            if (property.PropertyType.Closes(typeof(ValueObjectBase<>)))
            {
                return new FormFieldDescriptor(property.Name, FormFieldKind.ValueObject,
                    BuildStringGetter(entityType, property));
            }

            if (property.PropertyType.IsAssignableTo(typeof(DateTime?)))
            {
                return new FormFieldDescriptor(property.Name, FormFieldKind.DateTime, null);
            }

            if (property.PropertyType == typeof(EntityContext))
            {
                return new FormFieldDescriptor(property.Name, FormFieldKind.Skip, null);
            }

            return new FormFieldDescriptor(property.Name, FormFieldKind.String, null);
        }

        private static Func<IEntity, string?> BuildStringGetter(Type entityType, PropertyInfo property)
        {
            var param = Expression.Parameter(typeof(IEntity), "entity");
            var cast = Expression.Convert(param, entityType);
            var propAccess = Expression.Property(cast, property);

            Expression body;
            if (property.PropertyType.IsValueType)
            {
                var boxed = Expression.Convert(propAccess, typeof(object));
                body = Expression.Call(boxed, typeof(object).GetMethod(nameof(object.ToString))!);
            }
            else
            {
                body = Expression.Condition(
                    Expression.Equal(propAccess, Expression.Constant(null, property.PropertyType)),
                    Expression.Constant(null, typeof(string)),
                    Expression.Call(propAccess, typeof(object).GetMethod(nameof(object.ToString))!));
            }

            return Expression.Lambda<Func<IEntity, string?>>(body, param).Compile();
        }

        private sealed class FormFieldDescriptor
        {
            public string Name { get; }
            public FormFieldKind Kind { get; }
            public Func<IEntity, string?>? ValueGetter { get; }

            public FormFieldDescriptor(string name, FormFieldKind kind, Func<IEntity, string?>? valueGetter)
            {
                Name = name;
                Kind = kind;
                ValueGetter = valueGetter;
            }
        }

        private enum FormFieldKind
        {
            ValueObject,
            DateTime,
            Skip,
            String
        }
    }
}
