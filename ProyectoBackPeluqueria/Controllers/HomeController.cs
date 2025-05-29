using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using ProyectoBackPeluqueria.Filters;
using NugetProyectoBackPeluqueria.Models;

namespace ProyectoBackPeluqueria.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        [AuthorizeUsers]
        public IActionResult Index()
        {
            return View();
        }

        [AuthorizeUsers]
        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
