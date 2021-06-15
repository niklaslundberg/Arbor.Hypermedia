using System.Diagnostics.CodeAnalysis;
using Arbor.App.Extensions.Time;

namespace Arbor.Hypermedia
{
    public class DateFormField : HyperMediaFormField
    {
        public DateFormField([NotNull] string name, Date? defaultValue = default) : base(name) =>
            DefaultValue = defaultValue;

        public Date? DefaultValue { get; }
    }
}