using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Configuration;
using MimeKit;
using System;
using System.Threading.Tasks;

namespace WebApp.Services
{
    public class EmailService : IEmailService
    {
        private readonly IConfiguration _configuration;

        public EmailService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task SendEmailAsync(string toEmail, string subject, string htmlMessage)
        {
            var emailSettings = _configuration.GetSection("EmailSettings");

            var message = new MimeMessage();
            message.From.Add(new MailboxAddress(emailSettings["SenderName"], emailSettings["SenderEmail"]));
            message.To.Add(new MailboxAddress("", toEmail));
            message.Subject = subject;

            var bodyBuilder = new BodyBuilder { HtmlBody = htmlMessage };
            message.Body = bodyBuilder.ToMessageBody();

            using var client = new SmtpClient();
            try
            {
                // Conectar al servidor SMTP de Gmail de manera segura (STARTTLS)
                await client.ConnectAsync(emailSettings["SmtpServer"], int.Parse(emailSettings["Port"]), SecureSocketOptions.StartTls);
                
                // Autenticar con la Contraseña de Aplicación de Google
                await client.AuthenticateAsync(emailSettings["SenderEmail"], emailSettings["Password"]);
                
                // Enviar correo
                await client.SendAsync(message);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al intentar enviar el correo electrónico: {ex.Message}");
                // No abortamos la ejecución de la app si el correo falla, solo lo registramos
            }
            finally
            {
                await client.DisconnectAsync(true);
            }
        }
    }
}
