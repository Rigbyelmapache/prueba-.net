using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using WebApp.Data;
using WebApp.Models;
using WebApp.Models.DTOs;
using WebApp.Models.Enums;

namespace WebApp.Services
{
    public class AuthService : IAuthService
    {
        private readonly ApplicationDbContext _context;
        private readonly IConfiguration _configuration;
        private readonly IEmailService _emailService;

        public AuthService(ApplicationDbContext context, IConfiguration configuration, IEmailService emailService)
        {
            _context = context;
            _configuration = configuration;
            _emailService = emailService;
        }

        public async Task<AuthResult> LoginAsync(LoginRequest request)
        {
            
            var usuario = await _context.Usuarios
                .Include(u => u.Contacto)
                .FirstOrDefaultAsync(u => u.Username == request.Username);

            if (usuario == null)
            {
                return new AuthResult { Status = AuthStatus.InvalidCredentials, Message = "Credenciales incorrectas." };
            }

            var contacto = usuario.Contacto; 

            // Revisar si la cuenta está bloqueada temporalmente
            if (usuario.IsBlocked && usuario.BlockedUntil.HasValue)
            {
                if (DateTime.UtcNow < usuario.BlockedUntil.Value)
                {
                    return new AuthResult { Status = AuthStatus.Blocked, Message = "Cuenta bloqueada temporalmente." };
                }
                else
                {
                    // expiró el bloqueo, restablecer a 0
                    usuario.IsBlocked = false;
                    usuario.BlockedUntil = null;
                    usuario.IntentosFallidos = 0;
                    await _context.SaveChangesAsync();
                }
            }

     
            bool isPasswordValid = BCrypt.Net.BCrypt.Verify(request.Password, usuario.PasswordHash);

            if (!isPasswordValid)
            {
                usuario.IntentosFallidos++;

                if (usuario.IntentosFallidos >= 5)
                {
                    usuario.IsBlocked = true;
                    usuario.BlockedUntil = DateTime.UtcNow.AddSeconds(65);
                    await _context.SaveChangesAsync();
                    
                    
                    if (!string.IsNullOrEmpty(contacto?.CorreoPrincipal))
                    {
                        var subject = "⚠️ Alerta de Seguridad: Cuenta Bloqueada";
                        var body = $@"
                            <h2 style='color:#d32f2f; font-family:sans-serif;'>Su cuenta ha sido bloqueada temporalmente</h2>
                            <p style='font-family:sans-serif;'>Estimado/a <strong>{usuario.NombreCompleto}</strong>,</p>
                            <p style='font-family:sans-serif;'>Hemos detectado múltiples intentos fallidos de inicio de sesión en su cuenta del sistema.</p>
                            <p style='font-family:sans-serif;'>Por motivos estrictos de seguridad, su acceso ha sido bloqueado preventivamente por <strong>15 minutos</strong>.</p>
                            <p style='font-family:sans-serif;'>Si usted no realizó o identificó estos intentos, por favor comuníquese operativamente con el administrador de la plataforma tan pronto recupere acceso.</p>
                            <br/>
                            <p style='font-family:sans-serif; color: #64748b;'>Atentamente,<br/><strong>Equipo de Soporte de Seguridad</strong></p>";
                            
                        
                        await _emailService.SendEmailAsync(contacto.CorreoPrincipal, subject, body);
                    }

                    return new AuthResult { Status = AuthStatus.Blocked, Message = "Demasiados intentos fallidos. Cuenta bloqueada por 15 minutos." };
                }

                await _context.SaveChangesAsync();
                return new AuthResult { Status = AuthStatus.InvalidCredentials, Message = $"Credenciales incorrectas. Le restan {5 - usuario.IntentosFallidos} intentos." };
            }

          
            usuario.IntentosFallidos = 0;
            usuario.IsBlocked = false;
            usuario.BlockedUntil = null;
            usuario.Estado = EstadoUsuario.Activo;
            await _context.SaveChangesAsync();

            // Generar el Payload JWT
            string token = GenerateJwtToken(usuario);

            return new AuthResult
            {
                Status = AuthStatus.Success,
                Token = token,
                Message = "Acceso concedido."
            };
        }

        public async Task LogoutAsync(int usuarioId)
        {
            var usuario = await _context.Usuarios.FindAsync(usuarioId);
            if (usuario != null)
            {
                usuario.Estado = EstadoUsuario.Inactivo;
                await _context.SaveChangesAsync();
            }
        }

        private string GenerateJwtToken(Usuario usuario)
        {
            var jwtSettings = _configuration.GetSection("Jwt");
            var keyStr = jwtSettings["Key"];
   
            if (string.IsNullOrEmpty(keyStr)) throw new ArgumentNullException("Jwt:Key is missing from appsettings.json");
            
            var key = Encoding.ASCII.GetBytes(keyStr);

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, usuario.Id.ToString()),
                new Claim(ClaimTypes.Name, usuario.NombreCompleto ?? ""),
                new Claim("PrimerApellido", usuario.PrimerApellido ?? ""),
                new Claim("SegundoApellido", usuario.SegundoApellido ?? ""),
               
                new Claim(ClaimTypes.Role, usuario.Rol ?? "Usuario"),
                new Claim("Documento", usuario.NumeroDocumento ?? "")
            };

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddHours(8), // El Token será válido por 8 horas
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature),
                Issuer = jwtSettings["Issuer"],
                Audience = jwtSettings["Audience"]
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            var token = tokenHandler.CreateToken(tokenDescriptor);

            return tokenHandler.WriteToken(token);
        }
    }
}
