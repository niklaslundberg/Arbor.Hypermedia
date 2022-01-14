using System.Collections.Generic;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Arbor.Hypermedia.Tests
{
    public class GetTodo
    {
        public const string RouteName = "GetTodo";

        internal record TodoMetadata : EntityMetadata
        {
            public TodoMetadata(TodoItem.TodoItemView entity, IEnumerable<EntityMetadata>? actions, IEnumerable<EntityMetadata>? items) : base(
                entity,
                GetTodo.RouteName,
                "id",
                CustomHttpMethod.Get,
                actions,
                items)
            {
            }
        }
    }
}