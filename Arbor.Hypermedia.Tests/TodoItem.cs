using System.Collections.Generic;
using System.Collections.Immutable;
using Microsoft.AspNetCore.Mvc;

namespace Arbor.Hypermedia.Tests
{
    public class TodoItem : IMetadata
    {
        public class State
        {
            public static readonly State Done = new (nameof(Done));
            public static readonly State Todo = new (nameof(Todo));
            private readonly string _name;

            private State(string name)
            {
                _name = name;
            }

            public override string ToString() => _name;
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

        public EntityMetadata CreateMetadata() => new GetTodo.TodoMetadata(new TodoItemView(this), GetActions(), GetRelations());

        private IEnumerable<EntityMetadata> GetActions()
        {
            if (_state != State.Done)
            {
                yield return new TodoDone.MarkAsDoneMetadata(new TodoDone(Id));
            }

            yield return new TodoComment.Metadata(new TodoComment(Id, Comment ?? ""));
        }
        private IEnumerable<EntityMetadata> GetRelations()
        {
            yield return new GetTodoList.TodoListMetadata(new TodoListView());
        }

        public class TodoItemView : IEntity
        {
            public TodoItemView(TodoItem todo)
            {
                Context = new EntityContext(todo.Id.Value, nameof(TodoItem));
            }

            public EntityContext Context { get; }
        }

        public void Handle(TodoComment todoComment)
        {
            Comment = todoComment.Comment;
        }

        public void Handle(TodoDone todoDone)
        {
            _state = State.Done;
        }
    }
}