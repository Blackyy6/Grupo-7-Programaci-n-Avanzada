using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Proyecto_Grupo_7_Progra_Avanzada.Data;
using Proyecto_Grupo_7_Progra_Avanzada.Models;
using Proyecto_Grupo_7_Progra_Avanzada.API.Models;

namespace Proyecto_Grupo_7_Progra_Avanzada.API.Controllers
{
    /// <summary>
    /// API para sincronizar SINPE Externo.
    /// Endpoints:
    ///  - GET api/SinpeApi/consultar?telefono=XXXXXXXX
    ///  - POST api/SinpeApi/sincronizar
    ///  - POST api/SinpeApi/recibir
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class SinpeApiController : ControllerBase
    {
        private readonly AppDbContext _context;

        public SinpeApiController(AppDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Valida que el comercio dueño de la caja (por teléfono SINPE)
        /// esté configurado como Externa (2) o Ambas (3) y activo.
        /// </summary>
        private async Task<(bool ok, string mensaje)> ValidarComercioExternoPorTelefonoAsync(string telefonoSinpe)
        {
            if (string.IsNullOrWhiteSpace(telefonoSinpe))
            {
                return (false, "El teléfono SINPE es obligatorio.");
            }

            var caja = await _context.Cajas
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.TelefonoSINPE == telefonoSinpe && c.Estado);

            if (caja == null)
            {
                return (false, "No existe una caja activa asociada al teléfono SINPE proporcionado.");
            }

            var configuracion = await _context.Configuraciones
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.IdComercio == caja.IdComercio && c.Estado);

            if (configuracion == null)
            {
                return (false, "El comercio no tiene una configuración activa.");
            }

            if (configuracion.TipoConfiguracion != 2 && configuracion.TipoConfiguracion != 3)
            {
                return (false, "El comercio no está configurado como Externa o Ambas para utilizar el SINPE Empresarial.");
            }

            return (true, string.Empty);
        }

        // ===============================
        // CONSULTAR SINPE (10 pts)
        // ===============================

        /// <summary>
        /// Retorna la lista de SINPE asociados a la caja cuyo teléfono SINPE se recibe.
        /// Solo aplica para comercios configurados como Externa o Ambas.
        /// GET: api/SinpeApi/consultar?telefono=88888888
        /// </summary>
        [HttpGet("consultar")]
        public async Task<ActionResult<IEnumerable<SinpeConsultaDto>>> ConsultarSinpe([FromQuery] string telefono)
        {
            if (string.IsNullOrWhiteSpace(telefono))
            {
                return BadRequest("Debe indicar el teléfono SINPE de la caja.");
            }

            var validacion = await ValidarComercioExternoPorTelefonoAsync(telefono);
            if (!validacion.ok)
            {
                return BadRequest(new OperacionResultadoDto
                {
                    EsValido = false,
                    Mensaje = validacion.mensaje
                });
            }

            var sinpes = await _context.Sinpes
                .AsNoTracking()
                .Where(s => s.TelefonoDestinatario == telefono)
                .OrderByDescending(s => s.FechaDeRegistro)
                .ToListAsync();

            var resultado = sinpes.Select(s => new SinpeConsultaDto
            {
                IdSinpe = s.IdSinpe,
                TelefonoOrigen = s.TelefonoOrigen,
                NombreOrigen = s.NombreOrigen,
                TelefonoDestinatario = s.TelefonoDestinatario,
                NombreDestinatario = s.NombreDestinatario,
                Monto = s.Monto,
                Descripcion = s.Descripcion,
                Fecha = s.FechaDeRegistro,
                Estado = s.Estado
            });

            return Ok(resultado);
        }

        // ===============================
        // SINCRONIZAR SINPE (10 pts)
        // ===============================

        /// <summary>
        /// Sincroniza un SINPE (marca Estado = true) utilizando el IdSinpe.
        /// Solo comercios Externa/Ambas pueden utilizar este método.
        /// POST: api/SinpeApi/sincronizar
        /// Body: { "IdSinpe": 1 }
        /// </summary>
        [HttpPost("sincronizar")]
        public async Task<ActionResult<OperacionResultadoDto>> SincronizarSinpe([FromBody] SinpeSincronizarRequest request)
        {
            if (request == null || request.IdSinpe <= 0)
            {
                return BadRequest(new OperacionResultadoDto
                {
                    EsValido = false,
                    Mensaje = "Debe indicar un IdSinpe válido."
                });
            }

            var sinpe = await _context.Sinpes.FirstOrDefaultAsync(s => s.IdSinpe == request.IdSinpe);

            if (sinpe == null)
            {
                return NotFound(new OperacionResultadoDto
                {
                    EsValido = false,
                    Mensaje = "No se encontró el SINPE indicado."
                });
            }

            var validacion = await ValidarComercioExternoPorTelefonoAsync(sinpe.TelefonoDestinatario);
            if (!validacion.ok)
            {
                return BadRequest(new OperacionResultadoDto
                {
                    EsValido = false,
                    Mensaje = validacion.mensaje
                });
            }

            if (sinpe.Estado)
            {
                return Ok(new OperacionResultadoDto
                {
                    EsValido = false,
                    Mensaje = "El SINPE ya se encuentra sincronizado."
                });
            }

            try
            {
                var datosAnteriores = new
                {
                    sinpe.IdSinpe,
                    sinpe.Estado,
                    sinpe.TelefonoDestinatario,
                    sinpe.Monto,
                    sinpe.FechaDeRegistro
                };

                // Actualizar estado a sincronizado
                sinpe.Estado = true;

                var datosPosteriores = new
                {
                    sinpe.IdSinpe,
                    sinpe.Estado,
                    sinpe.TelefonoDestinatario,
                    sinpe.Monto,
                    sinpe.FechaDeRegistro
                };

                _context.Sinpes.Update(sinpe);

                // Registrar en Bitácora (reutilizando el estándar del proyecto)
                var evento = new Bitacora
                {
                    TablaDeEvento = "Sinpes",
                    TipoDeEvento = "Editar",
                    FechaDeEvento = DateTime.Now,
                    DescripcionDeEvento = $"SINPE {sinpe.IdSinpe} sincronizado desde API para la caja {sinpe.TelefonoDestinatario}",
                    DatosAnteriores = System.Text.Json.JsonSerializer.Serialize(datosAnteriores),
                    DatosPosteriores = System.Text.Json.JsonSerializer.Serialize(datosPosteriores)
                };

                _context.Bitacora.Add(evento);
                await _context.SaveChangesAsync();

                return Ok(new OperacionResultadoDto
                {
                    EsValido = true,
                    Mensaje = "SINPE sincronizado correctamente."
                });
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

                return StatusCode(500, new OperacionResultadoDto
                {
                    EsValido = false,
                    Mensaje = "Ocurrió un error al sincronizar el SINPE."
                });
            }
        }

        // ===============================
        // RECIBIR SINPE (10 pts)
        // ===============================

        /// <summary>
        /// Recibe un SINPE desde una entidad financiera y lo registra en la tabla Sinpes.
        /// Reutiliza la lógica de Registrar SINPE del avance 1.
        /// POST: api/SinpeApi/recibir
        /// </summary>
        [HttpPost("recibir")]
        public async Task<ActionResult<OperacionResultadoDto>> RecibirSinpe([FromBody] SinpeRecibirRequest request)
        {
            if (request == null)
            {
                return BadRequest(new OperacionResultadoDto
                {
                    EsValido = false,
                    Mensaje = "Los datos del SINPE son obligatorios."
                });
            }

            if (!ModelState.IsValid)
            {
                var errores = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage);

                return BadRequest(new OperacionResultadoDto
                {
                    EsValido = false,
                    Mensaje = "Datos inválidos: " + string.Join(" | ", errores)
                });
            }

            var validacion = await ValidarComercioExternoPorTelefonoAsync(request.TelefonoDestinatario);
            if (!validacion.ok)
            {
                return BadRequest(new OperacionResultadoDto
                {
                    EsValido = false,
                    Mensaje = validacion.mensaje
                });
            }

            try
            {
                var caja = await _context.Cajas
                    .AsNoTracking()
                    .FirstOrDefaultAsync(c => c.TelefonoSINPE == request.TelefonoDestinatario && c.Estado);

                if (caja == null)
                {
                    return BadRequest(new OperacionResultadoDto
                    {
                        EsValido = false,
                        Mensaje = "No existe una caja activa asociada al teléfono SINPE destinatario."
                    });
                }

                var pago = new Sinpes
                {
                    TelefonoOrigen = request.TelefonoOrigen.Trim(),
                    NombreOrigen = request.NombreOrigen.Trim(),
                    TelefonoDestinatario = request.TelefonoDestinatario.Trim(),
                    NombreDestinatario = request.NombreDestinatario.Trim(),
                    Monto = request.Monto,
                    Descripcion = string.IsNullOrWhiteSpace(request.Descripcion) ? null : request.Descripcion.Trim(),
                    FechaDeRegistro = DateTime.Now,
                    Estado = false // No sincronizado por defecto
                };

                _context.Sinpes.Add(pago);
                await _context.SaveChangesAsync();

                // Registrar en Bitácora (mismo estándar que en Registrar)
                var bitacora = new Bitacora
                {
                    TablaDeEvento = "SINPE",
                    TipoDeEvento = "Registrar",
                    FechaDeEvento = DateTime.Now,
                    DescripcionDeEvento = $"Se registró un pago SINPE a la caja con teléfono {pago.TelefonoDestinatario} desde API.",
                    DatosAnteriores = null,
                    DatosPosteriores = System.Text.Json.JsonSerializer.Serialize(pago)
                };

                _context.Bitacora.Add(bitacora);
                await _context.SaveChangesAsync();

                return Ok(new OperacionResultadoDto
                {
                    EsValido = true,
                    Mensaje = "SINPE registrado correctamente."
                });
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

                return StatusCode(500, new OperacionResultadoDto
                {
                    EsValido = false,
                    Mensaje = "Ocurrió un error al registrar el SINPE."
                });
            }
        }
    }
}
