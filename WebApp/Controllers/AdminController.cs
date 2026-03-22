using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;
using WebApp.Data;

namespace WebApp.Controllers
{
    [Authorize]
    public class AdminController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AdminController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public IActionResult Panel()
        {
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> ListaUsuarios()
        {
            // Optimizamos la consulta con AsNoTracking para acelerar lectura
            var usuarios = await _context.Usuarios
                .AsNoTracking()
                .ToListAsync();

            return View(usuarios);
        }

        [HttpGet]
        public async Task<IActionResult> ViewUsuario(int id)
        {
            // Optimizamos consulta trayendo solo las entidades dependientes necesarias
            var usuario = await _context.Usuarios
                .Include(u => u.Contacto)
                .Include(u => u.HistorialResponsabilidades)
                    .ThenInclude(h => h.Responsabilidad)
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.Id == id);

            if (usuario == null)
            {
                return NotFound();
            }

            return View(usuario);
        }
    }
}
