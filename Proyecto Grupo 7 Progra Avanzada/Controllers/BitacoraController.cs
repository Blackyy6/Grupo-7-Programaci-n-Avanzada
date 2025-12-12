using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Proyecto_Grupo_7_Progra_Avanzada.Data;
using Proyecto_Grupo_7_Progra_Avanzada.Models;

namespace Proyecto_Grupo_7_Progra_Avanzada.Controllers
{
    public class AuthController : Controller
    {
        private readonly AppDbContext _context;

        public AuthController(AppDbContext context)
        {
            _context = context;
        }

        // GET: Bitacora
        public async Task<IActionResult> Index()
        {
            var eventos = await _context.Bitacora
                .OrderByDescending(e => e.FechaDeEvento)
                .ToListAsync();

            return View(eventos);
        }

        // Método auxiliar para registrar eventos desde otros módulos
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