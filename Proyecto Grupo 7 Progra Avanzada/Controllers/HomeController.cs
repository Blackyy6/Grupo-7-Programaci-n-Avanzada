using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Proyecto_Grupo_7_Progra_Avanzada.Models;
using Microsoft.AspNetCore.Authorization;

namespace Proyecto_Grupo_7_Progra_Avanzada.Controllers
{
    // No ponemos [Authorize] en la clase, para que las acciones individuales controlen el acceso.
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        // CLAVE: El Home (Index) debe ser anónimo para romper el ciclo de redirección.
        [AllowAnonymous]
        public IActionResult Index()
        {
            return View();
        }

        [AllowAnonymous]
        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        [AllowAnonymous]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}