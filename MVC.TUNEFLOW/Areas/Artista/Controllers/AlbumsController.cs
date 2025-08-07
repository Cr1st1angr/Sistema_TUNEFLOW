using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Security.Claims;
using API.Consumer;
using Dapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Modelos.Tuneflow.Media;
using Modelos.Tuneflow.Models;
using Modelos.Tuneflow.Playlists;
using Modelos.Tuneflow.User.Administration;
using Modelos.Tuneflow.User.Production;
using MVC.TUNEFLOW.Services;
using Newtonsoft.Json;
using Npgsql;

namespace MVC.TUNEFLOW.Areas.Artista.Controllers
{
    [Area("Artista")]
    [Authorize(Roles = "artista")]
    public class AlbumsController : Controller
    {
        private readonly IConfiguration _configuration;

        string supabaseUrl = "https://kblhmjrklznspeijwzeg.supabase.co";
        string supabaseAnonKey = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJpc3MiOiJzdXBhYmFzZSIsInJlZiI6ImtibGhtanJrbHpuc3BlaWp3emVnIiwicm9sZSI6ImFub24iLCJpYXQiOjE3NTA4MDk2MDcsImV4cCI6MjA2NjM4NTYwN30.CpoCYjAUi4ijZzAEqi9R_3HeGq5xpWANMMIlAQjJx-o";
        string bucket = "albumstuneflow";
        string directory = "ImagenesAlbums";
        string bucketImagen = "imagenesportadas";
        private readonly SupabaseStorageService _supaCancion;
        private readonly SupabaseStorageService _supaPortada;
        private readonly CancionService _cancionService = new CancionService();
   
        public AlbumsController(IConfiguration configuration)
        {
            _supaCancion = new SupabaseStorageService(supabaseUrl, supabaseAnonKey, bucket);
            _supaPortada = new SupabaseStorageService(supabaseUrl, supabaseAnonKey, bucketImagen, directory);
            _configuration = configuration;
        }
        
        // GET: AlbumsController
        public async Task<ActionResult> Index()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            Console.WriteLine($"UserId: {userId}");
            var artista = await Crud<Artist>.GetArtistaPorUsuarioId(userId);
            Console.WriteLine($"Artista: {artista?.StageName}");
          

            if (artista == null)
            {
                return NotFound("No se encontró el artista del usuario");
            }

            ViewBag.ArtistaId = artista.Id;
            Console.WriteLine("ArtistaId con viewbag: " + ViewBag.ArtistaId);
            var albums = await GetAlbumsPorArtistaId(artista.Id);
            return View(albums);
        }

        // GET: AlbumsController/Details/5
        public async Task<ActionResult> Details(int id)
        {
            var album = await Crud<Album>.GetByIdAsync(id);
            ViewBag.Canciones = await GetCancionesPorAlbum(id);
            return View(album);
        }


        // GET: AlbumsController/Create
        public async Task< ActionResult> Create(int artistaId)
        {
            var generos = await Crud<Genre>.GetAllAsync();
            var opciones = string.Join("", generos.Select(g => $"<option value='{g.Name}'>{g.Name}</option>"));
            ViewBag.GenerosOptions = opciones;
            ViewBag.ArtistaId = artistaId;
            return View();
        }

        // POST: AlbumsController/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create(Album album)
        {
            try
            {
                Crud<Album>.CreateAsync(album);
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                ViewBag.Generos = Crud<Genre>.GetAllAsync();
                // Log the exception (ex) if necessary
                ModelState.AddModelError("", "Unable to create album. Please try again.");
                return View(album);
            }
        }

        // GET: AlbumsController/Edit/5
        public async Task<ActionResult> Edit(int id)
        {
            
           
            var generos = await Crud<Genre>.GetAllAsync();
            ViewBag.GenerosOptions = generos.Select(g => new SelectListItem
            {
                Text = g.Name,
                Value = g.Name
            }).ToList();
            var album= await Crud<Album>.GetByIdAsync(id); 
            ViewBag.Canciones = await GetCancionesPorAlbum(id);
            ViewBag.Album = album; 
          
            return View(album);
        }

        // POST: AlbumsController/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit(int id, string Title, string Genre, string Description)
        {
            try
            {
                Console.WriteLine($"Editando álbum con ID: {id}, Título: {Title}, Género: {Genre}, Descripción: {Description}");
                var album = await Crud<Album>.GetByIdAsync(id);

                var albumNuevo = new Album
                {
                    Id = id,
                    Title = Title,
                    Genre = Genre,
                    Description = Description,
                    ArtistId = album.ArtistId,
                    CoverPath = album.CoverPath,
                    ReleaseDate = album.ReleaseDate,
                    CreationDate = album.CreationDate
                };
                Console.WriteLine($"Nuevo álbum: {JsonConvert.SerializeObject(albumNuevo)}");

                await Crud<Album>.UpdateAsync(id, albumNuevo);
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                var album = await Crud<Album>.GetByIdAsync(id);

                var generos = await Crud<Genre>.GetAllAsync();
                ViewBag.GenerosOptions = generos.Select(g => new SelectListItem
                {
                    Text = g.Name,
                    Value = g.Name
                }).ToList();

                ViewBag.Canciones = await GetCancionesPorAlbum(id);
                ViewBag.Album = album;

                ModelState.AddModelError("", "Unable to update album. Please try again.");
                return View(album);
            }
        }

        // GET: AlbumsController/Delete/5
        public ActionResult Delete(int id)
        {
            var album = Crud<Album>.GetByIdAsync(id);
            return View(album);
        }

        // POST: AlbumsController/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(int id, Album album)
        {
            try
            {
                Crud<Album>.DeleteAsync(id);
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "Unable to delete album. Please try again.");

                return View();
            }
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SubirAlbum(
        IFormFile archivoImagenAlbum,int artistaId,string tituloAlbum,string generoAlbum, string descripcionAlbum,
         List<IFormFile> archivosCanciones, List<string> titulosCanciones)      
        {
            Console.WriteLine("Artista id: " + artistaId);
            var generos = await Crud<Genre>.GetAllAsync();
            var opciones = string.Join("", generos.Select(g => $"<option value='{g.Name}'>{g.Name}</option>"));
            ViewBag.GenerosOptions = opciones;

            var artista = await Crud<Artist>.GetByIdAsync(artistaId);

            if (artista == null)
            {
                Console.WriteLine("Artista no encontrado");
                ModelState.AddModelError("", "Artista no encontrado.");
                return View("Create");
            }

            if (archivosCanciones == null || archivosCanciones.Count < 2)
            {
                Console.WriteLine("Debe subir al menos 2 canciones para un álbum.");
                ModelState.AddModelError("", "Debe subir al menos 2 canciones para un álbum.");
                return View("Create");
            }

            if (archivoImagenAlbum == null || archivoImagenAlbum.Length == 0)
            {
                Console.WriteLine("Debe subir una imagen para el álbum.");
                ModelState.AddModelError("", "Debe subir una imagen para el álbum.");
                return View("Create");
            }

            try
            {
                Console.WriteLine("entro al try de subir");

          
                var urlImagenAlbum = await _supaPortada.SubirArchivoAsync(archivoImagenAlbum);
                Console.WriteLine("obtuvo la url de la imagen");

          
                var nuevoAlbum = new Album
                {
                    Title = tituloAlbum,
                    ArtistId = artistaId,
                    Genre = generoAlbum,
                    CoverPath = urlImagenAlbum,
                    ReleaseDate = DateTime.UtcNow,
                    Description = descripcionAlbum ?? "",
                    CreationDate = DateTime.UtcNow,
                    Songs = new List<Song>()
                };
                var estatistica = await Crud<ArtistStatistics>.GetArtistStatisticsByArtist(artistaId);
                var numeroSongs = estatistica.PublishedSongs;
                int canciones = 0;

                var albumCreado = await Crud<Album>.CreateAsync(nuevoAlbum);

                for (int i = 0; i < archivosCanciones.Count; i++)
                {
                    var archivoCancion = archivosCanciones[i];

                    if (archivoCancion == null || archivoCancion.Length == 0)
                        continue;

                    var urlCancion = await _supaCancion.SubirCancionAsyncrona(archivoCancion, artista.StageName);
                    int duracion = _cancionService.ObtenerDuracionCancion(archivoCancion);

                    string tituloCancion = (titulosCanciones != null && titulosCanciones.Count > i && !string.IsNullOrWhiteSpace(titulosCanciones[i]))
                                          ? titulosCanciones[i]
                                          : $"Canción {i + 1}";

                    var nuevaCancion = new Song
                    {
                        Title = tituloCancion,
                        FilePath = urlCancion,
                        ArtistId = artistaId,
                        AlbumId = albumCreado.Id, 
                        Duration = duracion,
                        Genre = generoAlbum,
                        ImagePath = urlImagenAlbum,
                        ReleaseDate = DateTime.UtcNow,
                        Available = true,
                        ExplicitContent = false 
                    };

                    var cancionCreada = await Crud<Song>.CreateAsync(nuevaCancion);

                    canciones++;

                    nuevoAlbum.Songs.Add(nuevaCancion);
                }

               
                
                
                var numeroAlbums = estatistica.PublishedAlbums;
                estatistica.PublishedAlbums = numeroAlbums + 1;
                var numeroCanciones = estatistica.PublishedSongs;
                estatistica.PublishedSongs = numeroCanciones + canciones;
                await Crud<ArtistStatistics>.UpdateAsync(estatistica.Id, estatistica);

                if (albumCreado == null)
                {
                    ModelState.AddModelError("", "Error al crear el álbum con las canciones.");
                    return View("Create");
                }

                TempData["Success"] = "Álbum y canciones subidos correctamente.";
                return RedirectToAction("Index", "Albums", new { area = "Artista" });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al subir el álbum: {ex.Message}");
                ModelState.AddModelError("", $"Error al subir el álbum: {ex.Message}");
                return View("Create");
            }
        }

        public async Task<List<Album>> GetAlbumsPorArtistaId(int idArtista)
        {
            using var connection = new NpgsqlConnection(_configuration.GetConnectionString("DefaultConnection"));
            await connection.OpenAsync();

            var sql = @"SELECT * 
                FROM ""Albums"" 
                WHERE ""ArtistId"" = @ArtistId";

            var albums = await connection.QueryAsync<Album>(sql, new { ArtistId = idArtista });

            return albums.ToList();


        }

        public async Task<List<Song>> GetCancionesPorAlbum(int albumId)
        {
            using var connection = new NpgsqlConnection(_configuration.GetConnectionString("DefaultConnection"));
            await connection.OpenAsync();

            var sql = @"SELECT * 
                FROM ""Songs"" 
                WHERE ""AlbumId"" = @AlbumId";

            var canciones = await connection.QueryAsync<Song>(sql, new { AlbumId = albumId });

            return canciones.ToList();
        }


        [HttpPost]
        public async Task<IActionResult> EliminarAlbum(int id)
        {
            try
            {
                var album = await Crud<Album>.GetByIdAsync(id);
                var canciones = await GetCancionesPorAlbum(album.Id);

                int varNumeroCanciones = 0;
                foreach (var cancion in canciones)
                {
                    await Crud<Song>.DeleteAsync(cancion.Id);
                    varNumeroCanciones++;
                }
                var estadistica = await Crud<ArtistStatistics>.GetArtistStatisticsByArtist(album.ArtistId);
                var numeroAlbums = estadistica.PublishedAlbums;
                estadistica.PublishedAlbums = numeroAlbums - 1;
                estadistica.PublishedSongs = estadistica.PublishedSongs - varNumeroCanciones;
                await Crud<ArtistStatistics>.UpdateAsync(estadistica.Id, estadistica);
                await Crud<Album>.DeleteAsync(id);
                return RedirectToAction("Index"); 
            }
            catch (Exception ex)
            {
                ViewBag.Error = ex.Message;
                return View("Error");
            }
        }

    }
}
