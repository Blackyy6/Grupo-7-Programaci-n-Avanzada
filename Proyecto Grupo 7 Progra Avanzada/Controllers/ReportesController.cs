using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization; // NECESARIO
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Proyecto_Grupo_7_Progra_Avanzada.Data;
using Proyecto_Grupo_7_Progra_Avanzada.Models;

namespace Proyecto_Grupo_7_Progra_Avanzada.Controllers
{
    // Requisito: Autorización para Administrador y Contador
    [Authorize(Roles = "Administrador, Contador")]
    public class ReportesController : Controller
    {
        private readonly AppDbContext _context;
        // CORRECCIÓN: Ahora el servicio se llama BitacoraController
        private readonly BitacoraController _bitacora;

        // CORRECCIÓN: Inyectamos BitacoraController
        public ReportesController(AppDbContext context, BitacoraController bitacora)
        {
            _context = context;
            _bitacora = bitacora;
        }

        // ----------------------------------------------------------------------
        // 1. LISTAR REPORTES
        // ----------------------------------------------------------------------

        // GET: /Reportes/Index
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            // Nota: Se recomienda usar _context.Reportes en lugar de _context.Set<Reporte>()
            var reportes = await _context.Reportes
                .Include(r => r.Comercio)
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
            // NOTA: Para registrar el usuario que ejecuta esta acción en Bitácora, 
            // usaríamos el Id del usuario logueado (User.Identity.Name)

            try
            {
                // 1. Definir el periodo de reporte (mes actual)
                DateTime hoy = DateTime.Today;
                DateTime inicioDeMes = new DateTime(hoy.Year, hoy.Month, 1);
                DateTime finDeMes = inicioDeMes.AddMonths(1).AddSeconds(-1);


                // 2. Obtener todos los comercios activos
                var comercios = await _context.Comercios.ToListAsync();
                int reportesGenerados = 0;

                // 3. Iterar sobre cada comercio para generar o actualizar el reporte
                foreach (var comercio in comercios)
                {
                    // --- OBTENER COMISIÓN ESPECÍFICA DEL COMERCIO ---
                    var configuracion = await _context.Configuraciones
                        .AsNoTracking()
                        .FirstOrDefaultAsync(c => c.IdComercio == comercio.IdComercio && c.Estado == true);

                    if (configuracion == null)
                    {
                        continue;
                    }

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

                    decimal montoRecaudado = sinpesDelMes.Sum(p => p.Monto);
                    int cantidadSinpes = sinpesDelMes.Count;
                    decimal montoComision = montoRecaudado * factorComision;

                    // c. Buscar si ya existe un reporte para este comercio y este mes
                    var reporteExistente = await _context.Reportes
                        .FirstOrDefaultAsync(r => r.IdComercio == comercio.IdComercio &&
                                                     r.FechaDelReporte.Year == hoy.Year &&
                                                     r.FechaDelReporte.Month == hoy.Month);

                    if (reporteExistente != null)
                    {
                        // Datos anteriores para Bitácora
                        var datosAnteriores = new
                        {
                            reporteExistente.CantidadDeCajas,
                            reporteExistente.MontoTotalRecaudado,
                            reporteExistente.CantidadDeSINPES,
                            reporteExistente.MontoTotalComision
                        };

                        // Si existe, ACTUALIZAR los datos
                        reporteExistente.CantidadDeCajas = telefonosCaja.Count;
                        reporteExistente.MontoTotalRecaudado = montoRecaudado;
                        reporteExistente.CantidadDeSINPES = cantidadSinpes;
                        reporteExistente.MontoTotalComision = montoComision;

                        _context.Reportes.Update(reporteExistente);

                        await _context.SaveChangesAsync();
                        reportesGenerados++;


                        // ------------------------
                        //      BITÁCORA (ACTUALIZAR)
                        // ------------------------
                        await _bitacora.RegistrarEvento(
                            "Reportes",
                            "Actualizar",
                            $"Se actualizó el reporte del comercio {comercio.Nombre} para {hoy.ToString("MM/yyyy")}.",
                            datosAnteriores,
                            reporteExistente
                        );
                        //-------------------------------------------------------------------------------
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
                            FechaDelReporte = inicioDeMes
                        };

                        _context.Reportes.Add(nuevoReporte);

                        await _context.SaveChangesAsync();
                        reportesGenerados++;


                        // ------------------------
                        //      BITÁCORA (CREAR)
                        // ------------------------
                        await _bitacora.RegistrarEvento(
                            "Reportes",
                            "Registrar",
                            $"Se creó un nuevo reporte para el comercio {comercio.Nombre} para {hoy.ToString("MM/yyyy")}.",
                            null,
                            nuevoReporte
                        );
                        //---------------------------------------------------------------------------
                    }
                }

                TempData["Ok"] = $"Proceso completado. Se generaron o actualizaron {reportesGenerados} reportes.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                // ------------------------
                //      BITÁCORA (ERROR)
                // ------------------------
                await _bitacora.RegistrarEvento(
                    "Reportes",
                    "Error",
                    $"Error al generar reportes: {ex.Message}.",
                    null,
                    null,
                    ex
                );

                TempData["Error"] = "Ocurrió un error inesperado al generar los reportes. Por favor, revise la Bitácora.";
                return RedirectToAction(nameof(Index));
            }
        }
    }
}