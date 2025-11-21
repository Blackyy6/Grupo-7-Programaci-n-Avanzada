using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Proyecto_Grupo_7_Progra_Avanzada.Data;
using Proyecto_Grupo_7_Progra_Avanzada.Models;
using System.Linq;

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
        public async Task<IActionResult> Registrar([Bind("TelefonoOrigen,NombreOrigen,TelefonoDestinatario,NombreDestinatario,Monto,Descripcion")] Sinpes input)
        {
            if (!ModelState.IsValid)
            {
                return View(input);
            }

            try
            {
                // Validar caja activa con telefono
                // Asumimos que Caja tiene la propiedad TelefonoSINPE
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

                // Normalizar, setear campos server-side
                var pago = new Sinpes
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

                _context.Sinpes.Add(pago);
                await _context.SaveChangesAsync();

                // 🔹 Registrar evento en la Bitácora
                var bitacora = new Bitacora
                {
                    TablaDeEvento = "SINPE",
                    TipoDeEvento = "Registrar",
                    FechaDeEvento = DateTime.Now,
                    DescripcionDeEvento = $"Se registró un pago SINPE a la caja con teléfono {pago.TelefonoDestinatario}.",
                    DatosAnteriores = null,
                    DatosPosteriores = System.Text.Json.JsonSerializer.Serialize(pago)
                };

                _context.Bitacora.Add(bitacora);
                await _context.SaveChangesAsync();
                //--------------------------------------------------------------------------------------------------------------

                TempData["Ok"] = "Se realizó el pago correctamente.";
                return RedirectToAction(nameof(Registrar));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, "Ocurrió un error inesperado al registrar el pago.");
                // Puedes loggear el error aquí: Console.WriteLine(ex.Message);
                return View(input);
            }
        }

        // GET: /Sinpe/VerPorCaja/{telefono}
        /// <summary>
        /// Muestra la lista de pagos SINPE recibidos por una caja específica.
        /// </summary>
        /// <param name="telefono">El teléfono SINPE de la caja destinataria.</param>
        /// <returns>Vista con los pagos filtrados.</returns>
        [HttpGet]
        public async Task<IActionResult> VerPorCaja(string telefono)
        {
            if (string.IsNullOrWhiteSpace(telefono))
            {
                TempData["Error"] = "Teléfono SINPE no proporcionado.";
                return RedirectToAction("Index", "Cajas"); // Redirigir si no hay teléfono
            }

            // Buscar la caja para obtener su nombre y mostrarlo en la vista
            var caja = await _context.Cajas.AsNoTracking().FirstOrDefaultAsync(c => c.TelefonoSINPE == telefono);

            // Si la caja existe, se usa su nombre. Si no existe, se usa solo el número.
            ViewData["NombreCaja"] = caja != null
                                    ? $"{caja.Nombre} ({telefono})"
                                    : $"Caja con Teléfono {telefono}";

            // Filtrar los pagos SINPE donde el TelefonoDestinatario coincida con el parámetro.
            var sinpesDeCaja = await _context.Sinpes
                .Where(p => p.TelefonoDestinatario == telefono)
                // Opcional: ordenar por fecha descendente
                .OrderByDescending(p => p.FechaDeRegistro)
                .ToListAsync();

            return View(sinpesDeCaja);
        }

        // POST: /Sinpe/Sincronizar/{id}
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Sincronizar(int id)
        {
            try
            {
                var sinpe = await _context.Sinpes.FirstOrDefaultAsync(s => s.IdSinpe == id);

                if (sinpe == null)
                {
                    TempData["Error"] = "El SINPE no existe.";
                    return RedirectToAction("Index", "Cajas");
                }

                if (sinpe.Estado) // ya sincronizado
                {
                    TempData["Info"] = "Este SINPE ya está sincronizado.";
                    return RedirectToAction(nameof(VerPorCaja), new { telefono = sinpe.TelefonoDestinatario });
                }

                // Guardar datos anteriores para bitácora
                var datosAnteriores = new
                {
                    sinpe.IdSinpe,
                    sinpe.Estado,
                    sinpe.TelefonoDestinatario,
                    sinpe.Monto,
                    sinpe.FechaDeRegistro
                };

                // Actualizar estado
                sinpe.Estado = true; // sincronizado
                var datosPosteriores = new
                {
                    sinpe.IdSinpe,
                    sinpe.Estado,
                    sinpe.TelefonoDestinatario,
                    sinpe.Monto,
                    sinpe.FechaDeRegistro
                };

                _context.Update(sinpe);

                // Registrar en Bitácora
                var evento = new Bitacora
                {
                    TablaDeEvento = "Sinpes",
                    TipoDeEvento = "Editar",
                    FechaDeEvento = DateTime.Now,
                    DescripcionDeEvento = $"SINPE {sinpe.IdSinpe} sincronizado en Caja {sinpe.TelefonoDestinatario}",
                    DatosAnteriores = System.Text.Json.JsonSerializer.Serialize(datosAnteriores),
                    DatosPosteriores = System.Text.Json.JsonSerializer.Serialize(datosPosteriores)
                };

                _context.Bitacora.Add(evento);

                await _context.SaveChangesAsync();

                TempData["Ok"] = "SINPE sincronizado correctamente.";
                return RedirectToAction(nameof(VerPorCaja), new { telefono = sinpe.TelefonoDestinatario });
            }
            catch (Exception ex)
            {
                // Registrar error en Bitácora
                var errorEvento = new Bitacora
                {
                    TablaDeEvento = "Sinpes",
                    TipoDeEvento = "Error",
                    FechaDeEvento = DateTime.Now,
                    DescripcionDeEvento = ex.Message,
                    StackTrace = ex.StackTrace
                };

                _context.Bitacora.Add(errorEvento);
                await _context.SaveChangesAsync();

                TempData["Error"] = "Ocurrió un error al sincronizar el SINPE.";
                return RedirectToAction("Index", "Cajas");
            }
        }
    }
}