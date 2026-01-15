using Microsoft.AspNetCore.Mvc;

namespace WebApplication1.Controllers
{
    public class BookDetailController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
