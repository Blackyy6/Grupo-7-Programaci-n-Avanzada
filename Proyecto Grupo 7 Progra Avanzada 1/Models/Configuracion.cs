using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Proyecto_Grupo_7_Progra_Avanzada.Models
{
    [Table("Configuracion")]
    public class Configuracion

    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int IdConfiguracion { get; set; }

        [Required(ErrorMessage = "El comercio es obligatorio.")]
        [Display(Name = "Comercio")]
        public int IdComercio { get; set; }

        [Required(ErrorMessage = "El tipo de configuración es obligatorio.")]
        [Range(1, 3, ErrorMessage = "El tipo de configuración debe ser 1 (Plataforma), 2 (Externa) o 3 (Ambas).")]
        [Display(Name = "Tipo de Configuración")]
        public int TipoConfiguracion { get; set; }

        [Required(ErrorMessage = "La comisión es obligatoria.")]
        [Range(0, 100, ErrorMessage = "La comisión debe estar entre 0 y 100.")]
        [Display(Name = "Comisión (%)")]
        public int Comision { get; set; }

        [Required]
        [Display(Name = "Fecha de Registro")]
        public DateTime FechaDeRegistro { get; set; }

        [Display(Name = "Fecha de Modificación")]
        public DateTime? FechaDeModificacion { get; set; }

        [Display(Name = "Estado")]
        public bool Estado { get; set; } // true = Activo, false = Inactivo

        // Relación (opcional si existe la entidad Comercio)
        [ForeignKey("IdComercio")]
        public Comercio? Comercio { get; set; }

        // Método helper para obtener el texto del tipo de configuración
        public string GetTipoConfiguracionTexto()
        {
            return TipoConfiguracion switch
            {
                1 => "Plataforma",
                2 => "Externa",
                3 => "Ambas",
                _ => "No Definido"
            };
        }
    }
}
