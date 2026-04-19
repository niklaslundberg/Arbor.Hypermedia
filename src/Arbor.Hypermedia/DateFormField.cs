using System;

namespace Arbor.Hypermedia
{
    public class DateFormField : HyperMediaFormField
    {
        public DateFormField(string name, DateOnly? defaultValue = default) : base(name) =>
            DefaultValue = defaultValue;

        public DateOnly? DefaultValue { get; }
    }
}