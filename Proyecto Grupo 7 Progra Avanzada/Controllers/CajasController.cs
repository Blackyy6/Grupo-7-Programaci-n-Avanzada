using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Proyecto_Grupo_7_Progra_Avanzada.Data;
using Proyecto_Grupo_7_Progra_Avanzada.Models;

namespace Proyecto_Grupo_7_Progra_Avanzada.Controllers
{
    // Autorización general: Todos los roles autorizados
    [Authorize(Roles = "Administrador, Contador, Cajero")]
    public class CajasController : Controller
    {
        private readonly AppDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly BitacoraController _bitacora;

        public CajasController(AppDbContext context, UserManager<ApplicationUser> userManager, BitacoraController bitacora)
        {
            _context = context;
            _userManager = userManager;
            _bitacora = bitacora;
        }

        // GET: Cajas/Index
        public async Task<IActionResult> Index()
        {
            IQueryable<Caja> cajas;
            ViewData["Title"] = "Cajas Registradas";

            // 1. Bandera para ocultar/mostrar botones en la vista
            ViewData["EsCajero"] = User.IsInRole("Cajero");

            // 2. Lógica de Filtrado
            if (User.IsInRole("Administrador") || User.IsInRole("Contador"))
            {
                // Administrador/Contador: Ve TODAS las cajas
                cajas = _context.Cajas.Include(c => c.Comercio);
            }
            else if (User.IsInRole("Cajero"))
            {
                // Cajero: Solo ve sus cajas
                var userId = _userManager.GetUserId(User);

                var usuarioCajero = await _context.Usuarios
                    .Include(u => u.Comercio)
                    .AsNoTracking()
                    .FirstOrDefaultAsync(u => u.IdNetUser.HasValue && u.IdNetUser.Value.ToString() == userId);

                if (usuarioCajero == null)
                {
                    TempData["Error"] = "Su usuario Cajero no está enlazado a un Comercio. Contacte al Administrador.";
                    return View(new List<Caja>());
                }

                int idComercioCajero = usuarioCajero.IdComercio;
                cajas = _context.Cajas
                    .Include(c => c.Comercio)
                    .Where(c => c.IdComercio == idComercioCajero);

                ViewData["Title"] = $"Cajas de {usuarioCajero.Comercio?.Nombre ?? "Su Comercio"}";
            }
            else
            {
                return View(new List<Caja>());
            }

            return View(await cajas.ToListAsync());
        }

        // GET: Cajas/Details/5
        // Cajero puede ver el detalle de SU caja
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var caja = await _context.Cajas.Include(c => c.Comercio).FirstOrDefaultAsync(m => m.IdCaja == id);

            if (caja == null) return NotFound();

            // CLAVE DE SEGURIDAD: Denegar acceso si es Cajero y la caja no es suya.
            if (User.IsInRole("Cajero"))
            {
                var userId = _userManager.GetUserId(User);
                var usuarioCajero = await _context.Usuarios.AsNoTracking().FirstOrDefaultAsync(u => u.IdNetUser.HasValue && u.IdNetUser.Value.ToString() == userId);

                if (usuarioCajero == null || caja.IdComercio != usuarioCajero.IdComercio)
                {
                    return Forbid();
                }
            }

            return View(caja);
        }

        // GET: Cajas/Create
        // RESTRICCIÓN: Solo Administrador y Contador
        [Authorize(Roles = "Administrador, Contador")]
        public IActionResult Create()
        {
            ViewData["IdComercio"] = new SelectList(_context.Comercios.Where(c => c.Estado), "IdComercio", "Nombre");
            return View();
        }

        // POST: Cajas/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        // RESTRICCIÓN: Solo Administrador y Contador
        [Authorize(Roles = "Administrador, Contador")]
        public async Task<IActionResult> Create([Bind("IdComercio,Nombre,Descripcion,TelefonoSINPE,Estado")] Caja caja)
        {
            if (ModelState.IsValid)
            {
                caja.FechaDeRegistro = DateTime.Now;
                caja.FechaDeModificacion = null;

                _context.Add(caja);
                await _context.SaveChangesAsync();

                // Registro en bitácora
                await _bitacora.RegistrarEvento(
                    "Cajas",
                    "Registrar",
                    $"Se registró la caja '{caja.Nombre}' (ID: {caja.IdCaja}).",
                    null,
                    caja
                );

                return RedirectToAction(nameof(Index));
            }

            ViewData["IdComercio"] = new SelectList(_context.Comercios.Where(c => c.Estado), "IdComercio", "Nombre", caja.IdComercio);
            return View(caja);
        }

        // GET: Cajas/Edit/5
        // RESTRICCIÓN: Solo Administrador y Contador
        [Authorize(Roles = "Administrador, Contador")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var caja = await _context.Cajas.FindAsync(id);
            if (caja == null) return NotFound();

            ViewData["IdComercio"] = new SelectList(_context.Comercios.Where(c => c.Estado), "IdComercio", "Nombre", caja.IdComercio);
            return View(caja);
        }

        // POST: Cajas/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        // RESTRICCIÓN: Solo Administrador y Contador
        [Authorize(Roles = "Administrador, Contador")]
        public async Task<IActionResult> Edit(int id, [Bind("IdCaja,IdComercio,Nombre,Descripcion,TelefonoSINPE,FechaDeRegistro,FechaDeModificacion,Estado")] Caja caja)
        {
            if (id != caja.IdCaja) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    var datosAnteriores = await _context.Cajas
                        .AsNoTracking()
                        .FirstOrDefaultAsync(c => c.IdCaja == id);

                    caja.FechaDeModificacion = DateTime.Now;
                    _context.Update(caja);
                    await _context.SaveChangesAsync();

                    // Registrar evento en la bitácora
                    await _bitacora.RegistrarEvento(
                        "Cajas",
                        "Editar",
                        $"Se editó la caja '{caja.Nombre}' (ID: {caja.IdCaja}).",
                        datosAnteriores,
                        caja
                    );
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!CajaExists(caja.IdCaja))
                    {
                        return NotFound();
                    }
                    throw;
                }
                return RedirectToAction(nameof(Index));
            }

            ViewData["IdComercio"] = new SelectList(_context.Comercios.Where(c => c.Estado), "IdComercio", "Nombre", caja.IdComercio);
            return View(caja);
        }

        // GET: Cajas/Delete/5
        // RESTRICCIÓN: Solo Administrador y Contador
        [Authorize(Roles = "Administrador, Contador")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var caja = await _context.Cajas
                .Include(c => c.Comercio)
                .FirstOrDefaultAsync(m => m.IdCaja == id);

            if (caja == null) return NotFound();

            return View(caja);
        }

        // POST: Cajas/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        // RESTRICCIÓN: Solo Administrador y Contador
        [Authorize(Roles = "Administrador, Contador")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var caja = await _context.Cajas
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.IdCaja == id);

            if (caja != null)
            {
                _context.Cajas.Remove(caja);
                await _context.SaveChangesAsync();

                // Registrar evento en la Bitácora
                await _bitacora.RegistrarEvento(
                    "Cajas",
                    "Eliminar",
                    $"Se eliminó la caja '{caja.Nombre}' (ID: {caja.IdCaja}).",
                    caja,
                    null
                );
            }

            return RedirectToAction(nameof(Index));
        }

        private bool CajaExists(int id)
        {
            return _context.Cajas.Any(e => e.IdCaja == id);
        }
    }
}