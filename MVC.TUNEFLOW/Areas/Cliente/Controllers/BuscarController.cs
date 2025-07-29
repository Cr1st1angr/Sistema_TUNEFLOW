using API.Consumer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Modelos.Tuneflow.Media;
using System.Diagnostics;
using System.Security.Claims;
using Modelos.Tuneflow.User.Consumer;
using Modelos.Tuneflow.User.Production;
using Npgsql;
using Dapper;
using Modelos.Tuneflow.Playlists;



namespace MVC.TUNEFLOW.Areas.Cliente.Controllers
{
    [Area("Cliente")]
    [Authorize(Roles = "cliente")]
    public class BuscarController : Controller
    {
        private readonly IConfiguration _config;
        public BuscarController(IConfiguration config)
        {
            _config = config;
        }
        public async Task<IActionResult> Index()
            {
                string userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
               
            if (string.IsNullOrEmpty(userId))
            {
                
                return RedirectToAction("Login", "Account");
                }
            var client = await Crud<Modelos.Tuneflow.User.Consumer.Client>.GetClientePorUsuarioId(userId);
         
          
            if (client == null)
            {
                Console.WriteLine("p4");
                return RedirectToAction("Index", "Buscar");
                }
            
            ViewBag.IdCliente = client.Id;
            Console.WriteLine($"ViewBag: {ViewBag.IdClient}");
            
            return View(new List<Song>());
            }

        [AllowAnonymous]
        [HttpGet]
        public async Task<IActionResult> Buscar(string nameSong)
        {
            Console.WriteLine($"Buscar llamado con parámetro: '{nameSong}'");
            if (string.IsNullOrWhiteSpace(nameSong))
            {
                Console.WriteLine("Error: parámetro vacío");
                return View("Index", new List<Song>());
            }
          
            var songs = await Crud<Song>.GetCancionesPorgenero(nameSong);
            foreach(var song in songs)
            {
                song.Artist = await Crud<Artist>.GetByIdAsync(song.ArtistId);
            }
            Console.WriteLine($"Buscar llamado con parámetro: '{nameSong}'");
            Console.WriteLine($"Número de canciones recibidas en controlador: {songs?.Count ?? 0}");
            string userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(userId))
            {
                return RedirectToAction("Login", "Account");
            }
            var client = await Crud<Modelos.Tuneflow.User.Consumer.Client>.GetClientePorUsuarioId(userId);

            if (client == null)
            {
                return RedirectToAction("Index", "Buscar");
            }

            ViewBag.IdCliente = client.Id;
            Console.WriteLine($"ViewBag: {ViewBag.IdClient}");

            return View("Index", songs);
        }

        [AllowAnonymous]
        [HttpGet]
        public async Task<IActionResult> Search(string nameSong, int? idPlaylist)
        {
            Console.WriteLine($"Buscar llamado con parámetro: '{nameSong}', idPlaylist: {idPlaylist}");

            if (string.IsNullOrWhiteSpace(nameSong))
            {
                Console.WriteLine("Error: parámetro vacío");
                return View("Index", new List<Song>());
            }

            var songs = await Crud<Song>.GetCancionesPorPalabrasClave(nameSong);
            Console.WriteLine($"Número de canciones recibidas en controlador: {songs?.Count ?? 0}");

            string userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return RedirectToAction("Login", "Account");
            }

            var client = await Crud<Modelos.Tuneflow.User.Consumer.Client>.GetClientePorUsuarioId(userId);
            if (client == null)
            {
                return RedirectToAction("Index", "Buscar");
            }

            ViewBag.IdCliente = client.Id;
            ViewBag.IdPlaylist = idPlaylist;  

            return View("Index", songs);
        }
        private async Task<List<int>> GetSongIdsEnPlaylistAsync(int playlistId)
        {
            await using var connection = new NpgsqlConnection(_config.GetConnectionString("DefaultConnection"));
            await connection.OpenAsync();

            var sql = @"SELECT ""SongId"" 
                FROM ""SongsPlaylists"" 
                WHERE ""PlaylistId"" = @PlaylistId";

            var songIds = await connection.QueryAsync<int>(sql, new { PlaylistId = playlistId });

            return songIds.ToList();
        }



        [AllowAnonymous]
        [HttpGet]
        public async Task<IActionResult> Search2(string nameSong, int? idPlaylist)
        {
            Console.WriteLine("Id playlist: " + idPlaylist);
            ViewBag.IdPlaylist = idPlaylist;

            if (string.IsNullOrWhiteSpace(nameSong))
            {
                Console.WriteLine("Error: parámetro vacío");
                return View("Index2", new List<Song>());
            }

            var songs = await Crud<Song>.GetCancionesPorPalabrasClave(nameSong);
            Console.WriteLine($"Número de canciones recibidas en controlador: {songs?.Count ?? 0}");

            string userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return RedirectToAction("Login", "Account");
            }

            var client = await Crud<Modelos.Tuneflow.User.Consumer.Client>.GetClientePorUsuarioId(userId);
            if (client == null)
            {
                return RedirectToAction("Index2", "Buscar");
            }

            ViewBag.IdCliente = client.Id;

            List<int> songIdsEnPlaylist = new List<int>();
            if (idPlaylist.HasValue)
            {
                songIdsEnPlaylist = await GetSongIdsEnPlaylistAsync(idPlaylist.Value);

            }
            ViewBag.SongIdsEnPlaylist = songIdsEnPlaylist;

            return View("Index2", songs);
        }


    }

}
