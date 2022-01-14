namespace Arbor.Hypermedia
{
    public class LinkRelation
    {
        public static readonly LinkRelation Self = new(nameof(Self));

        public LinkRelation(string name)
        {
            FriendlyName = name;
            Name = name.ToLowerInvariant();
        }

        public string FriendlyName { get; set; }

        public string Name { get; }

        public override string ToString() => Name;
    }
}