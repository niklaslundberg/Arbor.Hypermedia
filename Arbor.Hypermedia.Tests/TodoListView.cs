namespace Arbor.Hypermedia.Tests;

public class TodoListView : IEntity
{
    public TodoListView() => Context = new EntityContext("todos", typeof(TodoItem[]).Name);

    public EntityContext Context { get; }
}