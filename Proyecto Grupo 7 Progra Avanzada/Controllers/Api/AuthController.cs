using System;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Proyecto_Grupo_7_Progra_Avanzada.Data;
using System.IdentityModel.Tokens.Jwt;

namespace Proyecto_Grupo_7_Progra_Avanzada.Controllers.Api
{
    [Route("api/[controller]")]
    [ApiController]
    // Permitir acceso anónimo, ya que es la puerta de entrada para obtener el token
    public class AuthController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IConfiguration _configuration;

        public AuthController(AppDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        // DTO para recibir la solicitud de Token (IdComercio por query string)
        public class TokenRequest
        {
            [FromQuery(Name = "idComercio")]
            public int IdComercio { get; set; }
        }

        /// <summary>
        /// Genera un JWT Token para un comercio si posee la configuración 'Externa' o 'Ambas'.
        /// Endpoint: GET /api/auth/token?idComercio=123
        /// </summary>
        [HttpGet("token")]
        public async Task<IActionResult> GetToken([FromQuery] TokenRequest request)
        {
            var comercioId = request.IdComercio;

            if (comercioId <= 0)
            {
                return Unauthorized(new { Message = "IdComercio es requerido y debe ser positivo." });
            }

            // Buscar la configuración activa para el comercio
            // Se usa Configuraciones (plural) según tu DbContext
            var configuracion = await _context.Configuraciones
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.IdComercio == comercioId && c.Estado == true);

            if (configuracion == null)
            {
                return Unauthorized(new { Message = "Comercio no encontrado o sin configuración activa." });
            }

            // 1. TipoConfiguracion: 2 (Externa) o 3 (Ambas)
            // Si es 1 (Plataforma) o diferente, retorna 401
            if (configuracion.TipoConfiguracion != 2 && configuracion.TipoConfiguracion != 3)
            {
                return Unauthorized(new { Message = "El comercio no tiene permisos de autenticación API (Configuración debe ser Externa o Ambas)." });
            }

            // 2. Generar el JWT Token
            var tokenString = GenerateJwtToken(comercioId.ToString());

            return Ok(new { Token = tokenString });
        }

        private string GenerateJwtToken(string idComercio)
        {
            var jwtSettings = _configuration.GetSection("JwtSettings");
            // Aseguramos que la clave secreta exista, usando el fallback si es necesario
            var key = Encoding.ASCII.GetBytes(jwtSettings["SecretKey"] ?? "LlaveSecretaJWT_Avanzada_2025_#P7zH");

            var tokenHandler = new JwtSecurityTokenHandler();
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
                {
                    // Usamos el IdComercio como identificador principal del token
                    new Claim(ClaimTypes.NameIdentifier, idComercio),
                    new Claim(ClaimTypes.Role, "ComercioAPI"), // Rol API para diferenciar
                }),
                Expires = DateTime.UtcNow.AddHours(2), // Token válido por 2 horas
                Issuer = jwtSettings["Issuer"],
                Audience = jwtSettings["Audience"],
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }
    }
}