using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Arbor.Hypermedia.Generators
{
    [Generator]
    public sealed class EntityDescriptorGenerator : IIncrementalGenerator
    {
        private const string IEntityFullName = "Arbor.Hypermedia.IEntity";
        private const string ValueObjectBaseFullName = "Arbor.ModelBinding.Primitives.ValueObjectBase`1";
        private const string EntityContextFullName = "Arbor.Hypermedia.EntityContext";
        private const string IEntityDescriptorFullName = "Arbor.Hypermedia.IEntityDescriptor";
        private const string StringValueTypeAttributeFullName = "Arbor.ModelBinding.Primitives.StringValueTypeAttribute";
        private const string IntValueTypeAttributeFullName = "Arbor.ModelBinding.Primitives.IntValueTypeAttribute";
        private const string LongValueTypeAttributeFullName = "Arbor.ModelBinding.Primitives.LongValueTypeAttribute";

        private static readonly string GeneratorVersion =
            typeof(EntityDescriptorGenerator).Assembly.GetName().Version?.ToString() ?? "1.0";

        public void Initialize(IncrementalGeneratorInitializationContext context)
        {
            var entityTypes = context.SyntaxProvider
                .CreateSyntaxProvider(
                    predicate: static (node, _) => node is TypeDeclarationSyntax tds && tds.BaseList != null,
                    transform: GetEntityTypeModel)
                .Where(m => m != null)
                .Select((m, _) => m!);

            context.RegisterSourceOutput(
                entityTypes.Collect(),
                GenerateSource);
        }

        private static EntityTypeModel? GetEntityTypeModel(GeneratorSyntaxContext ctx, CancellationToken ct)
        {
            if (ctx.Node is not TypeDeclarationSyntax typeDecl)
            {
                return null;
            }

            if (ctx.SemanticModel.GetDeclaredSymbol(typeDecl, ct) is not INamedTypeSymbol typeSymbol)
            {
                return null;
            }

            if (typeSymbol.IsAbstract)
            {
                return null;
            }

            var compilation = ctx.SemanticModel.Compilation;
            var entityInterface = compilation.GetTypeByMetadataName(IEntityFullName);
            if (entityInterface == null)
            {
                return null;
            }

            bool implementsIEntity = false;
            foreach (var iface in typeSymbol.AllInterfaces)
            {
                if (SymbolEqualityComparer.Default.Equals(iface, entityInterface))
                {
                    implementsIEntity = true;
                    break;
                }
            }

            if (!implementsIEntity)
            {
                return null;
            }

            var valueObjectBase = compilation.GetTypeByMetadataName(ValueObjectBaseFullName);
            var entityContextType = compilation.GetTypeByMetadataName(EntityContextFullName);
            var stringValueTypeAttr = compilation.GetTypeByMetadataName(StringValueTypeAttributeFullName);
            var intValueTypeAttr = compilation.GetTypeByMetadataName(IntValueTypeAttributeFullName);
            var longValueTypeAttr = compilation.GetTypeByMetadataName(LongValueTypeAttributeFullName);

            var properties = ImmutableArray.CreateBuilder<PropertyModel>();
            foreach (var member in typeSymbol.GetMembers())
            {
                if (member is IPropertySymbol prop
                    && prop.DeclaredAccessibility == Accessibility.Public
                    && !prop.IsStatic)
                {
                    properties.Add(BuildPropertyModel(prop, valueObjectBase, entityContextType, stringValueTypeAttr, intValueTypeAttr, longValueTypeAttr));
                }
            }

            return new EntityTypeModel(
                typeSymbol.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat),
                GetDescriptorClassName(typeSymbol),
                properties.ToImmutable());
        }

        private static PropertyModel BuildPropertyModel(
            IPropertySymbol property,
            INamedTypeSymbol? valueObjectBaseDefinition,
            INamedTypeSymbol? entityContextType,
            INamedTypeSymbol? stringValueTypeAttr,
            INamedTypeSymbol? intValueTypeAttr,
            INamedTypeSymbol? longValueTypeAttr)
        {
            var type = property.Type;
            var unwrapped = UnwrapNullable(type);

            PropertyRole role;

            if (IsPrimitive(unwrapped))
            {
                role = PropertyRole.Primitive;
            }
            else if (IsValueObject(type, valueObjectBaseDefinition, stringValueTypeAttr, intValueTypeAttr, longValueTypeAttr))
            {
                role = PropertyRole.ValueObject;
            }
            else if (unwrapped.SpecialType == SpecialType.System_DateTime)
            {
                role = PropertyRole.DateTime;
            }
            else if (entityContextType != null
                     && SymbolEqualityComparer.Default.Equals(unwrapped, entityContextType))
            {
                role = PropertyRole.EntityContext;
            }
            else
            {
                role = PropertyRole.String;
            }

            return new PropertyModel(property.Name, role);
        }

        private static bool IsValueObject(
            ITypeSymbol type,
            INamedTypeSymbol? valueObjectBaseDefinition,
            INamedTypeSymbol? stringValueTypeAttr,
            INamedTypeSymbol? intValueTypeAttr,
            INamedTypeSymbol? longValueTypeAttr)
        {
            // Check base class chain for already-complete types
            if (valueObjectBaseDefinition != null && ClosesValueObjectBase(type, valueObjectBaseDefinition))
            {
                return true;
            }

            // Check for ValueType marker attributes. Needed for generated partial classes where the
            // base class (ValueObjectBase<T>) is added by another source generator and may not be
            // visible in the current compilation pass due to generator ordering.
            foreach (var attr in type.GetAttributes())
            {
                if (stringValueTypeAttr != null
                    && SymbolEqualityComparer.Default.Equals(attr.AttributeClass, stringValueTypeAttr))
                {
                    return true;
                }

                if (intValueTypeAttr != null
                    && SymbolEqualityComparer.Default.Equals(attr.AttributeClass, intValueTypeAttr))
                {
                    return true;
                }

                if (longValueTypeAttr != null
                    && SymbolEqualityComparer.Default.Equals(attr.AttributeClass, longValueTypeAttr))
                {
                    return true;
                }
            }

            return false;
        }

        private static ITypeSymbol UnwrapNullable(ITypeSymbol type)
        {
            if (type is INamedTypeSymbol named
                && named.IsGenericType
                && named.ConstructedFrom.SpecialType == SpecialType.System_Nullable_T)
            {
                return named.TypeArguments[0];
            }

            return type;
        }

        private static bool IsPrimitive(ITypeSymbol type)
        {
            return type.SpecialType == SpecialType.System_Boolean
                || type.SpecialType == SpecialType.System_Byte
                || type.SpecialType == SpecialType.System_SByte
                || type.SpecialType == SpecialType.System_Int16
                || type.SpecialType == SpecialType.System_UInt16
                || type.SpecialType == SpecialType.System_Int32
                || type.SpecialType == SpecialType.System_UInt32
                || type.SpecialType == SpecialType.System_Int64
                || type.SpecialType == SpecialType.System_UInt64
                || type.SpecialType == SpecialType.System_Char
                || type.SpecialType == SpecialType.System_Double
                || type.SpecialType == SpecialType.System_Single;
        }

        private static bool ClosesValueObjectBase(ITypeSymbol type, INamedTypeSymbol valueObjectBaseDefinition)
        {
            var current = type.BaseType;
            while (current != null)
            {
                if (current.IsGenericType
                    && SymbolEqualityComparer.Default.Equals(current.OriginalDefinition, valueObjectBaseDefinition))
                {
                    return true;
                }

                current = current.BaseType;
            }

            return false;
        }

        private static string GetDescriptorClassName(INamedTypeSymbol typeSymbol)
        {
            var sb = new StringBuilder();
            BuildTypeName(typeSymbol, sb);
            sb.Append("EntityDescriptor");
            return sb.ToString();
        }

        private static void BuildTypeName(INamedTypeSymbol typeSymbol, StringBuilder sb)
        {
            if (typeSymbol.ContainingType != null)
            {
                BuildTypeName(typeSymbol.ContainingType, sb);
                sb.Append('_');
            }

            sb.Append(typeSymbol.Name);
        }

        private static void GenerateSource(
            SourceProductionContext spc,
            ImmutableArray<EntityTypeModel> entityTypes)
        {
            if (entityTypes.IsEmpty)
            {
                return;
            }

            var sb = new StringBuilder();
            sb.AppendLine("// <auto-generated/>");
            sb.AppendLine("#nullable enable");
            sb.AppendLine();
            sb.AppendLine("using Microsoft.Extensions.DependencyInjection;");
            sb.AppendLine();
            sb.AppendLine("namespace Arbor.Hypermedia.Generated");
            sb.AppendLine("{");

            foreach (var entityType in entityTypes)
            {
                AppendDescriptorClass(sb, entityType);
            }

            AppendRegistrationClass(sb, entityTypes);

            sb.AppendLine("}");

            spc.AddSource("HypermediaEntityDescriptors.g.cs", sb.ToString());
        }

        private static void AppendDescriptorClass(StringBuilder sb, EntityTypeModel model)
        {
            var primitiveProps = new List<PropertyModel>();
            var formProps = new List<PropertyModel>();

            foreach (var p in model.Properties)
            {
                if (p.Role == PropertyRole.Primitive)
                {
                    primitiveProps.Add(p);
                }
                else if (p.Role != PropertyRole.EntityContext)
                {
                    formProps.Add(p);
                }
            }

            sb.AppendLine("    [global::System.CodeDom.Compiler.GeneratedCode(\"Arbor.Hypermedia.Generators\", \"" + GeneratorVersion + "\")]");
            sb.AppendLine("    internal sealed class " + model.DescriptorClassName + " : global::Arbor.Hypermedia.IEntityDescriptor");
            sb.AppendLine("    {");

            sb.AppendLine("        public global::System.Type EntityType => typeof(" + model.FullyQualifiedTypeName + ");");
            sb.AppendLine();

            // GetPrimitiveProperties
            sb.AppendLine("        public global::System.Collections.Generic.IReadOnlyDictionary<string, string?> GetPrimitiveProperties(global::Arbor.Hypermedia.IEntity entity)");
            sb.AppendLine("        {");
            if (primitiveProps.Count == 0)
            {
                sb.AppendLine("            return new global::System.Collections.Generic.Dictionary<string, string?>();");
            }
            else
            {
                sb.AppendLine("            var e = (" + model.FullyQualifiedTypeName + ")entity;");
                sb.AppendLine("            var result = new global::System.Collections.Generic.Dictionary<string, string?>(" + primitiveProps.Count + ");");
                foreach (var prop in primitiveProps)
                {
                    sb.AppendLine("            result[\"" + prop.Name + "\"] = e." + prop.Name + ".ToString();");
                }
                sb.AppendLine("            return result;");
            }
            sb.AppendLine("        }");
            sb.AppendLine();

            // GetFormFields
            sb.AppendLine("        public global::System.Collections.Generic.IEnumerable<global::Arbor.Hypermedia.HyperMediaFormField> GetFormFields(global::Arbor.Hypermedia.IEntity entity, global::Arbor.Hypermedia.CustomHttpMethod routeMethod)");
            sb.AppendLine("        {");
            if (formProps.Count == 0)
            {
                sb.AppendLine("            yield break;");
            }
            else
            {
                sb.AppendLine("            var e = (" + model.FullyQualifiedTypeName + ")entity;");
                foreach (var prop in formProps)
                {
                    bool isIdProp = prop.Name == "Id";
                    string indent = isIdProp ? "                " : "            ";

                    if (isIdProp)
                    {
                        sb.AppendLine("            if (routeMethod != global::Arbor.Hypermedia.CustomHttpMethod.Put)");
                        sb.AppendLine("            {");
                    }

                    string fieldCreation;
                    if (prop.Role == PropertyRole.ValueObject)
                    {
                        fieldCreation = "new global::Arbor.Hypermedia.StringFormField(\"" + prop.Name + "\", e." + prop.Name + "?.ToString())";
                    }
                    else if (prop.Role == PropertyRole.DateTime)
                    {
                        fieldCreation = "new global::Arbor.Hypermedia.DateFormField(\"" + prop.Name + "\")";
                    }
                    else
                    {
                        fieldCreation = "new global::Arbor.Hypermedia.StringFormField(\"" + prop.Name + "\")";
                    }

                    sb.AppendLine(indent + "yield return " + fieldCreation + ";");

                    if (isIdProp)
                    {
                        sb.AppendLine("            }");
                    }
                }
            }
            sb.AppendLine("        }");
            sb.AppendLine("    }");
            sb.AppendLine();
        }

        private static void AppendRegistrationClass(StringBuilder sb, ImmutableArray<EntityTypeModel> entityTypes)
        {
            sb.AppendLine("    [global::System.CodeDom.Compiler.GeneratedCode(\"Arbor.Hypermedia.Generators\", \"" + GeneratorVersion + "\")]");
            sb.AppendLine("    internal static class GeneratedHypermediaDescriptorRegistration");
            sb.AppendLine("    {");
            sb.AppendLine("        public static global::Microsoft.Extensions.DependencyInjection.IServiceCollection AddGeneratedHypermediaDescriptors(");
            sb.AppendLine("            this global::Microsoft.Extensions.DependencyInjection.IServiceCollection services)");
            sb.AppendLine("        {");
            foreach (var entityType in entityTypes)
            {
                sb.AppendLine("            services.AddSingleton<global::Arbor.Hypermedia.IEntityDescriptor, " + entityType.DescriptorClassName + ">();");
            }
            sb.AppendLine("            return services;");
            sb.AppendLine("        }");
            sb.AppendLine("    }");
        }
    }

    internal sealed class EntityTypeModel
    {
        public string FullyQualifiedTypeName { get; }
        public string DescriptorClassName { get; }
        public ImmutableArray<PropertyModel> Properties { get; }

        public EntityTypeModel(
            string fullyQualifiedTypeName,
            string descriptorClassName,
            ImmutableArray<PropertyModel> properties)
        {
            FullyQualifiedTypeName = fullyQualifiedTypeName;
            DescriptorClassName = descriptorClassName;
            Properties = properties;
        }
    }

    internal sealed class PropertyModel
    {
        public string Name { get; }
        public PropertyRole Role { get; }

        public PropertyModel(string name, PropertyRole role)
        {
            Name = name;
            Role = role;
        }
    }

    internal enum PropertyRole
    {
        Primitive,
        ValueObject,
        DateTime,
        EntityContext,
        String
    }
}
