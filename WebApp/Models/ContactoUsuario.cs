using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebApp.Models
{
    public class ContactoUsuario
    {
        [Key]
        public int Id { get; set; }

        public int UsuarioId { get; set; }
        
        [ForeignKey("UsuarioId")]
        public virtual Usuario Usuario { get; set; }

        [Required]
        [EmailAddress]
        [StringLength(150)]
        public string CorreoPrincipal { get; set; }

        [EmailAddress]
        [StringLength(150)]
        public string? CorreoSecundario { get; set; }

        [StringLength(20)]
        public string? TelefonoMovil { get; set; }

        [StringLength(20)]
        public string? TelefonoSecundario { get; set; }

        [StringLength(250)]
        public string? Direccion { get; set; }
    }
}
