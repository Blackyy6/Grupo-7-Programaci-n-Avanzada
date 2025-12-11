using System;
using System.ComponentModel.DataAnnotations;

namespace Proyecto_Grupo_7_Progra_Avanzada.API.Models
{
    /// <summary>
    /// DTO devuelto por el endpoint Consultar SINPE.
    /// </summary>
    public class SinpeConsultaDto
    {
        public int IdSinpe { get; set; }
        public string TelefonoOrigen { get; set; } = string.Empty;
        public string NombreOrigen { get; set; } = string.Empty;
        public string TelefonoDestinatario { get; set; } = string.Empty;
        public string NombreDestinatario { get; set; } = string.Empty;
        public decimal Monto { get; set; }
        public string? Descripcion { get; set; }
        public DateTime Fecha { get; set; }
        public bool Estado { get; set; }
    }

    /// <summary>
    /// Request para sincronizar SINPE por IdSinpe.
    /// </summary>
    public class SinpeSincronizarRequest
    {
        [Required]
        public int IdSinpe { get; set; }
    }

    /// <summary>
    /// Request para registrar/recibir un SINPE desde la entidad financiera.
    /// </summary>
    public class SinpeRecibirRequest
    {
        [Required]
        [StringLength(10, MinimumLength = 8)]
        [RegularExpression(@"^\d{8,10}$", ErrorMessage = "El teléfono de origen debe tener 8 a 10 dígitos.")]
        public string TelefonoOrigen { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string NombreOrigen { get; set; } = string.Empty;

        [Required]
        [StringLength(10, MinimumLength = 8)]
        [RegularExpression(@"^\d{8,10}$", ErrorMessage = "El teléfono destinatario debe tener 8 a 10 dígitos.")]
        public string TelefonoDestinatario { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string NombreDestinatario { get; set; } = string.Empty;

        [Required]
        [Range(0.01, 9999999999999999.99)]
        public decimal Monto { get; set; }

        [StringLength(50)]
        public string? Descripcion { get; set; }
    }

    /// <summary>
    /// Modelo estándar de respuesta para las operaciones del API SINPE.
    /// </summary>
    public class OperacionResultadoDto
    {
        public bool EsValido { get; set; }
        public string Mensaje { get; set; } = string.Empty;
    }
}
