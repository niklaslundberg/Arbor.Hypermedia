namespace Arbor.Hypermedia
{
    public abstract class HyperMediaFormField
    {
        protected HyperMediaFormField(string name) => Name = name;

        public string Name { get; }
    }
}