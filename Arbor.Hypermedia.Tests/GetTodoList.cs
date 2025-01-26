using System.Collections.Generic;

namespace Arbor.Hypermedia.Tests;

public class GetTodoList
{
    public const string RouteName = "Todos";

    internal record TodoListMetadata : EntityMetadata
    {
        public TodoListMetadata(TodoListView list, IEnumerable<EntityMetadata>? items = null) : base(
            list,
            GetTodoList.RouteName,
            null,
            CustomHttpMethod.Get,
            new List<EntityMetadata> {new CreateTodo().Metadata()},
            items)
        {
        }
    }
}