using System;
using System.IO;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using Scriban;

namespace Arbor.Hypermedia.Generators
{
    [Generator]
    public class MetadataGenerator : ISourceGenerator
    {
        private static readonly DiagnosticDescriptor WarningMessage = new("ARB1000",
            "Arbor Source generator",
            "'{0}'",
            "Arbor",
            DiagnosticSeverity.Warning,
            true);

        private static readonly DiagnosticDescriptor CustomInformation = new("ARB2000",
            "Code generation info",
            "Info from parser '{0}'",
            "Arbor",
            DiagnosticSeverity.Info,
            true);

        public void Initialize(GeneratorInitializationContext context) =>
            context.RegisterForSyntaxNotifications(() => new ModelBindingSyntaxReceiver());

        public void Execute(GeneratorExecutionContext context)
        {
            if (context.SyntaxReceiver is not ModelBindingSyntaxReceiver mySyntaxReceiver)
            {
                return;
            }

            if (mySyntaxReceiver.ClassesToGenerateConvertersFor.Count == 0)
            {
                return;
            }

            string all = string.Join(", ",
                mySyntaxReceiver.ClassesToGenerateConvertersFor.Select(classData => classData.Syntax.Identifier.ValueText));

            context.ReportDiagnostic(Diagnostic.Create(CustomInformation, Location.None,
                $"Generating code for items {all}"));

            try
            {
                var model = new
                {
                    Mappings = mySyntaxReceiver.ClassesToGenerateConvertersFor.Select(n =>
                        new
                        {
                            Identifier = n.Syntax.Identifier.ValueText,
                            n.Namespace,
                            n.NetType,
                            n.StringComparison,
                            n.MinValue,
                        }).ToArray(),
                    MainNamespace = mySyntaxReceiver.ClassesToGenerateConvertersFor.FirstOrDefault()?.Namespace
                };

                using var manifestResourceStream = typeof(MetadataGenerator).Assembly.GetManifestResourceStream("Arbor.ModelBinding.Generators.Template.scriban-cs");

                if (manifestResourceStream is null)
                {
                    context.ReportDiagnostic(Diagnostic.Create(WarningMessage, Location.None,
                        "Embedded template could not be found"));

                    return;
                }

                using var streamReader = new StreamReader(manifestResourceStream, Encoding.UTF8);

                string templateData = streamReader.ReadToEnd();

                if (string.IsNullOrWhiteSpace(templateData))
                {
                    context.ReportDiagnostic(Diagnostic.Create(WarningMessage, Location.None,
                        "Template could not be found"));

                    return;
                }

                var template1 = Template.Parse(templateData);
                context.ReportDiagnostic(Diagnostic.Create(CustomInformation, Location.None,
                    $"Rendering model with {model.Mappings.Length} items"));

                string? output = template1.Render(model);

                if (!string.IsNullOrWhiteSpace(output))
                {
                    context.AddSource("Converters.g.cs", SourceText.From(output, Encoding.UTF8));
                }
                else
                {
                    context.ReportDiagnostic(Diagnostic.Create(WarningMessage, Location.None,
                        "No source content was rendered"));
                }
            }
            catch (Exception ex)
            {
                context.ReportDiagnostic(Diagnostic.Create(WarningMessage, Location.None, ex.ToString()));
            }
        }
    }
}