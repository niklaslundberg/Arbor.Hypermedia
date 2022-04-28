using System.Diagnostics.CodeAnalysis;

namespace Arbor.Hypermedia
{
    public class ReferenceFormField : HyperMediaFormField
    {
        public ReferenceFormField(string name, string referenceValue) : base(name) =>
            ReferenceValue = referenceValue;

        public string ReferenceValue { get; }
    }
}