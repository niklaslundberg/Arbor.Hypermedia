using Arbor.AppModel.Messaging;
using Arbor.Hypermedia.Tests;

namespace Arbor.Hypermedia.Generators.Tests
{
    public sealed record MarkAsDone(TodoId TodoId) : ICommand<TodoDone>, IMetadata
    {
        public EntityMetadata CreateMetadata() => GetTodo.Metadata(TodoId);
    }

    public record TodoDone(TodoItem Result) : ICommandResult<TodoItem>;
}