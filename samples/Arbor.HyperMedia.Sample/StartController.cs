using Microsoft.AspNetCore.Mvc;

namespace Arbor.HyperMedia.Sample;

public class StartController : Controller
{
    [Route("")]
    [HttpGet]
    public IActionResult Index()
    {
        return RedirectToAction("Index", "Todo");
    }
}