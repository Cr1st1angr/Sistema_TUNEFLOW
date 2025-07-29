using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Modelos.Tuneflow.User.Production;
using Modelos.Tuneflow.User.Profiles;
using Modelos.Tuneflow.Models;
using API.Consumer;
using Modelos.Tuneflow.Media;
using System.Threading.Tasks;
using MVC.TUNEFLOW.Services;

namespace MVC.TUNEFLOW.Areas.Artista.Controllers
{
    [Area("Artista")]
    
    public class PerfilController : Controller
    {
        private readonly SupabaseStorageService _supabaseService;

        public PerfilController()
        {
            string supabaseUrl = "https://kblhmjrklznspeijwzeg.supabase.co";
            string supabaseAnonKey = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJpc3MiOiJzdXBhYmFzZSIsInJlZiI6ImtibGhtanJrbHpuc3BlaWp3emVnIiwicm9sZSI6ImFub24iLCJpYXQiOjE3NTA4MDk2MDcsImV4cCI6MjA2NjM4NTYwN30.CpoCYjAUi4ijZzAEqi9R_3HeGq5xpWANMMIlAQjJx-o";
            string bucket = "imagenesartistas";
            string directory = "ArtistasNuevos";



            _supabaseService = new SupabaseStorageService(supabaseUrl, supabaseAnonKey, bucket, directory);
        }

        [Authorize(Roles = "cliente,artista")]
        public async Task<ActionResult> Index(int id, int idCliente)
        {

            Console.WriteLine($"Al perfil ingresó el id: {id}"); 
            var artista = await Crud<Artist>.GetByIdAsync(id);
            var profile = await Crud<Profile>.GetPerfilPorArtistaId(artista.Id);
            ViewBag.IdCliente = idCliente; 
            var seguido = await Crud<Follow>.GetFollowByIdClient(idCliente, id);
            if(seguido != 0)
            {
                ViewBag.Seguido = true;
            }
            else
            {
                ViewBag.Seguido = false;
            }
            ViewBag.ArtistaId = artista.Id;
            ViewBag.StageName = artista.StageName; 


            Console.WriteLine($"Estado de Seguido: {ViewBag.Seguido}");
                return View(profile);
        }

        [Authorize(Roles = "artista")]
        public async Task<ActionResult> PerfilEditar()
        {
            string userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var artista = await Crud<Artist>.GetArtistaPorUsuarioId(userId);
            var profile = await Crud<Profile>.GetPerfilPorArtistaId(artista.Id);
            if (profile == null)
            {
                Console.WriteLine("No se encontró el perfil del artista.");
                return RedirectToAction("Panel", "Panel");
            }
            profile.Artist.Country = await Crud<Country>.GetByIdAsync(artista.CountryId);

            return View(profile);
        }

        // GET: PerfilController/Details/5
        public ActionResult Details(int id)
        {
            return View();
        }

        [HttpGet]
        [Authorize(Roles = "artista")]
        [Route("Artista/Perfil/ObtenerCancionesPorArtista")]
        public async Task<ActionResult> ObtenerCancionesPorArtista(int id)
        {
            var songs = await Crud<Song>.GetCancionesPorArtistaId(id);

            return Json(songs);
        }

        // GET: PerfilController/Create
        [Authorize(Roles = "artista")]
        public ActionResult Create()
        {
            return View();
        }

        // POST: PerfilController/Create
        [HttpPost]
        [Authorize(Roles = "artista")]
        [ValidateAntiForgeryToken]
        public ActionResult Create(IFormCollection collection)
        {
            try
            {
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }

        // GET: PerfilController/Edit/5
        public async Task<ActionResult> Edit(int id)
        {
            var profile = await Crud<Profile>.GetByIdAsync(id);
            return View(profile);
        }

        // POST: PerfilController/Edit/5
        [HttpPost]
        [Authorize(Roles = "artista")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit(int id, IFormFile ImageFile, string Biography)
        {
            try
            {
                var perfil = await Crud<Profile>.GetByIdAsync(id);
                if (ImageFile != null && ImageFile.Length > 0)
                {
                    var urlDefault = "https://kblhmjrklznspeijwzeg.supabase.co/storage/v1/object/public/imagenestuneflow/PerfilesDefecto/ImagenDefault.jpeg";
                    var urlEliminar = perfil.ProfileImage; 
                    if (!string.Equals(urlDefault, urlEliminar, StringComparison.OrdinalIgnoreCase))
                    {
                        var eliminado = await _supabaseService.EliminarArchivoAsync(urlEliminar);
                    }
                    var imageUrl = await _supabaseService.SubirArchivoAsync(ImageFile);
                    perfil.ProfileImage = imageUrl; 
                    perfil.Biography = Biography; 

                    await Crud<Profile>.UpdateAsync(perfil.Id, perfil);
                }
                return RedirectToAction(nameof(PerfilEditar));
            }
            catch
            {
                return View();
            }
        }

        // GET: PerfilController/Delete/5
        [Authorize(Roles = "artista")]
        public ActionResult Delete(int id)
        {
            return View();
        }

        // POST: PerfilController/Delete/5
        [HttpPost]
        [Authorize(Roles = "artista")]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(int id, IFormCollection collection)
        {
            try
            {
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }


        [HttpPost]

        [Authorize(Roles = "artista")]
        public async Task<IActionResult> SolicitarVerificacion()
        {
            string userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return Unauthorized(new { success = false, message = "No estás logueado" });

            var artista = await Crud<Artist>.GetArtistaPorUsuarioId(userId);
            if (artista == null)
                return NotFound(new { success = false, message = "Artista no encontrado" });

            if (artista.Verified)
                return Ok(new { success = false, message = "Ya estás verificado" });

            var peticiones = await Crud<ArtistVerificationRequest>.GetAllAsync();
            if (peticiones.Any(p => p.ArtistId == artista.Id))
                return Ok(new { success = false, message = "Ya enviaste una solicitud" });

            var nueva = new ArtistVerificationRequest
            {
                ArtistId = artista.Id,
                RequestDate = DateTime.Now
            };
            await Crud<ArtistVerificationRequest>.CreateAsync(nueva);

            return Ok(new { success = true, message = "Solicitud enviada correctamente" });
        }



    }
}
