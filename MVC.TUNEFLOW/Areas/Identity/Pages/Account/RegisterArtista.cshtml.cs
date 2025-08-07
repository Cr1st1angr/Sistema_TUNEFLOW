using System.ComponentModel.DataAnnotations;
using System.Text;
using System.Text.Encodings.Web;
using API.Consumer;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Logging;
using Modelos.Tuneflow.Models;
using Modelos.Tuneflow.User.Profiles;
using Modelos.Tuneflow.User.Production;
using NuGet.Protocol.Plugins;
using Modelos.Tuneflow.User.Administration;
using MVC.TUNEFLOW.Services;

namespace MVC.TUNEFLOW.Areas.Identity.Pages.Account
{
    public class RegisterArtistaModel : PageModel
    {
        private readonly SignInManager<IdentityUser> _signInManager;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly IUserStore<IdentityUser> _userStore;
        private readonly IUserEmailStore<IdentityUser> _emailStore;
        private readonly ILogger<RegisterArtistaModel> _logger;
        private readonly IEmailSender _emailSender;
        private readonly SupabaseStorageService _supa;
        string supabaseUrl = "https://kblhmjrklznspeijwzeg.supabase.co";
        string supabaseAnonKey = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJpc3MiOiJzdXBhYmFzZSIsInJlZiI6ImtibGhtanJrbHpuc3BlaWp3emVnIiwicm9sZSI6ImFub24iLCJpYXQiOjE3NTA4MDk2MDcsImV4cCI6MjA2NjM4NTYwN30.CpoCYjAUi4ijZzAEqi9R_3HeGq5xpWANMMIlAQjJx-o";
        string bucket = "cancionestuneflow";

        public RegisterArtistaModel(
            UserManager<IdentityUser> userManager,
            IUserStore<IdentityUser> userStore,
            SignInManager<IdentityUser> signInManager,
            ILogger<RegisterArtistaModel> logger,
            IEmailSender emailSender)
        {
            _userManager = userManager;
            _userStore = userStore;
            _emailStore = GetEmailStore();
            _signInManager = signInManager;
            _logger = logger;
            _emailSender = emailSender;
            _supa = new SupabaseStorageService(supabaseUrl, supabaseAnonKey, bucket);
        }

        [BindProperty]
        public InputModel Input { get; set; }
        public string ReturnUrl { get; set; }
        public IList<AuthenticationScheme> ExternalLogins { get; set; }
        public List<SelectListItem> Paises { get; set; }
        public List<SelectListItem> Generos { get; set; }

        public class InputModel
        {
            [Required, Display(Name = "Nombre")]
            public string FirstName { get; set; }

            [Required, Display(Name = "Apellido")]
            public string LastName { get; set; }

            [Required, EmailAddress, Display(Name = "Email")]
            public string Email { get; set; }

            [Required, Phone, Display(Name = "Teléfono")]
            public string Phone { get; set; }

            [Required, Display(Name = "País")]
            public int CountryId { get; set; }

            [DataType(DataType.Date), Display(Name = "Fecha de Nacimiento")]
            public DateTime BirthDate { get; set; }

            [Required, Display(Name = "Nombre Artístico")]
            public string StageName { get; set; }

            [Required, Display(Name = "Género Musical")]
            public string MusicGenre { get; set; }

            [Required, Display(Name = "Biografía")]
            public string Biography { get; set; }

            [Required, StringLength(100, MinimumLength = 6), DataType(DataType.Password)]
            [Display(Name = "Contraseña")]
            public string Password { get; set; }

            [DataType(DataType.Password)]
            [Display(Name = "Confirmar Contraseña")]
            [Compare("Password", ErrorMessage = "Las contraseñas no coinciden.")]
            public string ConfirmPassword { get; set; }
        }

        public async Task OnGetAsync(string returnUrl = null)
        {
            ReturnUrl = returnUrl;
            Paises = await GetPaisesAsync();
            Generos = await GetGenerosAsync();
            ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();
        }

        public async Task<IActionResult> OnPostAsync(string returnUrl = null)
        {
            returnUrl ??= Url.Content("~/");
            ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();

            if (ModelState.IsValid)
            {
                var user = CreateUser();
                await _userStore.SetUserNameAsync(user, Input.Email, CancellationToken.None);
                await _emailStore.SetEmailAsync(user, Input.Email, CancellationToken.None);
                var result = await _userManager.CreateAsync(user, Input.Password);

                if (result.Succeeded)
                {
                    _logger.LogInformation("Se creó cuenta para artista.");

                    var artist = new Modelos.Tuneflow.User.Production.Artist
                    {
                        FirstName = Input.FirstName,
                        LastName = Input.LastName,
                        Email = Input.Email,
                        Phone = Input.Phone,
                        BirthDate = Input.BirthDate.ToUniversalTime(),
                        CountryId = Input.CountryId,
                        StageName = Input.StageName,
                        MusicGenre = Input.MusicGenre,
                        AccountType = "Artista",
                        IsActive = true,
                        RegistrationDate = DateTime.UtcNow,
                        Verified = false, // por defecto
                        Password = Input.Password,
                        UserId = user.Id // Asignar el ID del usuario recién creado
                    };

                    var newArtist = await Crud<Modelos.Tuneflow.User.Production.Artist>.CreateAsync(artist);
                    await _supa.CrearCarpetaAsync(newArtist.StageName);
                    var profile = new Profile
                    {
                        ClientId = 0, // No aplica para artistas
                        ArtistId = newArtist.Id, // Asignar el ID del artista recién creado
                        ProfileImage = "https://kblhmjrklznspeijwzeg.supabase.co/storage/v1/object/public/imagenestuneflow/PerfilesDefecto/ImagenDefault.jpeg",
                        Biography = Input.Biography,
                        CreationDate = DateTime.UtcNow,
                    };

                    var newProfile = await Crud<Profile>.CreateAsync(profile);

                    var estadisticas = new ArtistStatistics
                    {
                        ArtistId = newArtist.Id,
                        TotalPlays = 0,
                        TotalFollowers = 0,
                        PublishedSongs = 0,
                        PublishedAlbums = 0
                    };

                    var newestadisticas = await Crud<ArtistStatistics>.CreateAsync(estadisticas);

                    await _userManager.AddToRoleAsync(user, "artista");

                    var userId = await _userManager.GetUserIdAsync(user);
                    var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                    code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));

                    var callbackUrl = Url.Page(
                        "/Account/ConfirmEmail",
                        null,
                        new { area = "Identity", userId = userId, code = code, returnUrl = returnUrl },
                        Request.Scheme);

                    var htmlMessage = $@"
        <div style='font-family: Arial, sans-serif; background-color: #f2f6ff; padding: 30px; border-radius: 10px; color: #333;'>
        <h2 style='color: #4a90e2;'>🎉 ¡Bienvenido a TuneFlow! 🎶</h2>
        <p>Hola 👋,</p>

        <p>Gracias por registrarte en <strong>TuneFlow</strong>, el lugar donde la música cobra vida.</p>

         <p>Antes de empezar a disfrutar del ritmo, por favor confirma tu cuenta haciendo clic en el siguiente botón:</p>

        <div style='text-align: center; margin: 30px 0;'>
        <a href='{HtmlEncoder.Default.Encode(callbackUrl)}' style='background-color: #4a90e2; color: white; padding: 12px 25px; text-decoration: none; border-radius: 6px; font-weight: bold;'>
            Confirmar mi cuenta
        </a>
        </div>

        <p>Si tú no creaste esta cuenta, simplemente ignora este mensaje.</p>

        <hr style='border: none; border-top: 1px solid #ddd; margin: 30px 0;'>
        <p style='font-size: 12px; color: #999;'>
        Este correo fue enviado automáticamente por TuneFlow. No respondas a este mensaje.
        </p>
        </div>";

                    await _emailSender.SendEmailAsync(Input.Email, "Confirma tu cuenta", htmlMessage);

                    if (_userManager.Options.SignIn.RequireConfirmedAccount)
                        return RedirectToPage("RegisterConfirmation", new { email = Input.Email, returnUrl });

                    await _signInManager.SignInAsync(user, isPersistent: false);
                    return LocalRedirect(returnUrl);
                }

                foreach (var error in result.Errors)
                    ModelState.AddModelError(string.Empty, error.Description);
            }

            Paises = await GetPaisesAsync(); // Para recargar el selector si falla
            Generos = await GetGenerosAsync(); // Para recargar el selector si falla
            return Page();
        }

        private IdentityUser CreateUser()
        {
            try
            {
                return Activator.CreateInstance<IdentityUser>();
            }
            catch
            {
                throw new InvalidOperationException($"No se puede crear '{nameof(IdentityUser)}'. Asegúrate de que tiene constructor vacío.");
            }
        }

        private async Task<List<SelectListItem>> GetPaisesAsync()
        {
            var countries = await Crud<Country>.GetAllAsync();

            return countries.Select(p => new SelectListItem
            {
                Value = p.Id.ToString(),
                Text = p.Name
            }).ToList();
        }

        private async Task<List<SelectListItem>> GetGenerosAsync()
        {
            var generos = await Crud<Genre>.GetAllAsync();

            return generos.Select(p => new SelectListItem
            {
                Value = p.Name,
                Text = p.Name
            }).ToList();
        }

        private IUserEmailStore<IdentityUser> GetEmailStore()
        {
            if (!_userManager.SupportsUserEmail)
                throw new NotSupportedException("Este UserManager no soporta email.");
            return (IUserEmailStore<IdentityUser>)_userStore;
        }
    }
}
