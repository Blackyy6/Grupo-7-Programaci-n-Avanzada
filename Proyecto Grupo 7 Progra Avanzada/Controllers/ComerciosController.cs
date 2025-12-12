using System;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Proyecto_Grupo_7_Progra_Avanzada.Data;
using Proyecto_Grupo_7_Progra_Avanzada.Models;

namespace Proyecto_Grupo_7_Progra_Avanzada
{
    public class ComerciosController : Controller
    {
        private readonly AppDbContext _context;

        public ComerciosController(AppDbContext context)
        {
            _context = context;
        }

        // GET: Comercios
        public async Task<IActionResult> Index()
        {
            return View(await _context.Comercios.ToListAsync());
        }

        // GET: Comercios/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var comercio = await _context.Comercios
                .FirstOrDefaultAsync(m => m.IdComercio == id);

            if (comercio == null)
            {
                return NotFound();
            }

            // Prueba de manejo de error: Provocar un NullReferenceException
            string? texto = null;
            int largo = texto.Length;

            return View(comercio);
        }

        // GET: Comercios/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Comercios/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        // [Bind] incluye solo los campos que el usuario debe llenar (prevención de overposting)
        public async Task<IActionResult> Create([Bind("Identificacion,TipoIdentificacion,Nombre,TipoDeComercio,Telefono,CorreoElectronico,Direccion")] Comercio comercio)
        {
            if (ModelState.IsValid)
            {
                // REQUERIMIENTO: Validación de duplicados
                bool yaExiste = await _context.Comercios
                    .AnyAsync(c => c.Identificacion == comercio.Identificacion);

                if (yaExiste)
                {
                    ModelState.AddModelError("Identificacion", "La identificación ingresada ya existe.");
                    return View(comercio);
                }

                // REQUERIMIENTO: Asignar valores por defecto
                comercio.FechaDeRegistro = DateTime.Now;
                comercio.Estado = true; // Activo
                comercio.FechaDeModificacion = null;

                _context.Add(comercio);
                await _context.SaveChangesAsync();

                // Registrar evento en la Bitácora
                var bitacora = new Bitacora
                {
                    TablaDeEvento = "Comercios",
                    TipoDeEvento = "Registrar",
                    FechaDeEvento = DateTime.Now,
                    DescripcionDeEvento = $"Se registró un nuevo comercio con nombre '{comercio.Nombre}'.",
                    DatosAnteriores = null,
                    DatosPosteriores = JsonSerializer.Serialize(comercio)
                };

                _context.Bitacora.Add(bitacora);
                await _context.SaveChangesAsync();
                //----------------------------------------------------------------------------------------------

                TempData["SuccessMessage"] = "Comercio registrado exitosamente.";
                return RedirectToAction(nameof(Index));
            }

            return View(comercio);
        }

        // GET: Comercios/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var comercio = await _context.Comercios.FindAsync(id);
            if (comercio == null)
            {
                return NotFound();
            }
            return View(comercio);
        }
        // POST: Comercios/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        // CORRECCIÓN CLAVE: 
        // Incluimos IdComercio, Identificacion y TipoIdentificacion en el [Bind].
        // Esto asegura que sus valores ocultos viajen y pasen la validación [Required].
        public async Task<IActionResult> Edit(int id, [Bind("IdComercio,Identificacion,TipoIdentificacion,Nombre,TipoDeComercio,Telefono,CorreoElectronico,Direccion,Estado")] Comercio comercio)
        {
            if (id != comercio.IdComercio)
            {
                return NotFound();
            }

            // 1. Validamos que los campos enviados cumplan con los DataAnnotations (incluyendo los campos fijos ocultos)
            if (ModelState.IsValid) // <-- ESTO AHORA DEBERÍA SER TRUE
            {
                // Guardamos los datos anteriores antes de editar
                var datosAnteriores = await _context.Comercios
                    .AsNoTracking()
                    .FirstOrDefaultAsync(c => c.IdComercio == id);

                if (datosAnteriores == null)
                {
                    return NotFound();
                }
                //-------------------------------------------------




                // ... (Tu lógica de edición SetValues/Attach va aquí)
                var comercioToUpdate = await _context.Comercios
                    .AsNoTracking()
                    .FirstOrDefaultAsync(c => c.IdComercio == id);

                if (comercioToUpdate == null)
                {
                    return NotFound();
                }

                try
                {
                    // Adjuntamos el objeto 'comercio' (con los datos del formulario) al contexto
                    _context.Comercios.Attach(comercio);

                    // Asignar fecha de modificación
                    comercio.FechaDeModificacion = DateTime.Now;

                    // Marcamos el objeto como modificado.
                    _context.Entry(comercio).State = EntityState.Modified;

                    // EXCLUIMOS los campos que no queremos que se editen, A PESAR de que estaban en el [Bind].
                    // Esto garantiza que el valor en la BD NO CAMBIE, aunque el formulario los haya enviado.
                    _context.Entry(comercio).Property(x => x.Identificacion).IsModified = false;
                    _context.Entry(comercio).Property(x => x.TipoIdentificacion).IsModified = false;
                    _context.Entry(comercio).Property(x => x.FechaDeRegistro).IsModified = false;

                    // Si quieres que Estado se edite, asegúrate de que no esté en la lista de exclusión.

                    await _context.SaveChangesAsync();


                    //Registrar evento en Bitácora
                    var bitacora = new Bitacora
                    {
                        TablaDeEvento = "Comercios",
                        TipoDeEvento = "Editar",
                        FechaDeEvento = DateTime.Now,
                        DescripcionDeEvento = $"Se editó el comercio con ID {comercio.IdComercio}.",
                        DatosAnteriores = JsonSerializer.Serialize(datosAnteriores),
                        DatosPosteriores = JsonSerializer.Serialize(comercio)
                    };

                    _context.Bitacora.Add(bitacora);
                    await _context.SaveChangesAsync();
                    //-----------------------------------------------------------------------------------



                    TempData["SuccessMessage"] = "Comercio actualizado exitosamente.";
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException)
                {
                    // ... (Manejo de concurrencia)
                }
            }
            // Si falla el ModelState.IsValid (aunque ya no debería fallar por los campos fijos),
            // se regresa a la vista para mostrar errores.
            return View(comercio);
        }

        // GET: Comercios/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var comercio = await _context.Comercios
                .FirstOrDefaultAsync(m => m.IdComercio == id);
            if (comercio == null)
            {
                return NotFound();
            }

            return View(comercio);
        }

        // POST: Comercios/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            // btenemos el comercio antes de eliminarlo
            var comercio = await _context.Comercios
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.IdComercio == id);
            //----------------------------------------------------

            // var comercio = await _context.Comercios.FindAsync(id);
            if (comercio != null)
            {
                _context.Comercios.Remove(comercio);

                // Registrar evento en la Bitácora
                var bitacora = new Bitacora
                {
                    TablaDeEvento = "Comercios",
                    TipoDeEvento = "Eliminar",
                    FechaDeEvento = DateTime.Now,
                    DescripcionDeEvento = $"Se eliminó el comercio con ID {comercio.IdComercio} y nombre '{comercio.Nombre}'.",
                    DatosAnteriores = JsonSerializer.Serialize(comercio),
                    DatosPosteriores = null
                };
                _context.Bitacora.Add(bitacora);
                await _context.SaveChangesAsync();
                //-----------------------------------------------------------------------------------------------------------------
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool ComercioExists(int id)
        {
            return _context.Comercios.Any(e => e.IdComercio == id);
        }
    }
}