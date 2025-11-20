using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Proyecto_Grupo_7_Progra_Avanzada.Data;
using Proyecto_Grupo_7_Progra_Avanzada.Models;

namespace Proyecto_Grupo_7_Progra_Avanzada.Controllers
{
    public class ReportesController : Controller
    {
        private readonly AppDbContext _context;
        // Se asume que tu AppDbContext tiene DbSets para Comercio, Caja, SinpePago, Reporte y Configuracion

        public ReportesController(AppDbContext context)
        {
            _context = context;
        }

        // ----------------------------------------------------------------------
        // 1. LISTAR REPORTES
        // ----------------------------------------------------------------------

        // GET: /Reportes/Index
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var reportes = await _context.Set<Reporte>()
                .Include(r => r.Comercio) // Se necesita el include para mostrar el nombre
                .OrderByDescending(r => r.FechaDelReporte)
                .ToListAsync();

            return View(reportes);
        }

        // ----------------------------------------------------------------------
        // 2. GENERAR Y ACTUALIZAR REPORTES
        // ----------------------------------------------------------------------

        // POST: /Reportes/Generar
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Generar()
        {
            try
            {
                // 1. Definir el periodo de reporte (mes actual)
                DateTime hoy = DateTime.Today;
                DateTime inicioDeMes = new DateTime(hoy.Year, hoy.Month, 1);
                // Calculamos el final del mes (el último milisegundo)
                DateTime finDeMes = inicioDeMes.AddMonths(1).AddSeconds(-1);


                // 2. Obtener todos los comercios activos (o al menos los que tienen configuracion)
                var comercios = await _context.Comercios.ToListAsync();

                // 3. Iterar sobre cada comercio para generar o actualizar el reporte
                foreach (var comercio in comercios)
                {
                    // --- OBTENER COMISIÓN ESPECÍFICA DEL COMERCIO ---
                    var configuracion = await _context.Configuraciones
                        .AsNoTracking() // No necesitamos seguimiento
                        .FirstOrDefaultAsync(c => c.IdComercio == comercio.IdComercio && c.Estado == true);

                    if (configuracion == null)
                    {
                        // Si el comercio no tiene una configuración ACTIVA, lo saltamos y continuamos con el siguiente
                        // En un caso de negocio, se podría registrar un evento en bitácora sobre este comercio
                        continue;
                    }

                    // El porcentaje de comisión está en la propiedad 'Comision' (es un INT de 0 a 100)
                    decimal factorComision = (decimal)configuracion.Comision / 100M;
                    // ------------------------------------------------


                    // a. Obtener todas las cajas del comercio para filtrar los SINPES
                    var telefonosCaja = await _context.Cajas
                        .Where(c => c.IdComercio == comercio.IdComercio && c.TelefonoSINPE != null)
                        .Select(c => c.TelefonoSINPE)
                        .ToListAsync();

                    // b. Calcular métricas de SINPE para el mes actual
                    var sinpesDelMes = await _context.Sinpes
                        .Where(p => telefonosCaja.Contains(p.TelefonoDestinatario) &&
                                    p.FechaDeRegistro >= inicioDeMes &&
                                    p.FechaDeRegistro <= finDeMes)
                        .ToListAsync();

                    // Calcular Monto Total Recaudado
                    decimal montoRecaudado = sinpesDelMes.Sum(p => p.Monto);

                    // Calcular Cantidad de SINPES
                    int cantidadSinpes = sinpesDelMes.Count;

                    // Calcular Monto Total Comisión (MontoTotalRecaudado * PorcentajeDeComision)
                    decimal montoComision = montoRecaudado * factorComision;

                    // c. Buscar si ya existe un reporte para este comercio y este mes
                    var reporteExistente = await _context.Set<Reporte>()
                        .FirstOrDefaultAsync(r => r.IdComercio == comercio.IdComercio &&
                                                  r.FechaDelReporte.Year == hoy.Year &&
                                                  r.FechaDelReporte.Month == hoy.Month);

                    if (reporteExistente != null)
                    {
                        // Si existe, ACTUALIZAR los datos
                        reporteExistente.CantidadDeCajas = telefonosCaja.Count;
                        reporteExistente.MontoTotalRecaudado = montoRecaudado;
                        reporteExistente.CantidadDeSINPES = cantidadSinpes;
                        reporteExistente.MontoTotalComision = montoComision;

                        _context.Set<Reporte>().Update(reporteExistente);
                    }
                    else if (cantidadSinpes > 0 || telefonosCaja.Count > 0)
                    {
                        // Si no existe y hay datos para reportar, CREAR un nuevo registro
                        var nuevoReporte = new Reporte
                        {
                            IdComercio = comercio.IdComercio,
                            CantidadDeCajas = telefonosCaja.Count,
                            MontoTotalRecaudado = montoRecaudado,
                            CantidadDeSINPES = cantidadSinpes,
                            MontoTotalComision = montoComision,
                            FechaDelReporte = inicioDeMes // Primer día del mes para el registro
                        };

                        _context.Set<Reporte>().Add(nuevoReporte);
                    }
                }

                // 4. Guardar todos los cambios a la base de datos
                await _context.SaveChangesAsync();

                TempData["Ok"] = $"Reportes generados y actualizados exitosamente para {comercios.Count} comercios.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Ocurrió un error inesperado al generar los reportes. Por favor, revise el log.";
                // Aquí se recomienda usar un logger para registrar 'ex'
                return RedirectToAction(nameof(Index));
            }
        }
    }
}