using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Proyecto_Grupo_7_Progra_Avanzada.Models
{
    [Table("Comercio")]
    public class Comercio
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int IdComercio { get; set; }

        [Required]
        [StringLength(30)]
        public string Identificacion { get; set; }

        [Required]
        public int TipoIdentificacion { get; set; } // 1 – Física, 2 – Jurídica

        [Required]
        [StringLength(200)]
        public string Nombre { get; set; }

        [Required]
        public int TipoDeComercio { get; set; } // 1 – Restaurantes, 2 - Supermercados, 3 – Ferreterías, 4 - Otros

        [Required]
        [StringLength(20)]
        public string Telefono { get; set; }

        [Required]
        [StringLength(200)]
        [EmailAddress]
        public string CorreoElectronico { get; set; }

        [Required]
        [StringLength(500)]
        public string Direccion { get; set; }

        [Required]
        public DateTime FechaDeRegistro { get; set; }

        public DateTime? FechaDeModificacion { get; set; }

        [Required]
        public bool Estado { get; set; } // 1 – Activo, 0 – Inactivo
    }
}