using System;
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

        [Required(ErrorMessage = "La identificación es obligatoria.")]
        [StringLength(30, ErrorMessage = "La identificación no puede tener más de 30 caracteres.")]
        [Display(Name = "Identificación")]
        public string Identificacion { get; set; }

        [Required(ErrorMessage = "El tipo de identificación es obligatorio.")]
        [Range(1, 2, ErrorMessage = "El valor debe ser 1 (Física) o 2 (Jurídica).")]
        [Display(Name = "Tipo de Identificación")]
        public int TipoIdentificacion { get; set; } // 1 – Física, 2 – Jurídica

        [Required(ErrorMessage = "El nombre es obligatorio.")]
        [StringLength(200, ErrorMessage = "El nombre no puede tener más de 200 caracteres.")]
        public string Nombre { get; set; }

        [Required(ErrorMessage = "El tipo de comercio es obligatorio.")]
        [Range(1, 4, ErrorMessage = "El valor debe ser 1 (Restaurantes), 2 (Supermercados), 3 (Ferreterías) o 4 (Otros).")]
        [Display(Name = "Tipo de Comercio")]
        public int TipoDeComercio { get; set; } // 1 – Restaurantes, 2 - Supermercados, 3 – Ferreterías, 4 - Otros

        [Required(ErrorMessage = "El teléfono es obligatorio.")]
        [StringLength(20, ErrorMessage = "El teléfono no puede tener más de 20 caracteres.")]
        [Phone(ErrorMessage = "El formato del teléfono no es válido.")]
        [Display(Name = "Teléfono")]
        public string Telefono { get; set; }

        [Required(ErrorMessage = "El correo electrónico es obligatorio.")]
        [StringLength(200, ErrorMessage = "El correo no puede tener más de 200 caracteres.")]
        [EmailAddress(ErrorMessage = "El formato del correo no es válido.")]
        [Display(Name = "Correo Electrónico")]
        public string CorreoElectronico { get; set; }

        [Required(ErrorMessage = "La dirección es obligatoria.")]
        [StringLength(500, ErrorMessage = "La dirección no puede tener más de 500 caracteres.")]
        [Display(Name = "Dirección")]
        public string Direccion { get; set; }


        [Display(Name = "Fecha de Registro")]
        public DateTime FechaDeRegistro { get; set; }

        [Display(Name = "Fecha de Modificación")]
        public DateTime? FechaDeModificacion { get; set; } 


        [Display(Name = "Estado")]
        public bool Estado { get; set; } // 1 – Activo, 0 – Inactivo
    }
}