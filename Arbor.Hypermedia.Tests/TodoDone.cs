using System.Collections.Generic;

namespace Arbor.Hypermedia.Tests
{
    public class TodoDone : IEntity
    {
        public TodoId Id { get; }

        public TodoDone(TodoId id)
        {
            Id = id;
        }
        public const string RouteName = "TodoDone";

        internal record MarkAsDoneMetadata : EntityMetadata
        {
            public MarkAsDoneMetadata(TodoDone entity) : base(
                entity,
                TodoDone.RouteName,
                "",
                CustomHttpMethod.Put)
            {
            }
        }

        public EntityContext Context => new (Id.Value, "MarkAsDone");
    }
}