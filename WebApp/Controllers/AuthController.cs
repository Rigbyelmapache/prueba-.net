using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Security.Claims;
using System.Threading.Tasks;
using WebApp.Models.DTOs;
using WebApp.Services;

namespace WebApp.Controllers
{
    public class AuthController : Controller
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        [HttpGet]
        public IActionResult Login()
        {
            // Previene que se logee si ya está autenticado
            if (User.Identity != null && User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Panel", "Admin");
            }
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> LoginPost([FromForm] LoginRequest request)
        {
            if (!ModelState.IsValid)
            {
                TempData["ErrorMessage"] = "Por favor ingrese todos los datos.";
                return RedirectToAction("Login");
            }

            var result = await _authService.LoginAsync(request);

            switch (result.Status)
            {
                case AuthStatus.Success:
                    // Guardar JWT en cookie HttpOnly por seguridad
                    var cookieOptions = new CookieOptions
                    {
                        HttpOnly = true,
                        Secure = false, // En un entorno local puede ser false sin SSL
                        SameSite = SameSiteMode.Strict,
                        Expires = DateTime.UtcNow.AddHours(8)
                    };
                    Response.Cookies.Append("JwtToken", result.Token, cookieOptions);

                    return RedirectToAction("Panel", "Admin");

                case AuthStatus.Blocked:
                    // Redirigir a vista estática de bloqueo
                    return RedirectToAction("Block");

                case AuthStatus.InvalidCredentials:
                default:
                    // Renderiza error y lo envía a la vista
                    TempData["ErrorMessage"] = result.Message;
                    return RedirectToAction("Login");
            }
        }

        [HttpGet]
        public IActionResult Block()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Logout()
        {
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!string.IsNullOrEmpty(userIdStr) && int.TryParse(userIdStr, out int userId))
            {
                await _authService.LogoutAsync(userId);
            }

            Response.Cookies.Delete("JwtToken");
            return RedirectToAction("Login");
        }
    }
}
