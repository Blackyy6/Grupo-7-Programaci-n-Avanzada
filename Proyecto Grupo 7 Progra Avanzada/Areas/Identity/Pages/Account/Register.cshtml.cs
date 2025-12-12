using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Proyecto_Grupo_7_Progra_Avanzada.Data;
using Proyecto_Grupo_7_Progra_Avanzada.Models;

namespace Proyecto_Grupo_7_Progra_Avanzada.Areas.Identity.Pages.Account
{
    [AllowAnonymous]
    public class RegisterModel : PageModel
    {
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly ILogger<RegisterModel> _logger;
        private readonly AppDbContext _context;

        public RegisterModel(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            RoleManager<IdentityRole> roleManager,
            ILogger<RegisterModel> logger,
            AppDbContext context)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _roleManager = roleManager;
            _logger = logger;
            _context = context;
        }

        [BindProperty]
        public InputModel Input { get; set; }

        public string ReturnUrl { get; set; }

        // Lista para el Dropdown
        public SelectList RoleSelectList { get; set; }

        public class InputModel
        {
            [Required(ErrorMessage = "El correo es obligatorio.")]
            [EmailAddress]
            [Display(Name = "Email")]
            public string Email { get; set; }

            [Required(ErrorMessage = "La contraseña es obligatoria.")]
            [StringLength(100, ErrorMessage = "La {0} debe tener al menos {2} caracteres.", MinimumLength = 6)]
            [DataType(DataType.Password)]
            [Display(Name = "Contraseña")]
            public string Password { get; set; }

            [DataType(DataType.Password)]
            [Display(Name = "Confirmar contraseña")]
            [Compare("Password", ErrorMessage = "Las contraseñas no coinciden.")]
            public string ConfirmPassword { get; set; }

            [Required(ErrorMessage = "Debe seleccionar un Rol.")]
            [Display(Name = "Rol")]
            public string Role { get; set; }
        }

        public async Task OnGetAsync(string returnUrl = null)
        {
            ReturnUrl = returnUrl;
            RoleSelectList = new SelectList(new[] { "Administrador", "Cajero" });
        }

        public async Task<IActionResult> OnPostAsync(string returnUrl = null)
        {
            returnUrl ??= Url.Content("~/");
            RoleSelectList = new SelectList(new[] { "Administrador", "Cajero" });

            if (ModelState.IsValid)
            {
                // 1. VALIDACIÓN DE CAJEROS
                if (Input.Role == "Cajero")
                {
                    // CAMBIO: Usamos 'CorreoElectronico' en lugar de 'Correo'
                    var usuarioExistente = await _context.Usuarios
                        .FirstOrDefaultAsync(u => u.CorreoElectronico == Input.Email);

                    if (usuarioExistente == null)
                    {
                        ModelState.AddModelError(string.Empty, "Error: El correo no está registrado como empleado (Tabla Usuarios).");
                        return Page();
                    }
                }

                // 2. CREAR USUARIO IDENTITY
                var user = new ApplicationUser { UserName = Input.Email, Email = Input.Email };
                var result = await _userManager.CreateAsync(user, Input.Password);

                if (result.Succeeded)
                {
                    _logger.LogInformation("Usuario creado exitosamente.");

                    // 3. ASIGNAR ROL
                    if (!await _roleManager.RoleExistsAsync(Input.Role))
                    {
                        await _roleManager.CreateAsync(new IdentityRole(Input.Role));
                    }
                    await _userManager.AddToRoleAsync(user, Input.Role);

                    // 4. ACTUALIZAR IDNETUSER EN TABLA USUARIOS (Solo Cajeros)
                    if (Input.Role == "Cajero")
                    {
                        // CAMBIO: Usamos 'CorreoElectronico'
                        var usuarioExistente = await _context.Usuarios
                            .FirstOrDefaultAsync(u => u.CorreoElectronico == Input.Email);

                        if (usuarioExistente != null)
                        {
                            // CAMBIO: Convertimos el ID de string a Guid
                            if (Guid.TryParse(user.Id, out Guid guidUser))
                            {
                                usuarioExistente.IdNetUser = guidUser;
                                _context.Usuarios.Update(usuarioExistente);
                                await _context.SaveChangesAsync();
                            }
                        }
                    }

                    await _signInManager.SignInAsync(user, isPersistent: false);
                    return LocalRedirect(returnUrl);
                }

                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }

            return Page();
        }
    }
}