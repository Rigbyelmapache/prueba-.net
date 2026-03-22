using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace WebApp.Models
{
    public class Responsabilidad
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Nombre { get; set; }

        [StringLength(250)]
        public string Descripcion { get; set; }

        public virtual ICollection<HistorialResponsabilidad> HistorialUsuarios { get; set; } = new List<HistorialResponsabilidad>();
    }
}
