using System;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Proyecto_Grupo_7_Progra_Avanzada.Data;
using Proyecto_Grupo_7_Progra_Avanzada.Models;

namespace Proyecto_Grupo_7_Progra_Avanzada.Controllers
{
    public class UsuariosController : Controller
    {
        private readonly AppDbContext _context;

        public UsuariosController(AppDbContext context)
        {
            _context = context;
        }

        // GET: Usuarios
        public async Task<IActionResult> Index()
        {
            var usuarios = await _context.Usuarios
                .Include(u => u.Comercio)
                .ToListAsync();

            return View(usuarios);
        }

        // GET: Usuarios/Create
        public IActionResult Create()
        {
            ViewData["IdComercio"] = new SelectList(_context.Comercios.Where(c => c.Estado), "IdComercio", "Nombre");
            return View();
        }

        // POST: Usuarios/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("IdComercio,Nombres,PrimerApellido,SegundoApellido,Identificacion,CorreoElectronico")] Usuario usuario)
        {
            if (ModelState.IsValid)
            {
                // Validar que no exista otro usuario con la misma identificación
                bool yaExiste = await _context.Usuarios
                    .AnyAsync(u => u.Identificacion == usuario.Identificacion);

                if (yaExiste)
                {
                    ModelState.AddModelError("Identificacion", "La identificación ingresada ya existe.");
                    ViewData["IdComercio"] = new SelectList(_context.Comercios.Where(c => c.Estado), "IdComercio", "Nombre", usuario.IdComercio);
                    return View(usuario);
                }

                // Asignar valores por defecto que no se solicitan al usuario
                usuario.FechaDeRegistro = DateTime.Now;
                usuario.Estado = true; // Activo
                usuario.FechaDeModificacion = null;

                _context.Add(usuario);
                await _context.SaveChangesAsync();

                // Registrar evento en la bitácora
                var bitacora = new Bitacora
                {
                    TablaDeEvento = "Usuarios",
                    TipoDeEvento = "Registrar",
                    FechaDeEvento = DateTime.Now,
                    DescripcionDeEvento = $"Se registró un nuevo usuario con identificación '{usuario.Identificacion}'.",
                    DatosAnteriores = null,
                    DatosPosteriores = JsonSerializer.Serialize(usuario)
                };

                _context.Bitacora.Add(bitacora);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Usuario registrado correctamente.";
                return RedirectToAction(nameof(Index));
            }

            ViewData["IdComercio"] = new SelectList(_context.Comercios.Where(c => c.Estado), "IdComercio", "Nombre", usuario.IdComercio);
            return View(usuario);
        }

        // GET: Usuarios/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var usuario = await _context.Usuarios.FindAsync(id);
            if (usuario == null)
            {
                return NotFound();
            }

            ViewData["IdComercio"] = new SelectList(_context.Comercios, "IdComercio", "Nombre", usuario.IdComercio);
            return View(usuario);
        }

        // POST: Usuarios/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("IdUsuario,IdComercio,Nombres,PrimerApellido,SegundoApellido,Identificacion,CorreoElectronico,FechaDeRegistro,FechaDeModificacion,Estado")] Usuario usuario)
        {
            if (id != usuario.IdUsuario)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    // Recuperar datos anteriores para la bitácora
                    var datosAnteriores = await _context.Usuarios
                        .AsNoTracking()
                        .FirstOrDefaultAsync(u => u.IdUsuario == id);

                    // Validar que no exista otro usuario con la misma identificación
                    bool yaExiste = await _context.Usuarios
                        .AnyAsync(u => u.IdUsuario != id && u.Identificacion == usuario.Identificacion);

                    if (yaExiste)
                    {
                        ModelState.AddModelError("Identificacion", "La identificación ingresada ya existe.");
                        ViewData["IdComercio"] = new SelectList(_context.Comercios, "IdComercio", "Nombre", usuario.IdComercio);
                        return View(usuario);
                    }

                    usuario.FechaDeModificacion = DateTime.Now;

                    _context.Update(usuario);
                    await _context.SaveChangesAsync();

                    // Registrar evento en la bitácora
                    var bitacora = new Bitacora
                    {
                        TablaDeEvento = "Usuarios",
                        TipoDeEvento = "Editar",
                        FechaDeEvento = DateTime.Now,
                        DescripcionDeEvento = $"Se editó el usuario con ID {usuario.IdUsuario}.",
                        DatosAnteriores = JsonSerializer.Serialize(datosAnteriores),
                        DatosPosteriores = JsonSerializer.Serialize(usuario)
                    };

                    _context.Bitacora.Add(bitacora);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!UsuarioExists(usuario.IdUsuario))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }

                TempData["SuccessMessage"] = "Usuario actualizado correctamente.";
                return RedirectToAction(nameof(Index));
            }

            ViewData["IdComercio"] = new SelectList(_context.Comercios, "IdComercio", "Nombre", usuario.IdComercio);
            return View(usuario);
        }

        private bool UsuarioExists(int id)
        {
            return _context.Usuarios.Any(e => e.IdUsuario == id);
        }
    }
}
