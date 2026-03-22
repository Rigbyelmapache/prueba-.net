using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WebApp.Data;
using WebApp.Models;
using WebApp.Models.Enums;
using WebApp.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

//  Services
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IEmailService, EmailService>();

// jwt
var jwtSettings = builder.Configuration.GetSection("Jwt");
var key = Encoding.ASCII.GetBytes(jwtSettings["Key"]!);

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.RequireHttpsMetadata = false; // Do not require HTTPS locally
    options.SaveToken = true;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtSettings["Issuer"],
        ValidAudience = jwtSettings["Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(key)
    };

  
    options.Events = new JwtBearerEvents
    {
        OnMessageReceived = context =>
        {
            if (context.Request.Cookies.ContainsKey("JwtToken"))
            {
                context.Token = context.Request.Cookies["JwtToken"];
            }
            return Task.CompletedTask;
        },
        OnChallenge = context =>
        {
            // Redirigir a login 
            context.HandleResponse();
            context.Response.Redirect("/Auth/Login");
            return Task.CompletedTask;
        }
    };
});

var app = builder.Build();


if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();


app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");


using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    
    // asegurar bd 
    context.Database.Migrate();
    
    // crear admin
    if (!context.Usuarios.Any(u => u.Rol == "Admin"))
    {
        var adminUser = new Usuario
        {
            Username = "admin",
            NombreCompleto = "Administrador Principal",
            PrimerApellido = "Pérez",
            SegundoApellido = "Guzmán",
            NumeroDocumento = "12345678",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("Admin123!"),
            Rol = "Admin",
            Estado = EstadoUsuario.Activo,
            Cargo = "Administrador de Recursos",
            Sede = "011 Ministerio de Salud",
            Nacionalidad = "Peruana",
            TipoDocumento = TipoDocumento.DNI,
            Genero = Genero.Masculino,
            TipoContratacion = TipoContratacion.CAS,
            Contacto = new ContactoUsuario
            {
                CorreoPrincipal = "josue.2019.17@gmail.com",
                CorreoSecundario = "",
                TelefonoMovil = "999888777",
                TelefonoSecundario = "",
                Direccion = ""
            }
        };
        context.Usuarios.Add(adminUser);
    }
    
    // creacion de usuarios de prueba
    if (context.Usuarios.Count(u => u.Rol == "User") == 0)
    {
        var defaultPassword = BCrypt.Net.BCrypt.HashPassword("User123!");
        
        var listUsuarios = new List<Usuario>
        {
            new Usuario { Username = "juan.mendoza", NombreCompleto = "Juan Carlos", PrimerApellido = "Mendoza", SegundoApellido = "Ruiz", NumeroDocumento = "71234560", PasswordHash = defaultPassword, Rol = "User", Estado = EstadoUsuario.Inactivo, Cargo = "Analista de Datos", Sede = "011 Ministerio de Salud", Nacionalidad = "Peruana", TipoDocumento = TipoDocumento.DNI, Genero = Genero.Masculino, TipoContratacion = TipoContratacion.CAS, Contacto = new ContactoUsuario { CorreoPrincipal = "e2655455@gmail.com", CorreoSecundario = "", TelefonoMovil = "912345670",TelefonoSecundario = "", Direccion = "Av. Siempre Viva 123" } },
            new Usuario { Username = "maria.lopez", NombreCompleto = "María Fernanda", PrimerApellido = "López", SegundoApellido = "Díaz", NumeroDocumento = "71234561", PasswordHash = defaultPassword, Rol = "User", Estado = EstadoUsuario.Inactivo, Cargo = "Coordinadora de Proyectos", Sede = "012 Sede Norte", Nacionalidad = "Peruana", TipoDocumento = TipoDocumento.DNI, Genero = Genero.Femenino, TipoContratacion = TipoContratacion.Regimen728, Contacto = new ContactoUsuario { CorreoPrincipal = "e2655455@gmail.com", CorreoSecundario = "", TelefonoMovil = "912345671", TelefonoSecundario = "", Direccion = "Calle Las Flores 456" } },
            new Usuario { Username = "carlos.gomez", NombreCompleto = "Carlos Alberto", PrimerApellido = "Gómez", SegundoApellido = "Sánchez", NumeroDocumento = "71234562", PasswordHash = defaultPassword, Rol = "User", Estado = EstadoUsuario.Inactivo, Cargo = "Técnico Informático", Sede = "011 Ministerio de Salud", Nacionalidad = "Peruana", TipoDocumento = TipoDocumento.DNI, Genero = Genero.Masculino, TipoContratacion = TipoContratacion.LocacionServicios, Contacto = new ContactoUsuario { CorreoPrincipal = "e2655455@gmail.com", CorreoSecundario = "", TelefonoMovil = "912345672",TelefonoSecundario = "", Direccion = "Jr. Progreso 789" } },
            new Usuario { Username = "ana.vargas", NombreCompleto = "Ana Paula", PrimerApellido = "Vargas", SegundoApellido = "Flores", NumeroDocumento = "71234563", PasswordHash = defaultPassword, Rol = "User", Estado = EstadoUsuario.Inactivo, Cargo = "Asistente Social", Sede = "013 Sede Sur", Nacionalidad = "Peruana", TipoDocumento = TipoDocumento.DNI, Genero = Genero.Femenino, TipoContratacion = TipoContratacion.Ley30057, Contacto = new ContactoUsuario { CorreoPrincipal = "ana.vargas@preba.com", CorreoSecundario = "", TelefonoMovil = "912345673",TelefonoSecundario = "", Direccion = "Residencial San Marino 101" } },
            new Usuario { Username = "luis.torres", NombreCompleto = "Luis Enrique", PrimerApellido = "Torres", SegundoApellido = "Castro", NumeroDocumento = "71234564", PasswordHash = defaultPassword, Rol = "User", Estado = EstadoUsuario.Inactivo, Cargo = "Auditor Interno", Sede = "011 Ministerio de Salud", Nacionalidad = "Colombiana", TipoDocumento = TipoDocumento.CarnetExtranjeria, Genero = Genero.Masculino, TipoContratacion = TipoContratacion.CAS, Contacto = new ContactoUsuario { CorreoPrincipal = "luis.torres@preba.com", CorreoSecundario = "", TelefonoMovil = "912345674",TelefonoSecundario = "", Direccion = "Av. Central 555" } },
            new Usuario { Username = "sofia.rios", NombreCompleto = "Sofía", PrimerApellido = "Ríos", SegundoApellido = "García", NumeroDocumento = "71234565", PasswordHash = defaultPassword, Rol = "User", Estado = EstadoUsuario.Inactivo, Cargo = "Especialista Legal", Sede = "014 Sede Este", Nacionalidad = "Peruana", TipoDocumento = TipoDocumento.DNI, Genero = Genero.Femenino, TipoContratacion = TipoContratacion.Regimen276, Contacto = new ContactoUsuario { CorreoPrincipal = "sofia.rios@preba.com", CorreoSecundario = "", TelefonoMovil = "912345675",TelefonoSecundario = "", Direccion = "Urb. Las Quintas 22" } }
        };
        context.Usuarios.AddRange(listUsuarios);
    }
    
    
    var usuariosmigracion = context.Usuarios.Where(u => string.IsNullOrEmpty(u.Username)).ToList();
    foreach (var u in usuariosmigracion)
    {
        if (u.Rol == "Admin") u.Username = "admin";
        else u.Username = u.NombreCompleto.ToLower().Replace(" ", ".") + u.Id;
    }
    
    context.SaveChanges();
}

app.Run();
