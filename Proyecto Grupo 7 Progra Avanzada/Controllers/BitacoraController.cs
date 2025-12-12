using System;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization; // NECESARIO
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Proyecto_Grupo_7_Progra_Avanzada.Data;
using Proyecto_Grupo_7_Progra_Avanzada.Models;

namespace Proyecto_Grupo_7_Progra_Avanzada.Controllers
{
    // CLAVE: Renombrar la clase y autorizar solo para Admin/Contador
    [Authorize(Roles = "Administrador, Contador")]
    public class BitacoraController : Controller
    {
        private readonly AppDbContext _context;

        public BitacoraController(AppDbContext context)
        {
            _context = context;
        }

        // GET: /Bitacora/Index
        public async Task<IActionResult> Index()
        {
            var eventos = await _context.Bitacora
                // Corrección: Tu vista usa FechaDeEvento
                .OrderByDescending(e => e.FechaDeEvento)
                .ToListAsync();

            return View(eventos);
        }

        // Método auxiliar para registrar eventos (se mantiene igual, es correcto)
        [ApiExplorerSettings(IgnoreApi = true)] // Oculta este método de documentación API
        public async Task RegistrarEvento(
            string tabla,
            string tipo,
            string descripcion,
            object? datosAnteriores = null,
            object? datosPosteriores = null,
            Exception? error = null)
        {
            var evento = new Bitacora
            {
                // ... (El resto del objeto Bitacora, asumiendo que tienes IdUsuario/UsuarioId)
                TablaDeEvento = tabla,
                TipoDeEvento = tipo,
                FechaDeEvento = DateTime.Now,
                DescripcionDeEvento = error?.Message ?? descripcion,
                StackTrace = error?.StackTrace,
                DatosAnteriores = datosAnteriores != null ? JsonSerializer.Serialize(datosAnteriores) : null,
                DatosPosteriores = datosPosteriores != null ? JsonSerializer.Serialize(datosPosteriores) : null
            };

            _context.Bitacora.Add(evento);
            await _context.SaveChangesAsync();
        }
    }
}