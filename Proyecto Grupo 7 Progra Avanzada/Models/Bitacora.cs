using System.ComponentModel.DataAnnotations;

namespace Proyecto_Grupo_7_Progra_Avanzada.Models
{
    public class Bitacora
    {
        [Key]
        public int IdEvento { get; set; }

        [Required]
        [MaxLength(50)]
        public string TablaDeEvento { get; set; } = string.Empty;

        [Required]
        [MaxLength(20)]
        public string TipoDeEvento { get; set; } = string.Empty;

        [Required]
        public DateTime FechaDeEvento { get; set; }

        [Required]
        public string DescripcionDeEvento { get; set; } = string.Empty;

        public string? StackTrace { get; set; }

        public string? DatosAnteriores { get; set; }

        public string? DatosPosteriores { get; set; }
    }
}

