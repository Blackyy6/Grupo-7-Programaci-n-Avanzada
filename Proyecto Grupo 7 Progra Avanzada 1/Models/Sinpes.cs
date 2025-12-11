using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Proyecto_Grupo_7_Progra_Avanzada.Models
{
    [Table("Sinpes")]
    public class Sinpes
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int IdSinpe { get; set; }

        [Required(ErrorMessage = "El teléfono de origen es obligatorio.")]
        [StringLength(10, MinimumLength = 8)]
        [RegularExpression(@"^\d{8,10}$", ErrorMessage = "El teléfono de origen debe tener 8 a 10 dígitos.")]
        [Display(Name = "Teléfono de origen")]
        public string TelefonoOrigen { get; set; } // varchar (10), not null [cite: 110]

        [Required(ErrorMessage = "El nombre de origen es obligatorio.")]
        [StringLength(200)]
        [Display(Name = "Nombre de origen")]
        public string NombreOrigen { get; set; } // varchar (200), not null [cite: 110]

        [Required(ErrorMessage = "El teléfono destinatario es obligatorio.")]
        [StringLength(10, MinimumLength = 8)]
        [RegularExpression(@"^\d{8,10}$", ErrorMessage = "El teléfono destinatario debe tener 8 a 10 dígitos.")]
        [Display(Name = "Teléfono del destinatario")]
        public string TelefonoDestinatario { get; set; } // varchar (10), not null [cite: 111]

        [Required(ErrorMessage = "El nombre destinatario es obligatorio.")]
        [StringLength(200)]
        [Display(Name = "Nombre del destinatario")]
        public string NombreDestinatario { get; set; } // varchar (200), not null [cite: 111]

        [Required(ErrorMessage = "El monto es obligatorio.")]
        [Range(0.01, 9999999999999999.99, ErrorMessage = "El monto debe ser un valor positivo.")]
        [Column(TypeName = "decimal(18,2)")]
        public decimal Monto { get; set; } // Decimal (18,2), not null [cite: 111]

        [Required]
        public DateTime FechaDeRegistro { get; set; } = DateTime.Now; // Datetime, not null [cite: 112, 114]

        [StringLength(50)]
        [Display(Name = "Descripción")]
        public string? Descripcion { get; set; } // varchar(50), null [cite: 113]

        [Required]
        public bool Estado { get; set; } = false; // bit, not null, 0 = No sincronizado por defecto [cite: 115]
    }
}