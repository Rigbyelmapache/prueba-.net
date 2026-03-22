using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using WebApp.Models.Enums;

namespace WebApp.Models
{
    public class Usuario
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(50)]
        public string Username { get; set; }

        [Required]
        [StringLength(100)]
        public string NombreCompleto { get; set; }

        [Required]
        [StringLength(100)]
        public string PrimerApellido { get; set; }

        [Required]
        [StringLength(100)]
        public string SegundoApellido { get; set; }

        // Identificación
        public TipoDocumento TipoDocumento { get; set; }
        
        [Required]
        [StringLength(50)]
        public string NumeroDocumento { get; set; }

        [DataType(DataType.Date)]
        public DateTime? FechaNacimiento { get; set; }

        [StringLength(50)]
        public string? Nacionalidad { get; set; }
        
        public Genero Genero { get; set; }

        // Datos Institucionales
        public EstadoUsuario Estado { get; set; } = EstadoUsuario.Activo;
        
        [StringLength(100)]
        public string? Cargo { get; set; }
        
        [StringLength(100)]
        public string? Sede { get; set; }
        
        public TipoContratacion TipoContratacion { get; set; }
        
        [DataType(DataType.Date)]
        public DateTime? FechaContratacion { get; set; }

        // Seguridad
        [Required]
        public string PasswordHash { get; set; }

        public string? Rol { get; set; } = "Usuario";

        public int IntentosFallidos { get; set; } = 0;

        public bool IsBlocked { get; set; } = false;

        public DateTime? BlockedUntil { get; set; }

        public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;

        // Relaciones a otras tablas
        public virtual ContactoUsuario Contacto { get; set; }
        public virtual ICollection<HistorialResponsabilidad> HistorialResponsabilidades { get; set; } = new List<HistorialResponsabilidad>();
    }
}
