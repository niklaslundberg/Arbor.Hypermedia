namespace Arbor.Hypermedia
{
    public class EntityContext
    {
        public EntityContext(string id, string type)
        {
            Id = id;
            Type = type;
        }

        public string Id { get; }

        public string Type { get; }
    }
}