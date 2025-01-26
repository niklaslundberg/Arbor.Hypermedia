using System.Collections.Generic;

namespace Arbor.Hypermedia.Tests;

internal record CreateTodoMetadata : EntityMetadata
{
    public CreateTodoMetadata(IEnumerable<EntityMetadata>? actions = null, IEnumerable<EntityMetadata>? items = null) : base(null, "todos", "", CustomHttpMethod.Post, actions, items)
    {
    }
}