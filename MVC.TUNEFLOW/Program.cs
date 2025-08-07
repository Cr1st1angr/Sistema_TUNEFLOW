
using API.Consumer; // Para tu clase Crud que interact a con la API
using API.TUNEFLOW.Controllers; // Posiblemente se refiere a controladores de tu API interna o compartida
using Microsoft.AspNetCore.Identity; // Para la gesti n de usuarios y autenticaci n
using Microsoft.EntityFrameworkCore; // Para interactuar con la base de datos a trav s de Entity Framework Core
using Modelos.Tuneflow.Media;
using Modelos.Tuneflow.Models;
using Modelos.Tuneflow.Payments;
using Modelos.Tuneflow.Playlists;
using Modelos.Tuneflow.User.Administration;
using Modelos.Tuneflow.User.Consumer;
using Modelos.Tuneflow.User.Profiles;
using Modelos.Tuneflow.User.Production;
using MVC.TUNEFLOW.Data; // Tu contexto de base de datos para el proyecto MVC
using Npgsql;
using System.Data;
using System;
using Microsoft.AspNetCore.Identity.UI.Services;
using MVC.TUNEFLOW.Models;
using MVC.TUNEFLOW.Services; // Agregado para usar TimeSpan

namespace MVC.TUNEFLOW
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            Crud<Administrator>.EndPoint = "https://localhost:7031/api/Administrators";
            Crud<Album>.EndPoint = "https://localhost:7031/api/Albums";
            Crud<Song>.EndPoint = "https://localhost:7031/api/Songs";
            Crud<FavoriteSong>.EndPoint = "https://localhost:7031/api/FavoriteSongs";
            Crud<Artist>.EndPoint = "https://localhost:7031/api/Artists";
            Crud<Client>.EndPoint = "https://localhost:7031/api/Clients";
            Crud<ArtistStatistics>.EndPoint = "https://localhost:7031/api/ArtistsStatistics";
            Crud<SongPlaylist>.EndPoint = "https://localhost:7031/api/SongsPlaylists";
            Crud<Payment>.EndPoint = "https://localhost:7031/api/Payments";
            Crud<Profile>.EndPoint = "https://localhost:7031/api/Profiles";
            Crud<Playback>.EndPoint = "https://localhost:7031/api/Playbacks";
            Crud<Follow>.EndPoint = "https://localhost:7031/api/Follows";
            Crud<Subscription>.EndPoint = "https://localhost:7031/api/Subscriptions";
            Crud<SubscriptionType>.EndPoint = "https://localhost:7031/api/SubscriptionsTypes";
            Crud<Playlist>.EndPoint = "https://localhost:7031/api/Playlists";
            Crud<Country>.EndPoint = "https://localhost:7031/api/Countries";
            Crud<Genre>.EndPoint = "https://localhost:7031/api/Genres";
            Crud<ArtistVerificationRequest>.EndPoint = "https://localhost:7031/api/ArtistVerificationRequests";
            Crud<ADS>.EndPoint = "https://localhost:7031/api/ADS";


            builder.Services.AddTransient<IDbConnection>(sp =>
    new NpgsqlConnection(builder.Configuration.GetConnectionString("DefaultConnection")));


            var identityConnectionString = builder.Configuration.GetConnectionString("DefaultConnection");
            if (string.IsNullOrEmpty(identityConnectionString))
            {
                throw new InvalidOperationException("La cadena de conexi n 'DefaultConnection' para Identity no se encontr .");
            }

            builder.Services.AddDbContext<ApplicationDbContext>(options =>
            {
                options.UseNpgsql(identityConnectionString, npgsqlOptions =>
                {
                    npgsqlOptions.EnableRetryOnFailure(
                        maxRetryCount: 5,
                        maxRetryDelay: TimeSpan.FromSeconds(30),
                        errorCodesToAdd: null
                    );
                });
            });

            var tuneflowConnectionString = builder.Configuration.GetConnectionString("DefaultConnection");
            if (string.IsNullOrEmpty(tuneflowConnectionString))
            {
                throw new InvalidOperationException("La cadena de conexi n 'DefaultConnection' para TUNEFLOWContext no se encontr .");
            }

            builder.Services.AddDbContext<TUNEFLOWContext>(options =>
            {
                options.UseNpgsql(tuneflowConnectionString, npgsqlOptions =>
                {
                    npgsqlOptions.EnableRetryOnFailure(
                        maxRetryCount: 5,
                        maxRetryDelay: TimeSpan.FromSeconds(30),
                        errorCodesToAdd: null
                    );
                });
            });

            builder.Services.AddDefaultIdentity<IdentityUser>(options => options.SignIn.RequireConfirmedAccount = true)
                .AddRoles<IdentityRole>()
                .AddEntityFrameworkStores<ApplicationDbContext>();



            builder.Services.AddTransient<IEmailSender, EmailSender>();
            builder.Services.AddControllersWithViews();

            builder.Services.AddRazorPages();

            builder.Services.AddHttpClient();

            builder.Services.AddSession(options =>
            {
                options.IdleTimeout = TimeSpan.FromMinutes(30);
                options.Cookie.HttpOnly = true;
                options.Cookie.IsEssential = true;
            });

            var app = builder.Build();

            if (app.Environment.IsDevelopment())
            {
                app.UseMigrationsEndPoint();
            }
            else
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
                name: "areas",
                pattern: "{area:exists}/{controller=Panel}/{action=Panel}/{id?}");

            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}");

            app.MapRazorPages();

            app.Run();
        }
    }
}
