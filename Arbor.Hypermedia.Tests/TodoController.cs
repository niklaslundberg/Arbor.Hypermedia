using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace Arbor.Hypermedia.Tests
{
    public class TodoController : Controller
    {
        private readonly HyperMediaResult _hyperMediaResult;
        private readonly DataStore _store = new ();

        public TodoController(HyperMediaResult hyperMediaResult) => _hyperMediaResult = hyperMediaResult;

        [Route("/todo/{id}", Name = GetTodo.RouteName)]
        [Microsoft.AspNetCore.Mvc.HttpGet]
        public async Task<IActionResult> Index([FromRoute] TodoId id) =>
            await _hyperMediaResult.ToHyperMediaResult(this, _store.GetOrDefault(id));

        [Route("/todo/done", Name = TodoDone.RouteName)]
        [Microsoft.AspNetCore.Mvc.HttpPost]
        public async Task<IActionResult> Done([FromBody] TodoDone done) =>
            await _hyperMediaResult.ToHyperMediaResult(this, _store.GetOrDefault(done.Id));

        [Route("/todo/{id}/comment", Name = TodoComment.RouteName)]
        [Microsoft.AspNetCore.Mvc.HttpPut]
        public async Task<IActionResult> Comment([FromRoute] TodoId id, [FromBody] TodoComment.Input comment)
        {

            var todoItem = _store.GetOrDefault(id);

            if (todoItem is null)
            {
                return NotFound();
            }

            todoItem.Handle(new TodoComment(id, comment.Comment));
            return await _hyperMediaResult.ToHyperMediaResult(this, todoItem);
        }
    }


}