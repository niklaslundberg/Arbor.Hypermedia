using System.Diagnostics.CodeAnalysis;

namespace Arbor.Hypermedia
{
    public class StringFormField : HyperMediaFormField
    {
        public StringFormField([NotNull] string name) : base(name)
        {
        }
    }
}