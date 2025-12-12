using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Proyecto_Grupo_7_Progra_Avanzada.Models;
using Microsoft.AspNetCore.Authorization;

namespace Proyecto_Grupo_7_Progra_Avanzada.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        // CLAVE: Permitir anónimos en el Home SOLO para la redirección inicial, 
        // y redirigir explícitamente al Login si no hay sesión.
        [AllowAnonymous]
        public IActionResult Index()
        {
            // Si el usuario NO está autenticado, lo enviamos directamente al Login Path.
            if (!User.Identity.IsAuthenticated)
            {
                // Redirigimos al Login Path configurado
                return Redirect("~/Identity/Account/Login");
            }

            // Si está autenticado, muestra el Home normal.
            return View();
        }

        // Privacy requiere autenticación (porque @attribute [Authorize] está en _ViewImports)
        public IActionResult Privacy()
        {
            return View();
        }

        // La acción de Error debe ser siempre pública 
        [AllowAnonymous]
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}