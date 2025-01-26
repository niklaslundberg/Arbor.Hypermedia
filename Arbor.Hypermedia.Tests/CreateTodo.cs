namespace Arbor.Hypermedia.Tests;

public class CreateTodo
{
    public const string RouteName = nameof(CreateTodo);
    public const string RouteTemplate = "/todo";
    public EntityMetadata Metadata() => new CreateTodoMetadata();
}