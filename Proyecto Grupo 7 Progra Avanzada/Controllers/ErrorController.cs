using Microsoft.AspNetCore.Mvc;

namespace Proyecto_Grupo_7_Progra_Avanzada.Controllers
{
    public class ErrorController : Controller
    {
        // Manejo de error 404 - No Encontrado
        [Route("Error/404")]
        public IActionResult NoEncontrado()
        {
            return View();
        }

        // Manejo de error 500 - Error Interno del Servidor
        [Route("Error/500")]
        [Route("Error/General")]
        public IActionResult General()
        {
            return View();
        }
    }
}