using System.Collections.Generic;
using System.Collections.Immutable;

namespace Arbor.Hypermedia.Tests
{
    public class TodoItem : IMetadata
    {
        public class State
        {
            public static readonly State Done = new ();
            public static readonly State Todo = new ();

            private State(){}
        }

        private State _state;

        public TodoItem(TodoId id, State state, string? comment = null)
        {
            _state = state;
            Id = id;
            Comment = comment;
        }

        public TodoId Id { get; }

        public string? Comment { get; private set; }

        public EntityMetadata CreateMetadata() => new GetTodo.TodoMetadata(new TodoItemView(Id), GetActions());

        private IEnumerable<EntityMetadata> GetActions()
        {
            if (_state != State.Done)
            {
                yield return new TodoDone.MarkAsDoneMetadata(new TodoDone(Id));
            }

            yield return new TodoComment.Metadata(new TodoComment(Id,Comment ?? ""));
        }

        public class TodoItemView : IEntity
        {
            public TodoItemView(TodoId id) => Context = new EntityContext(id.Value, nameof(TodoItem));

            public EntityContext Context { get; }
        }

        public void Handle(TodoComment todoComment)
        {
            Comment = todoComment.Comment;
        }
    }
}