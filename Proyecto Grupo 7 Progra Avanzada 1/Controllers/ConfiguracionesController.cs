using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Proyecto_Grupo_7_Progra_Avanzada.Data;
using Proyecto_Grupo_7_Progra_Avanzada.Models;

namespace Proyecto_Grupo_7_Progra_Avanzada.Controllers
{
    public class ConfiguracionesController : Controller
    {
        private readonly AppDbContext _context;

        public ConfiguracionesController(AppDbContext context)
        {
            _context = context;
        }

        // GET: Configuraciones
        public async Task<IActionResult> Index()
        {
            // Incluir la relación con Comercio para mostrar el nombre
            var configuraciones = await _context.Configuraciones
                .Include(c => c.Comercio)
                .ToListAsync();
            
            return View(configuraciones);
        }

        // GET: Configuraciones/Create
        public async Task<IActionResult> Create()
        {
            // Obtener lista de comercios activos para el dropdown
            var comerciosActivos = await _context.Comercios
                .Where(c => c.Estado == true)
                .OrderBy(c => c.Nombre)
                .ToListAsync();

            // Si hay un comercio seleccionado previamente (por error de validación), mantenerlo seleccionado
            int? selectedComercioId = null;
            if (TempData["SelectedComercioId"] != null)
            {
                selectedComercioId = (int)TempData["SelectedComercioId"];
            }

            ViewBag.Comercios = new SelectList(comerciosActivos, "IdComercio", "Nombre", selectedComercioId);
            return View();
        }

        // POST: Configuraciones/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("IdComercio,TipoConfiguracion,Comision")] Configuracion configuracion)
        {
            if (ModelState.IsValid)
            {
                // REQUERIMIENTO: Validar que no exista más de una configuración por comercio
                bool yaExiste = await _context.Configuraciones
                    .AnyAsync(c => c.IdComercio == configuracion.IdComercio);

                if (yaExiste)
                {
                    // REQUERIMIENTO: Mostrar mensaje en el navegador y redireccionar hacia la vista de crear
                    TempData["ErrorMessage"] = "Este comercio ya tiene una configuración registrada. Solo se permite una configuración por comercio.";
                    TempData["SelectedComercioId"] = configuracion.IdComercio;
                    return RedirectToAction(nameof(Create));
                }

                // REQUERIMIENTO: Asignar valores por defecto (no se solicitan al usuario)
                configuracion.FechaDeRegistro = DateTime.Now;
                configuracion.Estado = true; // Activo por defecto
                configuracion.FechaDeModificacion = null;

                _context.Add(configuracion);
                await _context.SaveChangesAsync();

                // Registrar en bitácora
                var bit = new Bitacora
                {
                    TablaDeEvento = "Configuraciones",
                    TipoDeEvento = "Registrar",
                    FechaDeEvento = DateTime.Now,
                    DescripcionDeEvento = $"Se creó una configuración para el comercio {configuracion.IdComercio}.",
                    DatosAnteriores = null,
                    DatosPosteriores = System.Text.Json.JsonSerializer.Serialize(configuracion)
                };
                //---------------------------------------------------------------------------------------------------
                _context.Bitacora.Add(bit);
                await _context.SaveChangesAsync();


                TempData["SuccessMessage"] = "Configuración registrada exitosamente.";
                return RedirectToAction(nameof(Index));
            }

            // Si hay errores de validación, recargar la lista de comercios
            var comercios = await _context.Comercios
                .Where(c => c.Estado == true)
                .OrderBy(c => c.Nombre)
                .ToListAsync();
            ViewBag.Comercios = new SelectList(comercios, "IdComercio", "Nombre", configuracion.IdComercio);

            return View(configuracion);
        }

        // GET: Configuraciones/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var configuracion = await _context.Configuraciones
                .Include(c => c.Comercio)
                .FirstOrDefaultAsync(m => m.IdConfiguracion == id);

            if (configuracion == null)
            {
                return NotFound();
            }

            return View(configuracion);
        }

        // POST: Configuraciones/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("IdConfiguracion,IdComercio,TipoConfiguracion,Comision,Estado")] Configuracion configuracion)
        {
            if (id != configuracion.IdConfiguracion)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    //obterner la data anterior para la bitacora
                    var beforeData = await _context.Configuraciones
                    .AsNoTracking()
                     .FirstOrDefaultAsync(c => c.IdConfiguracion == id);
                    //---------------------------------------------------

                    // Obtener la configuración existente
                    var configuracionToUpdate = await _context.Configuraciones
                        .AsNoTracking()
                        .FirstOrDefaultAsync(c => c.IdConfiguracion == id);

                    if (configuracionToUpdate == null)
                    {
                        return NotFound();
                    }

                    // Adjuntar el objeto al contexto
                    _context.Configuraciones.Attach(configuracion);

                    // REQUERIMIENTO: Actualizar fecha de modificación automáticamente
                    configuracion.FechaDeModificacion = DateTime.Now;

                    // Marcar como modificado
                    _context.Entry(configuracion).State = EntityState.Modified;

                    // EXCLUIR campos que no se deben editar
                    _context.Entry(configuracion).Property(x => x.IdComercio).IsModified = false;
                    _context.Entry(configuracion).Property(x => x.FechaDeRegistro).IsModified = false;

                    // Solo se permiten editar: TipoConfiguracion, Comision, Estado, FechaDeModificacion
                    // (FechaDeModificacion ya se actualizó arriba)

                    await _context.SaveChangesAsync();

                    // Registrar en bitacora
                    var bit = new Bitacora
                    {
                        TablaDeEvento = "Configuraciones",
                        TipoDeEvento = "Editar",
                        FechaDeEvento = DateTime.Now,
                        DescripcionDeEvento = $"Se editó la configuración con ID {configuracion.IdConfiguracion}.",
                        DatosAnteriores = System.Text.Json.JsonSerializer.Serialize(beforeData),
                        DatosPosteriores = System.Text.Json.JsonSerializer.Serialize(configuracion)
                    };

                    _context.Bitacora.Add(bit);
                    await _context.SaveChangesAsync();
                    //---------------------------------------------------------------------------------------------------

                    TempData["SuccessMessage"] = "Configuración actualizada exitosamente.";
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ConfiguracionExists(configuracion.IdConfiguracion))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
            }

            // Si hay errores, recargar la relación con Comercio para la vista
            var configuracionConComercio = await _context.Configuraciones
                .Include(c => c.Comercio)
                .FirstOrDefaultAsync(c => c.IdConfiguracion == id);
            
            if (configuracionConComercio != null)
            {
                configuracion.Comercio = configuracionConComercio.Comercio;
            }

            return View(configuracion);
        }

        private bool ConfiguracionExists(int id)
        {
            return _context.Configuraciones.Any(e => e.IdConfiguracion == id);
        }
    }
}

