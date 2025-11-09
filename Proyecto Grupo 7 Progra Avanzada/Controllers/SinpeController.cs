using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Proyecto_Grupo_7_Progra_Avanzada.Data;
using Proyecto_Grupo_7_Progra_Avanzada.Models;

namespace Proyecto_Grupo_7_Progra_Avanzada.Controllers
{
    public class SinpeController : Controller
    {
        private readonly AppDbContext _context;

        public SinpeController(AppDbContext context)
        {
            _context = context;
        }

        // GET: /Sinpe/Registrar
        [HttpGet]
        public IActionResult Registrar()
        {
            return View();
        }

        // POST: /Sinpe/Registrar
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Registrar([Bind("TelefonoOrigen,NombreOrigen,TelefonoDestinatario,NombreDestinatario,Monto,Descripcion")] SinpePago input)
        {
            if (!ModelState.IsValid)
            {
                return View(input);
            }

            try
            {
                // Validar caja activa con telefono
                var caja = await _context.Cajas.AsNoTracking().FirstOrDefaultAsync(c => c.TelefonoSINPE == input.TelefonoDestinatario);

                if (caja == null)
                {
                    ModelState.AddModelError(string.Empty, "No existe una caja con ese teléfono SINPE.");
                    return View(input);
                }

                if (!caja.Estado)
                {
                    ModelState.AddModelError(string.Empty, "No se permite pagar a una caja inactiva.");
                    return View(input);
                }

                if (string.IsNullOrWhiteSpace(caja.TelefonoSINPE))
                {
                    ModelState.AddModelError(string.Empty, "La caja no tiene teléfono SINPE registrado.");
                    return View(input);
                }

                // Normalizar, setear campos server-side
                var pago = new SinpePago
                {
                    TelefonoOrigen = input.TelefonoOrigen.Trim(),
                    NombreOrigen = input.NombreOrigen.Trim(),
                    TelefonoDestinatario = input.TelefonoDestinatario.Trim(),
                    NombreDestinatario = input.NombreDestinatario.Trim(),
                    Monto = input.Monto,
                    Descripcion = string.IsNullOrWhiteSpace(input.Descripcion) ? null : input.Descripcion.Trim(),
                    FechaDeRegistro = DateTime.Now, // no se pide al usuario
                    Estado = false // No sincronizado
                };

                _context.SinpePagos.Add(pago);
                await _context.SaveChangesAsync();

                TempData["Ok"] = $"Pago SINPE registrado con Id #{pago.IdSinpe}.";
                return RedirectToAction(nameof(Registrar));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, "Ocurrió un error inesperado al registrar el pago.");
                return View(input);
            }
        }
    }
}
