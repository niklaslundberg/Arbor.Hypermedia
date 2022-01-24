using System;
using System.Collections.Generic;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Arbor.Hypermedia.Generators
{
    internal class ModelBindingSyntaxReceiver : ISyntaxReceiver
    {
        public List<ClassData> ClassesToGenerateConvertersFor { get; } = new ();

        private readonly HashSet<string> _supportedDataTypes =
            new(StringComparer.OrdinalIgnoreCase) {"int", "string", "long"};

        public void OnVisitSyntaxNode(SyntaxNode syntaxNode)
        {
            if (syntaxNode is ClassDeclarationSyntax cds
                && cds.IsPartial()
                && cds.DataType() is {} dataType
                && _supportedDataTypes.Contains(dataType))
            {
                ClassesToGenerateConvertersFor.Add(new ClassData(cds, cds.Namespace(), dataType, cds.StringComparison(), cds.MinValue()));
            }
        }
    }
}