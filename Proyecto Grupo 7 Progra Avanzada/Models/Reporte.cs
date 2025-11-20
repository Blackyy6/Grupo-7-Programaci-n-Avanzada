using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Proyecto_Grupo_7_Progra_Avanzada.Models
{
    [Table("Reporte")]
    public class Reporte
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int IdReporte { get; set; }

        [Required]
        [Display(Name = "Comercio")]
        public int IdComercio { get; set; }

        [Required]
        [Display(Name = "Cantidad de Cajas")]
        public int CantidadDeCajas { get; set; }

        [Required]
        [Column(TypeName = "decimal(10,2)")]
        [Display(Name = "Monto Total Recaudado")]
        public decimal MontoTotalRecaudado { get; set; }

        [Required]
        [Display(Name = "Cantidad de SINPES")]
        public int CantidadDeSINPES { get; set; }

        [Required]
        [Column(TypeName = "decimal(10,2)")]
        [Display(Name = "Monto Total Comisión")]
        public decimal MontoTotalComision { get; set; }

        [Required]
        [Display(Name = "Fecha del Reporte")]
        public DateTime FechaDelReporte { get; set; }

        [ForeignKey("IdComercio")]
        public Comercio? Comercio { get; set; }
    }
}