using System.Collections.Generic;
using System.Collections.Immutable;
using Arbor.AppModel.ExtensionMethods;

namespace Arbor.Hypermedia
{
    public class SingleSelectionFormField : HyperMediaFormField
    {
        public SingleSelectionFormField(string name, IEnumerable<SelectionOption> options) : base(name) =>
            Options = options.SafeToImmutableArray();

        public ImmutableArray<SelectionOption> Options { get; }
    }
}