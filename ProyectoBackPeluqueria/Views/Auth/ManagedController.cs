using Microsoft.AspNetCore.Mvc;

namespace MvcNetCoreCSRF.Controllers
{
    public class ManagedController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Login(string username, string password)
        {
            if (username.ToLower() == "admin" && password == "123")
            {
                HttpContext.Session.SetString("username", username);
                return RedirectToAction("Productos", "Tienda");
            }
            else
            {
                ViewData["Error"] = "Usuario o contraseña incorrectos";
                return View();
            }
        }

        public IActionResult Denied()
        {
            return View();
        }
    }
}
