using System.Collections.Immutable;
using System.Linq;

namespace Arbor.Hypermedia.Tests;

public class TodoList : IMetadata
{
    public const string RouteName = "todos";
    public ImmutableArray<TodoItem> TodoItems { get; }

    public TodoList(ImmutableArray<TodoItem> todoItems) => TodoItems = todoItems;
    public EntityMetadata GetEntityMetadata() => new GetTodoList.TodoListMetadata(new TodoListView(), items: TodoItems.Select(item => item.GetEntityMetadata()));
}