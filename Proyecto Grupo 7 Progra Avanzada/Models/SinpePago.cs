using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Proyecto_Grupo_7_Progra_Avanzada.Models
{
    [Table("SinpePagos")]
    public class SinpePago
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int IdSinpe { get; set; }

        [Required]
        [StringLength(10, MinimumLength = 8)]
        [RegularExpression(@"^\d{8,10}$", ErrorMessage = "El teléfono de origen debe tener 8-10 dígitos.")]
        [Display(Name = "Teléfono de origen")]
        public string TelefonoOrigen { get; set; }

        [Required]
        [StringLength(200)]
        [Display(Name = "Nombre de origen")]
        public string NombreOrigen { get; set; }

        [Required]
        [StringLength(10, MinimumLength = 8)]
        [RegularExpression(@"^\d{8,10}$", ErrorMessage = "El teléfono destinatario debe tener 8-10 dígitos.")]
        [Display(Name = "Teléfono del destinario")]
        public string TelefonoDestinatario { get; set; }

        [Required]
        [StringLength(200)]
        [Display(Name = "Nombre del destinario")]
        public string NombreDestinatario { get; set; }

        [Required]
        [Range(0.01, 9999999999999999.99)]
        [Column(TypeName = "decimal(18,2)")]
        public decimal Monto { get; set; }

        [Required]
        public DateTime FechaDeRegistro { get; set; } = DateTime.Now;

        [StringLength(50)]
        public string? Descripcion { get; set; }

        [Required]
        public bool Estado { get; set; } = false; // 0 = No sincronizado, 1 = Sincronizado
    }
}
