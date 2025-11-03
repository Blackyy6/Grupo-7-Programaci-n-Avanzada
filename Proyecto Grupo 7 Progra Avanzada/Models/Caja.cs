using System.ComponentModel.DataAnnotations;

namespace Proyecto_Grupo_7_Progra_Avanzada.Models
{
    public class Caja
    {
        [Key]
        public int IdCaja { get; set; }

        [Required]
        public int IdComercio { get; set; }

        [Required]
        [StringLength(100)]
        public string Nombre { get; set; }

        [StringLength(255)]
        public string Descripcion { get; set; }

        [Required]
        [Phone]
        public string TelefonoSINPE { get; set; }

        public DateTime FechaDeRegistro { get; set; } = DateTime.Now;

        public DateTime? FechaDeModificacion { get; set; }

        public bool Estado { get; set; } = true;

        // Propiedad de Navegacion
        public Comercio Comercio { get; set; }

    }
}
