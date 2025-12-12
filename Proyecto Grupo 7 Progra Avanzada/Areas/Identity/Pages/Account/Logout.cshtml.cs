using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using Proyecto_Grupo_7_Progra_Avanzada.Models; // Asegura que se use ApplicationUser

namespace Proyecto_Grupo_7_Progra_Avanzada.Areas.Identity.Pages.Account
{
    // Permitir acceso anónimo, ya que estás cerrando sesión
    [AllowAnonymous]
    public class LogoutModel : PageModel
    {
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly ILogger<LogoutModel> _logger;

        public LogoutModel(SignInManager<ApplicationUser> signInManager, ILogger<LogoutModel> logger)
        {
            _signInManager = signInManager;
            _logger = logger;
        }

        // Se usa para enlaces de GET (aunque generalmente se prefiere POST)
        public void OnGet()
        {
        }

        // CLAVE: Método POST llamado por el formulario de _LoginPartial.cshtml
        public async Task<IActionResult> OnPost(string returnUrl = null)
        {
            // Cierra la sesión del usuario
            await _signInManager.SignOutAsync();
            _logger.LogInformation("Usuario cerró sesión.");

            if (returnUrl != null)
            {
                // Redirige a la URL especificada (ej: Home/Index)
                return LocalRedirect(returnUrl);
            }
            else
            {
                // Si no hay URL, redirige a la página de inicio o a la de login
                return RedirectToPage();
            }
        }
    }
}