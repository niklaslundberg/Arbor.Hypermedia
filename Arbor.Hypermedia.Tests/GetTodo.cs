using System.Collections.Generic;

namespace Arbor.Hypermedia.Tests
{
    public class GetTodo
    {
        public const string RouteName = "GetTodo";

        internal record TodoMetadata : EntityMetadata
        {
            public TodoMetadata(TodoItem.TodoItemView entity, IEnumerable<EntityMetadata>? actions) : base(
                entity,
                GetTodo.RouteName,
                "id",
                CustomHttpMethod.Get,
                actions)
            {
            }
        }
    }
}