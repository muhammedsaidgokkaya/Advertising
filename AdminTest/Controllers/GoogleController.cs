using Microsoft.AspNetCore.Mvc;

namespace AdminTest.Controllers
{
    public class GoogleController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult callback(string code, string scope)
        {
            return View();
        }
    }
}
