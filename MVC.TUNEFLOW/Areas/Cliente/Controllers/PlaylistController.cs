using API.Consumer;
using Modelos.Tuneflow.Playlists;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Modelos;
using System.Text.RegularExpressions;
using Modelos.Tuneflow.User.Consumer;
using Modelos.Tuneflow.User.Profiles;
using Modelos.Tuneflow.Media;
using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using MVC.TUNEFLOW.Services;
using Modelos.Tuneflow.User;
using Microsoft.OpenApi.Writers;
using Npgsql;
using Dapper;

namespace MVC.TUNEFLOW.Areas.Cliente.Controllers
{
    [Area("Cliente")]
    [Authorize(Roles = "cliente")]
    public class PlaylistController : Controller
    {
        private readonly IConfiguration _config;
       
        private readonly SupabaseStorageService _supabaseService;

        public PlaylistController(IConfiguration config)
        {
            string supabaseUrl = "https://kblhmjrklznspeijwzeg.supabase.co";
            string supabaseAnonKey = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJpc3MiOiJzdXBhYmFzZSIsInJlZiI6ImtibGhtanJrbHpuc3BlaWp3emVnIiwicm9sZSI6ImFub24iLCJpYXQiOjE3NTA4MDk2MDcsImV4cCI6MjA2NjM4NTYwN30.CpoCYjAUi4ijZzAEqi9R_3HeGq5xpWANMMIlAQjJx-o";
            string bucket = "imagenesplaylistusuarios";
            string directory = "PortadasPlaylists";


            _config = config;
            _supabaseService = new SupabaseStorageService(supabaseUrl, supabaseAnonKey, bucket, directory);
        }
       

        public async Task<IActionResult> Index()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var client = await Crud<Modelos.Tuneflow.User.Consumer.Client>.GetClientePorUsuarioId(userId);
            ViewBag.IdCliente = client.Id;
            if (client == null) return RedirectToAction("Index", "Buscar");

            var playlists = await Crud<Playlist>.GetPlaylistPorClienteId(client.Id);
            return View(playlists);
        }

        public async Task<ActionResult> Canciones(int id)
        {
            Playlist playlist = await Crud<Playlist>.GetByIdAsync(id);
            playlist.Songs = await Crud<SongPlaylist>.GetCancionesPorPlaylist(id);
            Console.WriteLine($"Canciones {playlist.Songs.Count}");

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var client = await Crud<Modelos.Tuneflow.User.Consumer.Client>.GetClientePorUsuarioId(userId);

            ViewBag.IdPlaylist = playlist.Id;
            Console.WriteLine("el id de la playlist en el metodo/vista canciones es: " + ViewBag.IdPlaylist);
            ViewBag.IdCliente = client.Id;

            var cancionesEnPlaylist = await Crud<SongPlaylist>.GetCancionesPorPlaylist(playlist.Id);

            
            var listaCanciones = cancionesEnPlaylist.ToList();

            ViewBag.CancionesEnPlaylist = listaCanciones.Select(sp => sp.Id).ToList();

            return View(playlist);
        }


        public ActionResult Create() => View();

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create(string Title, string Description, IFormFile ImageCover)
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                var client = await Crud<Modelos.Tuneflow.User.Consumer.Client>.GetClientePorUsuarioId(userId);
                ViewBag.IdCliente = client.Id;
                string urlImagen = await _supabaseService.SubirArchivoAsync(ImageCover);
                if (string.IsNullOrEmpty(urlImagen))
                {
                    ModelState.AddModelError("", "Error al subir la imagen de portada.");
                    return View();
                }
                var playlist = new Playlist
                {
                    Title = Title,
                    Description = Description,
                    CreationDate = DateTime.UtcNow,
                    ClientId = client.Id,
                    PlaylistCover = urlImagen

                };
                await Crud<Playlist>.CreateAsync(playlist);
                return RedirectToAction("Index", "Playlist", new { area = "Cliente" });

            }
            catch (Exception e)
            {
                ModelState.AddModelError("", "Error al crear la playlist: " + e.Message);
                return View();
            }
        }

        public async Task<ActionResult> Edit(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var cliente = await Crud<Modelos.Tuneflow.User.Consumer.Client>.GetClientePorUsuarioId(userId);


            ViewBag.IdCliente = cliente.Id;
            var playlist = await Crud<Playlist>.GetByIdAsync(id);
            return View(playlist);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit(int id, string Title, string Description, IFormFile ImageCover)
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                var cliente = await Crud<Modelos.Tuneflow.User.Consumer.Client>.GetClientePorUsuarioId(userId);


                ViewBag.IdCliente = cliente.Id;
                var playlist = await Crud<Playlist>.GetByIdAsync(id);
                var urlEliminar = playlist.PlaylistCover;
                var eliminado = await _supabaseService.EliminarArchivoAsync(urlEliminar);
                playlist.PlaylistCover = await _supabaseService.SubirArchivoAsync(ImageCover);

                playlist.Title = Title;
                playlist.Description = Description;

                var actualizado = await Crud<Playlist>.UpdateAsync(playlist.Id,playlist);
                if (actualizado)
                {
                    return RedirectToAction(nameof(Index));
                }
                else
                {
                    ModelState.AddModelError("", "Error al actualizar la playlist.");
                    return View(playlist);
                }
            }
            catch (Exception e)
            {
                ModelState.AddModelError("", "Error al actualizar la playlist: " + e.Message);
                return View();
            }
        }

        [HttpPost]
        public async Task<ActionResult> Delete(int id)
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                var cliente = await Crud<Modelos.Tuneflow.User.Consumer.Client>.GetClientePorUsuarioId(userId);


                ViewBag.IdCliente = cliente.Id;
                var playlist = await Crud<Playlist>.GetByIdAsync(id);
                var eliminadoSupa = await _supabaseService.EliminarArchivoAsync(playlist.PlaylistCover);
                if (!eliminadoSupa)
                {
                    ModelState.AddModelError("", "Error al eliminar la imagen de portada.");
                    return View("Create", playlist);
                }
                List<Song> canciones = await Crud<SongPlaylist>.GetCancionesPorPlaylist(id);
                Console.WriteLine($"Canciones para eliminar: {canciones.Count}");

                foreach (var cancion in canciones)
                {
                    try
                    {
                        if (cancion == null)
                        {
                            Console.WriteLine("⚠️ Canción es NULL");
                            continue;
                        }

                        Console.WriteLine($"➡️ Procesando canción con ID: {cancion.Id}");

                        var idEliminar = await Crud<SongPlaylist>.ObtenerIdSongPlaylist(cancion.Id, playlist.Id);

                        Console.WriteLine($"✅ ID a eliminar en SongPlaylist: {idEliminar}");

                        if (idEliminar != 0)
                        {
                            await Crud<SongPlaylist>.DeleteAsync(idEliminar);
                            Console.WriteLine("🎵 Canción eliminada");
                        }
                        else
                        {
                            Console.WriteLine("⚠️ No se encontró relación en SongPlaylist");
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"❌ ERROR dentro del foreach: {ex.Message}");
                    }
                }
                var eliminado = await Crud<Playlist>.DeleteAsync(id);
                return RedirectToAction("Index", "Playlist", new { area = "Cliente" });
            }
            catch (Exception e)
            {
                ModelState.AddModelError("", "Error al eliminar la playlist: " + e.Message);
                return RedirectToAction("Index", "Playlist", new { area = "Cliente" });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddToPlaylist(int idCancion, int idPlaylist)
        {   Console.WriteLine("Entro a AddToPlaylist");
            Console.WriteLine("El id de la cancion es: " + idCancion);
            Console.WriteLine("El id de la playlist es: " + idPlaylist);

            try
            {
                string userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(userId))
                    return Unauthorized(new { status = "error", message = "Usuario no autenticado" });
              

                var cliente = await Crud < Modelos.Tuneflow.User.Consumer.Client>.GetClientePorUsuarioId(userId);
                ViewBag.IdCliente = cliente.Id;
             
                if (cliente == null)
                    return NotFound(new { status = "error", message = "Cliente no encontrado" });
                var playlist = await Crud<Playlist>.GetByIdAsync(idPlaylist);
                Console.WriteLine("El id de playlist es: " + idPlaylist);
                if (playlist == null || playlist.ClientId != cliente.Id)
                    return NotFound(new { status = "error", message = "Playlist no encontrada o no te pertenece" });

                
                var cancionesEnPlaylist = await Crud<SongPlaylist>.GetCancionesPorPlaylist(idPlaylist);
                Console.WriteLine("Va a verificar si la cancion existe en la playlist de Id"+ idPlaylist);
            
                var existe = cancionesEnPlaylist.Any(sp => sp.Id == idCancion);


                if (existe)
                {
                    Console.WriteLine("Sí encontró la canción con mismo id");

                    var ideliminar = await GetIdDeSongPlaylist(idCancion, idPlaylist);
                    Console.WriteLine("El id de la songplylist es: " + ideliminar);

                    if (ideliminar.HasValue)
                    {
                        int ideliminars = ideliminar.Value;
                        await Crud<SongPlaylist>.DeleteAsync(ideliminars);
                        Console.WriteLine("Eliminó la canción con id: " + ideliminars);
                        return Ok(new { status = "removed", message = "Canción eliminada de la playlist" });
                    }
                    else
                    {
                        return NotFound(new { message = "No se encontró la relación Song-Playlist." });
                    }
                }
                else
                {
                    var nuevaRelacion = new SongPlaylist
                    {
                        PlaylistId = idPlaylist,
                        SongId = idCancion
                    };
                    await Crud<SongPlaylist>.CreateAsync(nuevaRelacion);
                    return Ok(new { status = "added", message = "Canción agregada a la playlist" });
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error en AddToPlaylist: {ex.Message}");
                return StatusCode(500, new { status = "error", message = "Error interno del servidor" });
            }
        }

        private async Task<int?> GetIdDeSongPlaylist(int songId, int playlistId)
        {
            await using var connection = new NpgsqlConnection(_config.GetConnectionString("DefaultConnection"));
            await connection.OpenAsync();

            var sql = @"SELECT * 
                FROM ""SongsPlaylists"" 
                WHERE ""SongId"" = @SongId AND ""PlaylistId"" = @PlaylistId"; 

            var songPlaylist = await connection.QuerySingleAsync<SongPlaylist>(sql, new
            {
                SongId = songId,
                PlaylistId = playlistId
            });

            return songPlaylist?.Id;
        }



        [HttpPost]
        public async Task<ActionResult> DeleteSong(int playlistId, int songId)
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                var cliente = await Crud<Modelos.Tuneflow.User.Consumer.Client>.GetClientePorUsuarioId(userId);


                ViewBag.IdCliente = cliente.Id;
                var idEliminar = await Crud<SongPlaylist>.ObtenerIdSongPlaylist(songId, playlistId);
                if (idEliminar == 0)
                {
                    ModelState.AddModelError("", "La canción no está en la playlist.");
                    return RedirectToAction("Canciones", new { id = playlistId });
                }

                await Crud<SongPlaylist>.DeleteAsync(idEliminar);
                Console.WriteLine($"🎵 Canción {songId} eliminada de playlist {playlistId}");

                
                return RedirectToAction("Canciones", new { id = playlistId });
            }
            catch (Exception e)
            {
                Console.WriteLine($" Error al eliminar la canción: {e.Message}");
                return RedirectToAction("Canciones", new { id = playlistId });
            }
        }

    }
}
