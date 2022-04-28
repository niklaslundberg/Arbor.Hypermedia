using System.Diagnostics.CodeAnalysis;
using Arbor.AppModel.Time;

namespace Arbor.Hypermedia
{
    public class DateFormField : HyperMediaFormField
    {
        public DateFormField(string name, Date? defaultValue = default) : base(name) =>
            DefaultValue = defaultValue;

        public Date? DefaultValue { get; }
    }
}