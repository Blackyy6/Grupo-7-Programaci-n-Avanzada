using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Proyecto_Grupo_7_Progra_Avanzada.Data;
using Proyecto_Grupo_7_Progra_Avanzada.Models;

namespace Proyecto_Grupo_7_Progra_Avanzada.Controllers.Api
{
    [ApiController]
    [Route("api/sinpe")]
    public class SinpeSyncApiController : ControllerBase
    {
        private readonly AppDbContext _context;

        public SinpeSyncApiController(AppDbContext context)
        {
            _context = context;
        }

        // ============================================================
        // 1) Consultar SINPE (10 pts)
        // GET: /api/sinpe/consultar?telefonoCaja=########
        // ============================================================
        [HttpGet("consultar")]
        public async Task<IActionResult> Consultar([FromQuery] string telefonoCaja)
        {
            telefonoCaja = (telefonoCaja ?? string.Empty).Trim();

            if (string.IsNullOrWhiteSpace(telefonoCaja))
                return BadRequest(new { Mensaje = "Debe proporcionar el teléfono de la caja (telefonoCaja)." });

            var validacion = await ValidarCajaYPermisoAsync(telefonoCaja);
            if (!validacion.EsValido)
                return StatusCode(validacion.CodigoHttp, new { Mensaje = validacion.Mensaje });

            // Obtener SINPES por teléfono destinatario (teléfono SINPE de la caja)
            var sinpes = await _context.Sinpes
                .AsNoTracking()
                .Where(s => s.TelefonoDestinatario == telefonoCaja)
                .OrderByDescending(s => s.FechaDeRegistro)
                .Select(s => new
                {
                    s.IdSinpe,
                    s.TelefonoOrigen,
                    s.NombreOrigen,
                    s.TelefonoDestinatario,
                    s.NombreDestinatario,
                    s.Monto,
                    s.Descripcion,
                    Fecha = s.FechaDeRegistro,
                    s.Estado
                })
                .ToListAsync();

            return Ok(sinpes);
        }

        // ============================================================
        // 2) Sincronizar SINPE (10 pts)
        // POST: /api/sinpe/sincronizar/{idSinpe}
        // ============================================================
        [HttpPost("sincronizar/{idSinpe:int}")]
        public async Task<IActionResult> Sincronizar([FromRoute] int idSinpe)
        {
            if (idSinpe <= 0)
                return BadRequest(new ApiResultado { EsValido = false, Mensaje = "El IdSinpe es inválido." });

            var sinpe = await _context.Sinpes.FirstOrDefaultAsync(s => s.IdSinpe == idSinpe);
            if (sinpe == null)
                return NotFound(new ApiResultado { EsValido = false, Mensaje = "El SINPE no existe." });

            // Validar permiso por la caja (teléfono destinatario)
            var validacion = await ValidarCajaYPermisoAsync(sinpe.TelefonoDestinatario);
            if (!validacion.EsValido)
                return StatusCode(validacion.CodigoHttp, new ApiResultado { EsValido = false, Mensaje = validacion.Mensaje });

            if (sinpe.Estado)
                return Ok(new ApiResultado { EsValido = true, Mensaje = "El SINPE ya estaba sincronizado." });

            sinpe.Estado = true;
            await _context.SaveChangesAsync();

            return Ok(new ApiResultado { EsValido = true, Mensaje = "SINPE sincronizado correctamente." });
        }

        // ============================================================
        // 3) Recibir SINPE (10 pts)
        // POST: /api/sinpe/recibir
        // Body: JSON con los datos del SINPE
        // ============================================================
        [HttpPost("recibir")]
        public async Task<IActionResult> Recibir([FromBody] RecibirSinpeRequest request)
        {
            if (request == null)
                return BadRequest(new ApiResultado { EsValido = false, Mensaje = "Debe enviar un cuerpo JSON válido." });

            // Validaciones mínimas (las de DataAnnotations se aplican automáticamente por [ApiController])
            if (!ModelState.IsValid)
                return BadRequest(new ApiResultado { EsValido = false, Mensaje = "Datos inválidos. Verifique los campos requeridos." });

            var telefonoCaja = (request.TelefonoDestinatario ?? string.Empty).Trim();
            var validacion = await ValidarCajaYPermisoAsync(telefonoCaja);
            if (!validacion.EsValido)
                return StatusCode(validacion.CodigoHttp, new ApiResultado { EsValido = false, Mensaje = validacion.Mensaje });

            try
            {
                var sinpe = new Sinpes
                {
                    TelefonoOrigen = request.TelefonoOrigen.Trim(),
                    NombreOrigen = request.NombreOrigen.Trim(),
                    TelefonoDestinatario = request.TelefonoDestinatario.Trim(),
                    NombreDestinatario = request.NombreDestinatario.Trim(),
                    Monto = request.Monto,
                    Descripcion = request.Descripcion,
                    FechaDeRegistro = DateTime.Now,
                    Estado = false
                };

                _context.Sinpes.Add(sinpe);
                await _context.SaveChangesAsync();

                return Ok(new ApiResultado { EsValido = true, Mensaje = "SINPE recibido y registrado correctamente." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResultado { EsValido = false, Mensaje = $"Error al registrar SINPE: {ex.Message}" });
            }
        }

        // ============================================================
        // Helpers
        // ============================================================
        private async Task<(bool EsValido, int CodigoHttp, string Mensaje)> ValidarCajaYPermisoAsync(string telefonoCaja)
        {
            telefonoCaja = (telefonoCaja ?? string.Empty).Trim();

            var caja = await _context.Cajas
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.TelefonoSINPE == telefonoCaja);

            if (caja == null)
                return (false, 404, "La caja consultada no existe.");

            // Buscar configuración del comercio
            var config = await _context.Configuraciones
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.IdComercio == caja.IdComercio);

            if (config == null)
                return (false, 403, "El comercio no tiene configuración para sincronización externa.");

            // Solo Externa (2) o Ambas (3)
            if (config.TipoConfiguracion != 2 && config.TipoConfiguracion != 3)
                return (false, 403, "Este comercio no está habilitado para sincronización externa (solo Externa o Ambas).");

            return (true, 200, "OK");
        }
    }

    public class ApiResultado
    {
        public bool EsValido { get; set; }
        public string Mensaje { get; set; } = string.Empty;
    }

    public class RecibirSinpeRequest
    {
        public string TelefonoOrigen { get; set; } = string.Empty;
        public string NombreOrigen { get; set; } = string.Empty;
        public string TelefonoDestinatario { get; set; } = string.Empty;
        public string NombreDestinatario { get; set; } = string.Empty;
        public decimal Monto { get; set; }
        public string? Descripcion { get; set; }
    }
}
