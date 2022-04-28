using System.Diagnostics.CodeAnalysis;

namespace Arbor.Hypermedia
{
    public class StringFormField : HyperMediaFormField
    {
        public string? Value { get; }

        public StringFormField(string name, string? value = null) : base(name)
        {
            Value = value;
        }
    }
}