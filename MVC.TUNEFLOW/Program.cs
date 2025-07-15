using API.Consumer; // Para tu clase Crud que interact�a con la API
using API.TUNEFLOW.Controllers; // Posiblemente se refiere a controladores de tu API interna o compartida
using Microsoft.AspNetCore.Identity; // Para la gesti�n de usuarios y autenticaci�n
using Microsoft.EntityFrameworkCore; // Para interactuar con la base de datos a trav�s de Entity Framework Core
using Modelos.Tuneflow.Media;
using Modelos.Tuneflow.Modelos;
using Modelos.Tuneflow.Pagos;
using Modelos.Tuneflow.Playlist;
using Modelos.Tuneflow.Usuario.Administracion;
using Modelos.Tuneflow.Usuario.Consumidor;
using Modelos.Tuneflow.Usuario.Perfiles;
using Modelos.Tuneflow.Usuario.Produccion;
using MVC.TUNEFLOW.Data; // Tu contexto de base de datos para el proyecto MVC
using Npgsql;
using System.Data;
using System; // Agregado para usar TimeSpan

namespace MVC.TUNEFLOW
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // --- Configuraci�n de EndPoints para el cliente API ---
            // Es crucial que estos EndPoints apunten a la URL correcta de tu API de backend.
            // Si tu API (API.TUNEFLOW) no se ejecuta en 'https://localhost:7031',
            // debes ajustar estos EndPoints a la URL real donde est� alojada.
            Crud<Administrador>.EndPoint = "https://localhost:7031/api/Administradores";
            Crud<Album>.EndPoint = "https://localhost:7031/api/Albums";
            Crud<Cancion>.EndPoint = "https://localhost:7031/api/Canciones";
            Crud<CancionFavorita>.EndPoint = "https://localhost:7031/api/CancionesFavoritas";
            Crud<Artista>.EndPoint = "https://localhost:7031/api/Artistas";
            Crud<Cliente>.EndPoint = "https://localhost:7031/api/Clientes"; // Aqu� ten�as una duplicaci�n, se mantiene una.
            Crud<EstadisticasArtista>.EndPoint = "https://localhost:7031/api/EstadisticasArtistas";
            Crud<MusicaPlaylist>.EndPoint = "https://localhost:7031/api/MusicasPlaylists";
            Crud<Pago>.EndPoint = "https://localhost:7031/api/Pagos";
            Crud<Perfil>.EndPoint = "https://localhost:7031/api/Perfiles";
            Crud<Reproduccion>.EndPoint = "https://localhost:7031/api/Reproducciones";
            Crud<Seguimiento>.EndPoint = "https://localhost:7031/api/Seguimientos";
            Crud<Suscripcion>.EndPoint = "https://localhost:7031/api/Suscripciones";
            Crud<TipoSuscripcion>.EndPoint = "https://localhost:7031/api/TiposSuscripciones";
            Crud<Playlist>.EndPoint = "https://localhost:7031/api/Playlists";
            Crud<Pais>.EndPoint = "https://localhost:7031/api/Paises";
            // Crud<Modelos.Tuneflow.Usuario.Consumidor.Cliente>.EndPoint = "https://localhost:7031/api/Clientes"; // Esta l�nea estaba duplicada y es redundante. Se elimin� o se consolid� con la anterior.

            builder.Services.AddTransient<IDbConnection>(sp =>
    new NpgsqlConnection(builder.Configuration.GetConnectionString("DefaultConnection")));


            // --- Configuraci�n de DbContext para la Autenticaci�n (Identity) ---
            // Usa ApplicationDbContext para gestionar los usuarios de Identity.
            // Es CRUCIAL que esta cadena de conexi�n apunte a la misma base de datos de Supabase.
            var identityConnectionString = builder.Configuration.GetConnectionString("DefaultConnection");
            if (string.IsNullOrEmpty(identityConnectionString))
            {
                throw new InvalidOperationException("La cadena de conexi�n 'DefaultConnection' para Identity no se encontr�.");
            }

            builder.Services.AddDbContext<ApplicationDbContext>(options =>
            {
                options.UseNpgsql(identityConnectionString, npgsqlOptions =>
                {
                    // Habilitar la estrategia de reintentos para Identity DB Context
                    npgsqlOptions.EnableRetryOnFailure(
                        maxRetryCount: 5,
                        maxRetryDelay: TimeSpan.FromSeconds(30),
                        errorCodesToAdd: null
                    );
                    // Opcional: npgsqlOptions.CommandTimeout(60);
                });
            });

            // --- Configuraci�n de DbContext para TUNEFLOW (modelos de tu aplicaci�n) ---
            // Usas un segundo DbContext llamado TUNEFLOWContext.
            // Si este contexto tambi�n se conecta a la base de datos de Supabase,
            // la estrategia de reintentos tambi�n es fundamental aqu�.
            // Aseg�rate de que "DefaultConnection" sea la cadena correcta si ambos contextos usan la misma DB.
            var tuneflowConnectionString = builder.Configuration.GetConnectionString("DefaultConnection"); // Asumo que es la misma que la de Identity.
            if (string.IsNullOrEmpty(tuneflowConnectionString))
            {
                throw new InvalidOperationException("La cadena de conexi�n 'DefaultConnection' para TUNEFLOWContext no se encontr�.");
            }

            builder.Services.AddDbContext<TUNEFLOWContext>(options =>
            {
                options.UseNpgsql(tuneflowConnectionString, npgsqlOptions =>
                {
                    // Habilitar la estrategia de reintentos para TUNEFLOW DB Context
                    npgsqlOptions.EnableRetryOnFailure(
                        maxRetryCount: 5,
                        maxRetryDelay: TimeSpan.FromSeconds(30),
                        errorCodesToAdd: null
                    );
                    // Opcional: npgsqlOptions.CommandTimeout(60);
                });
            });

            // --- Configuraci�n de ASP.NET Core Identity ---
            // A�ade los servicios de Identity, configurando el usuario predeterminado (IdentityUser)
            // y a�adiendo soporte para roles. Los datos de Identity se almacenar�n en ApplicationDbContext.
            builder.Services.AddDefaultIdentity<IdentityUser>(options => options.SignIn.RequireConfirmedAccount = true)
                .AddRoles<IdentityRole>() // Permite el uso de roles de usuario
                .AddEntityFrameworkStores<ApplicationDbContext>(); // Conecta Identity con tu DbContext

            // A�ade soporte para controladores MVC con vistas
            builder.Services.AddControllersWithViews();

            // A�ade soporte para Razor Pages (com�nmente usado con Identity UI)
            builder.Services.AddRazorPages();

            var app = builder.Build();

            // --- Configuraci�n del Pipeline de Solicitudes HTTP ---
            // Configurar el pipeline de solicitudes HTTP.
            if (app.Environment.IsDevelopment())
            {
                // En desarrollo, usa el endpoint de migraciones para aplicar cambios autom�ticamente (si est� configurado)
                app.UseMigrationsEndPoint();
            }
            else
            {
                // En producci�n, configura el manejo de errores global
                app.UseExceptionHandler("/Home/Error");
                // Configura HSTS para forzar conexiones HTTPS, recomendado para producci�n
                app.UseHsts();
            }

            // Forzar el uso de HTTPS
            app.UseHttpsRedirection();
            // Habilitar el servicio de archivos est�ticos (CSS, JS, im�genes, etc.)
            app.UseStaticFiles();

            // Habilitar el enrutamiento
            app.UseRouting();

            // Habilitar la autenticaci�n (middleware para procesar credenciales de usuario)
            app.UseAuthentication();
            // Habilitar la autorizaci�n (middleware para aplicar pol�ticas de acceso)
            app.UseAuthorization();

            // --- Mapeo de Rutas para Controladores y �reas ---
            // Mapeo de rutas para �reas (ej: /Admin/Panel)
            app.MapControllerRoute(
                name: "areas",
                pattern: "{area:exists}/{controller=Panel}/{action=Panel}/{id?}");

            // Mapeo de ruta predeterminada (ej: /Home/Index)
            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}");

            // Mapeo de Razor Pages (usado por Identity UI)
            app.MapRazorPages();

            // Iniciar la aplicaci�n
            app.Run();
        }
    }
}