using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace Arbor.Hypermedia.Tests
{
    public class TodoController : Controller
    {
        private readonly DataStore _store;

        public TodoController(DataStore store)
        {
            _store = store;
        }

        [Route("/todo/", Name = "todos")]
        [Microsoft.AspNetCore.Mvc.HttpGet]
        public async Task<TodoList> Index() =>
            new(_store.Items.Values.OrderBy(value => value.Id).ToImmutableArray());

        [Route("/todo/{id}", Name = GetTodo.RouteName)]
        [Microsoft.AspNetCore.Mvc.HttpGet]
        public async Task<ActionResult<TodoItem>> Index([FromRoute] TodoId id) =>
            _store.GetOrDefault(id);

        [Route("/todo/{id}/done", Name = TodoDone.RouteName)]
        [Microsoft.AspNetCore.Mvc.HttpPut]
        public async Task<ActionResult<TodoItem>> Done([FromRoute] TodoId id)
        {
            var todoItem = _store.GetOrDefault(id);

            if (todoItem is null)
            {
                return NotFound();
            }

            todoItem.Handle(new TodoDone(id));

            return todoItem;
        }

        [Route("/todo/{id}/comment", Name = TodoComment.RouteName)]
        [Microsoft.AspNetCore.Mvc.HttpPut]
        public async Task<ActionResult<TodoItem>> Comment([FromRoute] TodoId id, [FromBody] TodoComment.Input comment)
        {
            var todoItem = _store.GetOrDefault(id);

            if (todoItem is null)
            {
                return NotFound();
            }

            todoItem.Handle(new TodoComment(id, comment.Comment));

            return todoItem;
        }
    }


}