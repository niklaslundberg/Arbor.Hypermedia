using System.Collections.Immutable;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace Arbor.Hypermedia.Tests
{
    public class TodoController : Controller
    {
        private readonly DataStore _store = new ();

        [Route("/todo/", Name = "todos")]
        [Microsoft.AspNetCore.Mvc.HttpGet]
        public async Task<ImmutableArray<TodoItem>> Index() =>
            _store.Items.Values.ToImmutableArray();

        [Route("/todo/{id}", Name = GetTodo.RouteName)]
        [Microsoft.AspNetCore.Mvc.HttpGet]
        public async Task<ActionResult<TodoItem>> Index([FromRoute] TodoId id) =>
            _store.GetOrDefault(id);

        [Route("/todo/done", Name = TodoDone.RouteName)]
        [Microsoft.AspNetCore.Mvc.HttpPost]
        public async Task<ActionResult<TodoItem>> Done([FromBody] TodoDone done) =>
           _store.GetOrDefault(done.Id);

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