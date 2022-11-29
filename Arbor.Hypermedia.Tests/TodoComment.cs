namespace Arbor.Hypermedia.Tests
{
    public class TodoComment : IEntity
    {
        public const string RouteName = "TodoComment";
        public TodoId Id { get; }

        public string Comment { get; }

        public TodoComment(TodoId id, string comment)
        {
            Id = id;
            Comment = comment;
        }


        internal record Metadata : EntityMetadata
        {
            public Metadata(TodoComment entity) : base(
                entity,
                TodoComment.RouteName,
                "id",
                CustomHttpMethod.Put)
            {
            }
        }
        public EntityContext Context => new(Id.Value, "Set comment");

        public class Input
        {
            public string Comment { get; }

            public Input(string comment)
            {
                Comment = comment;
            }
        }
    }
}