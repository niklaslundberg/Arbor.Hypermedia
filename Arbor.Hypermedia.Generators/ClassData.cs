using System;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Arbor.Hypermedia.Generators
{
    internal class ClassData
    {
        public ClassData(
            ClassDeclarationSyntax syntax,
            string? @namespace,
            string? dataType,
            StringComparison? stringComparison,
            long? minValue)
        {
            Syntax = syntax;
            Namespace = @namespace;
            DataType = dataType;
            StringComparison = stringComparison;
            MinValue = minValue;
        }

        public string NetType => DataType switch
        {
            "string" => "string",
            "int" => "int",
            "String" => "string",
            "Int" => "int",
            "Long" => "long",
            "long" => "long",
            _ => "string"
        };

        public ClassDeclarationSyntax Syntax { get; }

        public string? Namespace { get; }

        public string? DataType { get; }
        public StringComparison? StringComparison { get; }
        public long? MinValue { get; }
    }
}