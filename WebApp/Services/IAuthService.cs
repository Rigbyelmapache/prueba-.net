using System.Threading.Tasks;
using WebApp.Models.DTOs;

namespace WebApp.Services
{
    public interface IAuthService
    {
        Task<AuthResult> LoginAsync(LoginRequest request);
        Task LogoutAsync(int usuarioId);
    }
}
