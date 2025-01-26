using Arbor.Hypermedia.Tests;

namespace Arbor.Hypermedia.Generators.Tests;

public record TodoDone(TodoItem Result) : ICommandResult<TodoItem>;