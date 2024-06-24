using Microsoft.AspNetCore.Mvc;

namespace JamesCrafts.Controllers
{
    public class UserController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
