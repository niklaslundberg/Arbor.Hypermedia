using System.Collections.Concurrent;

namespace Arbor.Hypermedia.Tests
{
    public class DataStore
    {
        public DataStore() =>
            Items = new()
            {
                [new TodoId("1")] = new TodoItem(new TodoId("1"), TodoItem.State.Todo, "existing comment"),
                [new TodoId("2")] = new TodoItem(new TodoId("2"), TodoItem.State.Todo),
                [new TodoId("3")] = new TodoItem(new TodoId("3"), TodoItem.State.Done)
            };
        public ConcurrentDictionary<TodoId, TodoItem> Items { get; }

        public TodoItem? GetOrDefault(TodoId id)
        {
            if (Items.TryGetValue(id, out var value))
            {
                return value;
            }

            return default;
        }
    }
}