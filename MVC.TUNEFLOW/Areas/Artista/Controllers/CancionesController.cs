using System.Security.Claims;
using System.Text.RegularExpressions;
using API.Consumer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Modelos.Tuneflow.Media;
using Modelos.Tuneflow.User.Production;
using MVC.TUNEFLOW.Services;
using Npgsql;
using Dapper;
using Microsoft.AspNetCore.Mvc.Rendering;
using Modelos.Tuneflow.Models;
using Modelos.Tuneflow.User.Administration;
using Modelos.Tuneflow.Playlists;

namespace MVC.TUNEFLOW.Areas.Artista.Controllers
{
    [Area("Artista")]
    [Authorize]
    public class CancionesController : Controller
    {
        string supabaseUrl = "https://kblhmjrklznspeijwzeg.supabase.co";
        string supabaseAnonKey = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJpc3MiOiJzdXBhYmFzZSIsInJlZiI6ImtibGhtanJrbHpuc3BlaWp3emVnIiwicm9sZSI6ImFub24iLCJpYXQiOjE3NTA4MDk2MDcsImV4cCI6MjA2NjM4NTYwN30.CpoCYjAUi4ijZzAEqi9R_3HeGq5xpWANMMIlAQjJx-o";
        string bucket = "cancionestuneflow";
        string bucketImagen = "imagenestuneflow";
        string directory = "ImagenesCanciones";
        private readonly SupabaseStorageService _supaCancion;
        private readonly SupabaseStorageService _supaImagen;
        private readonly CancionService _cancionService = new CancionService();
        
        public CancionesController()
        {
            _supaCancion = new SupabaseStorageService(supabaseUrl, supabaseAnonKey, bucket);
            _supaImagen = new SupabaseStorageService(supabaseUrl, supabaseAnonKey, bucketImagen, directory);
        }
        // GET: CancionesController
        public async Task<IActionResult> Index()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var artista = await Crud<Artist>.GetArtistaPorUsuarioId(userId);

            if (artista == null)
            {
                return NotFound("No se encontró el artista del usuario");
            }

            ViewBag.ArtistaId = artista.Id;

            var canciones = await Crud<Song>.GetCancionesPorArtistaId(artista.Id);

            foreach(var cancion in canciones)
            {
                if(cancion.AlbumId != null)
                {
                    int albumId = (int)cancion.AlbumId;
                    var album = await Crud<Album>.GetByIdAsync(albumId);
                    cancion.Album = album;
                }
            }
            ViewBag.ErrorMessage = null;
            return View(canciones);
        }


        // GET: CancionesController/Details/5
        public async Task<ActionResult> Details(int id)
        {
            var cancion = await Crud<Song>.GetByIdAsync(id);
            if(cancion.AlbumId != null)
            {
                int albumId = (int)cancion.AlbumId;
                var album = await Crud<Album>.GetByIdAsync(albumId);
                cancion.Album = album;
            }
            return View(cancion);
        }

        // GET: CancionesController/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: CancionesController/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(Song cancion)
        {
            try
            {
                Crud<Song>.CreateAsync(cancion);
                return RedirectToAction(nameof(Index));
            }
            catch(Exception ex)
            {
                // Log the exception (ex) if necessary
                ModelState.AddModelError("", "Unable to create song. Please try again.");
                return View(cancion);
        
            }
        }

        // GET: CancionesController/Edit/5
        public ActionResult Edit(int id)
        {
            var cancion = Crud<Song>.GetByIdAsync(id);
            return View(cancion);
        }

        // POST: CancionesController/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(int id, Song cancion)
        {
            try
            {
                Crud<Song>.UpdateAsync(id, cancion);
                return RedirectToAction(nameof(Index));
            }
            catch(Exception ex)
            {
                // Log the exception (ex) if necessary
                ModelState.AddModelError("", "Unable to edit song. Please try again.");
                return View(cancion);
            }
        }

        public async Task<ActionResult> Delete(int id)    
        {
            try
            {
                
                var song = await Crud<Song>.GetByIdAsync(id);
                var artista = await Crud<Artist>.GetByIdAsync(song.ArtistId);
                string url = song.FilePath;
                string urlImagen = song.ImagePath;
                string nombre = artista.StageName;

                await _supaCancion.EliminarCancionAsync(url);
                var eliminado = await _supaImagen.EliminarArchivoAsync(urlImagen);

                await Crud<Song>.DeleteAsync(id);

                var estadistica = await Crud<ArtistStatistics>.GetArtistStatisticsByArtist(song.ArtistId);
                var canciones = estadistica.PublishedSongs;

                estadistica.PublishedSongs = canciones - 1;
                await Crud<ArtistStatistics>.UpdateAsync(estadistica.Id, estadistica);
                ViewBag.ErrorMessage = null;
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                // Log the exception (ex) if necessary
                ModelState.AddModelError("", "Unable to delete song. Please try again.");
                ViewBag.ErrorMessage = "No se pudo eliminar la canción, pertenece a un Álbum";
                Console.WriteLine("No se puede eliminar la cancion");
                return RedirectToAction(nameof(Index));
            }
        }

        [HttpGet]
        public async Task<ActionResult> Subir(int artistaId)
        {
            ViewBag.ArtistaId = artistaId;
            ViewBag.Generos = await GetGenerosAsync();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SubirCancion(IFormFile archivoCancion, IFormFile archivoImagen, int artistaId, string Title,string Genre, bool ExplicitContent)
        {
            var artista = await Crud<Artist>.GetByIdAsync(artistaId);   

            if (artista == null)
            {   Console.WriteLine("Artista no encontrado");
                ModelState.AddModelError("", "Artista no encontrado.");
                var cancionError = new Song { ArtistId = artistaId };
                return View("Subir", cancionError);
            }

            if (archivoCancion == null || archivoCancion.Length == 0)
            {
                Console.WriteLine("No se ha seleccionado un archivo de audio válido.");
                ModelState.AddModelError("", "Debe seleccionar un archivo de audio válido.");
                var cancionError = new Song { ArtistId = artistaId };
                return View("Subir", cancionError);

            }

            try
            {
                // Subir archivo de audio
                var urlCancion = await _supaCancion.SubirCancionAsyncrona(archivoCancion, artista.StageName);
                var urlImagen = await _supaImagen.SubirArchivoAsync(archivoImagen);
                int segundos = _cancionService.ObtenerDuracionCancion(archivoCancion);

                var nuevaCancion = new Song
                {
                    Title = Title,
                    FilePath = urlCancion,
                    ArtistId = artistaId,
                    AlbumId = null,
                    Duration = segundos,
                    Genre = Genre,
                    ExplicitContent = ExplicitContent,
                    ImagePath = urlImagen,       
                    ReleaseDate = DateTime.UtcNow, 
                    Available = true
                };

                var song = await Crud<Song>.CreateAsync(nuevaCancion);

                if (song == null)
                {
                    return RedirectToAction("SubirCancion", new { artistaId = artistaId });
                }

                var estadistica = await Crud<ArtistStatistics>.GetArtistStatisticsByArtist(artistaId);
                var numeroCanciones = estadistica.PublishedSongs;
                estadistica.PublishedSongs = numeroCanciones + 1;
                await Crud<ArtistStatistics>.UpdateAsync(estadistica.Id, estadistica);
                Console.WriteLine("Canción subida correctamente");
                TempData["Success"] = "Canción subida correctamente.";
                return RedirectToAction("Index", "Canciones", new {area = "Artista"});
            }
            catch (Exception ex)
            { Console.WriteLine($"Error al subir la canción: {ex.Message}");
                ModelState.AddModelError("", $"Error al subir la canción: {ex.Message}");
                var cancionError = new Song { ArtistId = artistaId };
                ViewBag.Generos = await GetGenerosAsync();
                return View("Subir", cancionError);
            }
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





    }
}
