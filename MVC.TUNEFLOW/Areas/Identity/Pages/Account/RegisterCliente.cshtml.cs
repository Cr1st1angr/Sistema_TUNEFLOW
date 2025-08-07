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
using Modelos.Tuneflow.Playlists;
using Modelos.Tuneflow.User.Consumer;
using Modelos.Tuneflow.User.Profiles;

namespace MVC.TUNEFLOW.Areas.Identity.Pages.Account
{
    public class RegisterClienteModel : PageModel
    {
        private readonly SignInManager<IdentityUser> _signInManager;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly IUserStore<IdentityUser> _userStore;
        private readonly IUserEmailStore<IdentityUser> _emailStore;
        private readonly ILogger<RegisterClienteModel> _logger;
        private readonly IEmailSender _emailSender;

        public RegisterClienteModel(
            UserManager<IdentityUser> userManager,
            IUserStore<IdentityUser> userStore,
            SignInManager<IdentityUser> signInManager,
            ILogger<RegisterClienteModel> logger,
            IEmailSender emailSender)
        {
            _userManager = userManager;
            _userStore = userStore;
            _emailStore = GetEmailStore();
            _signInManager = signInManager;
            _logger = logger;
            _emailSender = emailSender;
        }

        [BindProperty]
        public InputModel Input { get; set; }

        public string ReturnUrl { get; set; }
        public IList<AuthenticationScheme> ExternalLogins { get; set; }
        public List<SelectListItem> Countries { get; set; }

        public class InputModel
        {
            [Required, Display(Name = "Nombre")]
            public string Nombre { get; set; }

            [Required, Display(Name = "Apellido")]
            public string Apellido { get; set; }

            [Required, EmailAddress, Display(Name = "Email")]
            public string Email { get; set; }

            [Required, Phone, Display(Name = "Teléfono")]
            public string Telefono { get; set; }

            [Required, Display(Name = "País")]
            public int PaisId { get; set; }

            [DataType(DataType.Date), Display(Name = "Fecha de nacimiento")]
            public DateTime FechaNacimiento { get; set; }

            [Required, StringLength(100, MinimumLength = 6), DataType(DataType.Password)]
            [Display(Name = "Contraseña")]
            public string Password { get; set; }

            [DataType(DataType.Password)]
            [Display(Name = "Confirmar contraseña")]
            [Compare("Password", ErrorMessage = "Las contraseñas no coinciden.")]
            public string ConfirmPassword { get; set; }
        }

        public async Task OnGetAsync(string returnUrl = null)
        {
            ReturnUrl = returnUrl;
            Countries = await GetPaisesAsync();
            ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();
        }

        public async Task<IActionResult> OnPostAsync(string returnUrl = null)
        {
            returnUrl ??= Url.Content("~/");
            ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();

            if (!ModelState.IsValid)
            {
                Countries = await GetPaisesAsync();
                return Page();
            }

            var user = CreateUser();
            await _userStore.SetUserNameAsync(user, Input.Email, CancellationToken.None);
            await _emailStore.SetEmailAsync(user, Input.Email, CancellationToken.None);

            var result = await _userManager.CreateAsync(user, Input.Password);
            if (!result.Succeeded)
            {
                foreach (var error in result.Errors)
                    ModelState.AddModelError(string.Empty, error.Description);

                Countries = await GetPaisesAsync();
                return Page();
            }

            _logger.LogInformation("Cliente registrado correctamente.");

            // Crear suscripción básica
            var subscription = new Subscription
            {
                StartDate = DateTime.UtcNow,
                SubscriptionTypeId = 1, // Básica por defecto
                NumberMembers = 0
            };

            var newSubscription = await Crud<Subscription>.CreateAsync(subscription);


            // Crear cliente
            var client = new Modelos.Tuneflow.User.Consumer.Client
            {
                FirstName = Input.Nombre,
                LastName = Input.Apellido,
                Email = Input.Email,
                Phone = Input.Telefono,
                CountryId = Input.PaisId,
                BirthDate = Input.FechaNacimiento.ToUniversalTime(),
                AccountType = "Cliente",
                IsActive = true,
                RegistrationDate = DateTime.UtcNow,
                SubscriptionId = newSubscription.Id,
                Password = Input.Password,
                UserId = user.Id // Asignar el ID del usuario recién creado
            };


            var newClient = await Crud<Modelos.Tuneflow.User.Consumer.Client>.CreateAsync(client);

            var profile = new Profile
            {
                ClientId = newClient.Id,
                ArtistId = 0, // Inicialmente no es un artista
                ProfileImage = "https://kblhmjrklznspeijwzeg.supabase.co/storage/v1/object/public/imagenestuneflow/PerfilesDefecto/ImagenDefault.jpeg",
                Biography = "Apasionado de la Música",
                CreationDate = DateTime.UtcNow,
            };

            var newProfile = await Crud<Profile>.CreateAsync(profile);

            var favoritePlaylists = new Playlist
            {
                Title = "Tus Me Gusta",
                Description = "Esta es una playlist que contiene tus canciones favoritas",
                CreationDate = DateTime.UtcNow,
                ClientId = newClient.Id,
                PlaylistCover = "https://kblhmjrklznspeijwzeg.supabase.co/storage/v1/object/public/imagenestuneflow/PerfilesDefecto/PortadaFavoritos.png"
            };

            var favoritePlaylist = await Crud<Playlist>.CreateAsync(favoritePlaylists);

            await _userManager.AddToRoleAsync(user, "cliente");

            var userId = await _userManager.GetUserIdAsync(user);
            var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
            code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));

            var callbackUrl = Url.Page(
                "/Account/ConfirmEmail",
                null,
                new { area = "Identity", userId = userId, code = code, returnUrl },
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

        private IdentityUser CreateUser()
        {
            try
            {
                return Activator.CreateInstance<IdentityUser>();
            }
            catch
            {
                throw new InvalidOperationException($"No se pudo crear '{nameof(IdentityUser)}'. Asegúrate de que tiene constructor vacío.");
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

        private IUserEmailStore<IdentityUser> GetEmailStore()
        {
            if (!_userManager.SupportsUserEmail)
                throw new NotSupportedException("El UserManager no soporta email.");
            return (IUserEmailStore<IdentityUser>)_userStore;
        }
    }
}