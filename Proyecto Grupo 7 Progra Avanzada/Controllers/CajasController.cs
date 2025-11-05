using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Proyecto_Grupo_7_Progra_Avanzada.Data;
using Proyecto_Grupo_7_Progra_Avanzada.Models;

namespace Proyecto_Grupo_7_Progra_Avanzada.Controllers
{
    public class CajasController : Controller
    {
        private readonly AppDbContext _context;

        public CajasController(AppDbContext context)
        {
            _context = context;
        }

        // GET: Cajas
        public async Task<IActionResult> Index()
        {
            return View(await _context.Cajas.ToListAsync());
        }

        // GET: Cajas/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var caja = await _context.Cajas
                .FirstOrDefaultAsync(m => m.IdCaja == id);
            if (caja == null)
            {
                return NotFound();
            }

            return View(caja);
        }

        // GET: Cajas/Create
        public IActionResult Create()
        {
            ViewData["IdComercio"] = new SelectList(_context.Comercios.Where(c => c.Estado), "IdComercio", "Nombre");
            return View();   
        }

        // POST: Cajas/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("IdComercio,Nombre,Descripcion,TelefonoSINPE,Estado")] Caja caja)
        {

            if (ModelState.IsValid)
            {


                caja.FechaDeRegistro = DateTime.Now;
                caja.FechaDeModificacion = null;

                _context.Add(caja);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            ViewData["IdComercio"] = new SelectList(_context.Comercios.Where(c => c.Estado), "IdComercio", "Nombre", caja.IdComercio);
            return View(caja);

        }

        // GET: Cajas/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var caja = await _context.Cajas.FindAsync(id);
            if (caja == null)
            {
                return NotFound();
            }
            return View(caja);
        }

        // POST: Cajas/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("IdCaja,IdComercio,Nombre,Descripcion,TelefonoSINPE,FechaDeRegistro,FechaDeModificacion,Estado")] Caja caja)
        {
            if (id != caja.IdCaja)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    caja.FechaDeModificacion = DateTime.Now;
                    _context.Update(caja);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!CajaExists(caja.IdCaja))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(caja);
        }

        // GET: Cajas/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var caja = await _context.Cajas
                .FirstOrDefaultAsync(m => m.IdCaja == id);
            if (caja == null)
            {
                return NotFound();
            }

            return View(caja);
        }

        // POST: Cajas/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var caja = await _context.Cajas.FindAsync(id);
            if (caja != null)
            {
                _context.Cajas.Remove(caja);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool CajaExists(int id)
        {
            return _context.Cajas.Any(e => e.IdCaja == id);
        }
    }
}
