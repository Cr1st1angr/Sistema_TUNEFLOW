using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Modelos.Tuneflow.Media;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text;
using API.Consumer;
using System.Security.Claims;
using Modelos.Tuneflow.User;
using Modelos.Tuneflow.Models;
using Modelos.Tuneflow.User.Consumer;
using Modelos.Tuneflow.Playlists;
using Modelos.Tuneflow.User.Administration;

namespace MVC.TUNEFLOW.Areas.Cliente.Controllers
{
    [Area("Cliente")]
    [Authorize(Roles = "cliente")]
    public class ReproductorController : Controller
    {
        private readonly HttpClient _httpClient;
        private const string SessionKeyListaReproduccion = "ListaReproduccion";
        private const string SessionKeyIndiceActual = "IndiceActual";

        public ReproductorController(IHttpClientFactory httpClientFactory)
        {
            _httpClient = httpClientFactory.CreateClient();
            _httpClient.BaseAddress = new Uri("https://localhost:7031/api/");
        }

        // Guarda el índice actual de la canción en la sesión.
        private void GuardarIndiceActual(int index)
        {
            HttpContext.Session.SetInt32(SessionKeyIndiceActual, index);
        }

        // Guarda la lista de canciones en la sesión como una cadena JSON.
        private void GuardarListaEnSesion(List<Song> canciones)
        {
            var cancionesJson = JsonSerializer.Serialize(canciones);
            HttpContext.Session.SetString(SessionKeyListaReproduccion, cancionesJson);
        }

        // Obtiene la lista de canciones almacenada en la sesión, deserializándola desde JSON.
        private List<Song>? ObtenerListaDeSesion()
        {
            var cancionesJson = HttpContext.Session.GetString(SessionKeyListaReproduccion);
            return cancionesJson == null ? null : JsonSerializer.Deserialize<List<Song>>(cancionesJson);
        }

        // Agrega una canción a la lista de sesión si no está ya presente.
        private void AgregarCancionASesion(Song cancion)
        {
            var lista = ObtenerListaDeSesion() ?? new List<Song>();
            if (!lista.Any(c => c.Id == cancion.Id))
            {
                lista.Add(cancion);
                GuardarListaEnSesion(lista);
            }
        }

        // Obtiene los datos de una canción específica por su ID y los devuelve en una vista parcial.
        [HttpGet]
        public async Task<IActionResult> GetCancionData(int id)
        {
            try
            {
                var cancion = await _httpClient.GetFromJsonAsync<Song>($"songs/{id}");
                if (cancion == null)
                    return NotFound();
                return PartialView("_Reproductor", cancion);
            }
            catch (Exception ex)
            {
                return Content($"Error al obtener la canción: {ex.Message}");
            }
        }

        // Reproduce una canción específica por su ID, la agrega a la lista de sesión y muestra el reproductor.
        [HttpGet]
        public async Task<IActionResult> Reproducir(int id)
        {
            try
            {
                var cancion = await _httpClient.GetFromJsonAsync<Song>($"songs/{id}");
                if (cancion == null)
                    return NotFound();
                AgregarCancionASesion(cancion);
                return PartialView("_Reproductor", cancion);
            }
            catch (Exception ex)
            {
                return Content($"Error al obtener la canción: {ex.Message}");
            }
        }

        [HttpGet]
        public async Task<IActionResult> ReproducirPlaylist(int playlistId)
        {
            var canciones = await _httpClient.GetFromJsonAsync<List<Song>>($"SongsPlaylists/SongsForPlaylist/{playlistId}");
            if (canciones == null || !canciones.Any())
                return NotFound("No hay canciones en esta playlist.");

            GuardarListaEnSesion(canciones);

            // Imprimir en consola cada canción de la playlist
            Console.WriteLine($"Playlist {playlistId} contiene:");
            foreach (var song in canciones)
            {
                Console.WriteLine($"- {song.Title} por {song.Artist?.StageName ?? "Desconocido"}");
            }

            return Ok("Playlist cargada correctamente en sesión.");
        }

        [HttpGet]
        public async Task<IActionResult> ReproducirAlbum(int albumId)
        {
            var canciones = await _httpClient.GetFromJsonAsync<List<Song>>($"Albums/{albumId}/songs");
            if (canciones == null || !canciones.Any())
                return NotFound("No hay canciones en este álbum.");

            GuardarListaEnSesion(canciones);

            // Imprimir en consola cada canción del álbum
            Console.WriteLine($"Álbum {albumId} contiene:");
            foreach (var song in canciones)
            {
                Console.WriteLine($"- {song.Title} por {song.Artist?.StageName ?? "Desconocido"}");
            }

            return Ok("Álbum cargado correctamente en sesión.");
        }



        // Obtiene una canción aleatoria, la agrega a la lista de sesión y devuelve sus datos en JSON.
        [HttpGet]
        public async Task<IActionResult> SiguienteCancion()
        {
            Console.WriteLine("ENtro al metodo SiguienteCancion");
            try
            {
                var cancion = await _httpClient.GetFromJsonAsync<Song>("songs/Random");
                if (cancion == null)
                {
                    Console.WriteLine("No se encontró canción aleatoria.");
                    return Json(new { success = false, message = "No se encontró ninguna canción aleatoria." });
                }

                var lista = ObtenerListaDeSesion() ?? new List<Song>();

                // Agregar la canción si no está
                if (!lista.Any(c => c.Id == cancion.Id))
                {
                    lista.Add(cancion);
                    GuardarListaEnSesion(lista);
                    Console.WriteLine($"Agregada canción nueva al historial: {cancion.Title} (ID: {cancion.Id})");
                }
                else
                {
                    Console.WriteLine($"La canción ya está en el historial: {cancion.Title} (ID: {cancion.Id})");
                }

                var index = lista.FindIndex(c => c.Id == cancion.Id);
                GuardarIndiceActual(index);

                return Json(new
                {
                    success = true,
                    cancion = new
                    {
                        id = cancion.Id,
                        titulo = cancion.Title,
                        artista = cancion.Artist?.StageName ?? "Desconocido",
                        url = cancion.FilePath,
                        portada = cancion.ImagePath ?? "/img/default-album.jpg",
                        idCliente = await GetClientId()
                    }
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ERROR en SiguienteCancion: {ex.Message}");
                return Json(new { success = false, message = $"Error al obtener canción aleatoria: {ex.Message}" });
            }
        }
        private int ObtenerIndiceActual() => HttpContext.Session.GetInt32(SessionKeyIndiceActual) ?? 0;


        [HttpGet]
        public IActionResult GetIndiceActual()
        {
            var index = ObtenerIndiceActual();
            return Json(new { index });
        }


        [HttpGet]
        public IActionResult CancionAnterior()
        {
            Console.WriteLine("Entrando al método CancionAnterior");
            var lista = ObtenerListaDeSesion();
            if (lista == null || !lista.Any())
            {
                Console.WriteLine("Historial vacío al intentar retroceder.");
                return Json(new { success = false, message = "No hay historial de canciones." });
            }

            var index = ObtenerIndiceActual();
            Console.WriteLine($"Índice actual antes de retroceder: {index}");

            index = (index - 1 + lista.Count) % lista.Count;
            GuardarIndiceActual(index);

            var cancion = lista[index];
            Console.WriteLine($"Reproduciendo canción anterior: {cancion.Title} (ID: {cancion.Id})");

            return Json(new
            {
                success = true,
                cancion = new
                {
                    id = cancion.Id,
                    titulo = cancion.Title,
                    artista = cancion.Artist?.StageName ?? "Desconocido",
                    url = cancion.FilePath,
                    portada = cancion.ImagePath ?? "/img/default-album.jpg"
                }
            });
        }




        private async Task<int?> GetClientId()
        {
            string userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return 0; // Usuario no identificado
            var client = await Crud<Modelos.Tuneflow.User.Consumer.Client>.GetClientePorUsuarioId(userId);
            return client.Id;

        }

        // Verifica si una canción específica es favorita para el cliente actual.
        [HttpGet]
        public async Task<IActionResult> ComprobarFavorito(int songId)
        {
            try
            {
                var clientId = await GetClientId();
                if (clientId == 0)
                    return Json(new { esFavorito = false, error = "Usuario no identificado." });
                var response = await _httpClient.GetAsync($"FavoriteSongs/IsFavorite/{songId}/{clientId}");
                if (response.IsSuccessStatusCode)
                {
                    var favorito = await response.Content.ReadFromJsonAsync<object>();
                    return Json(new { esFavorito = true, favorito });
                }
                else if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    return Json(new { esFavorito = false });
                }
                return Json(new { esFavorito = false, error = "Error al verificar favorito" });
            }
            catch (Exception ex)
            {
                return Json(new { esFavorito = false, error = ex.Message });
            }
        }

        // Agrega una canción a la lista de favoritos del cliente y a su playlist de favoritos.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AgregarFavorito(int songId)
        {
            try
            {
                var clientId = await GetClientId();
                if (clientId == 0)
                    return Json(new { success = false, message = "Usuario no identificado." });
                var favoriteSong = new
                {
                    id = 0,
                    clientId = clientId,
                    songId = songId,
                    dateAdded = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ss.fffZ")
                };
                var content = new StringContent(JsonSerializer.Serialize(favoriteSong), Encoding.UTF8, "application/json");
                var response = await _httpClient.PostAsync("FavoriteSongs", content);
                if (response.IsSuccessStatusCode)
                {
                    var playlistResponse = await _httpClient.GetAsync($"Playlists/FavoritesPlaylist/{clientId}");
                    if (playlistResponse.IsSuccessStatusCode)
                    {
                        var playlistId = await playlistResponse.Content.ReadFromJsonAsync<int>();
                        var songPlaylist = new
                        {
                            id = 0,
                            songId = songId,
                            playlistId = playlistId
                        };
                        var playlistContent = new StringContent(JsonSerializer.Serialize(songPlaylist), Encoding.UTF8, "application/json");
                        await _httpClient.PostAsync("SongsPlaylists", playlistContent);
                    }
                    return Json(new { success = true, message = "Canción agregada a favoritos" });
                }
                return Json(new { success = false, message = "Error al agregar a favoritos" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        // Elimina una canción de la lista de favoritos del cliente y de su playlist de favoritos.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EliminarFavorito(int songId)
        {
            try
            {
                var clientId = await GetClientId();
                if (clientId == 0)
                    return Json(new { success = false, message = "Usuario no identificado." });
                var favoritoResponse = await _httpClient.GetAsync($"FavoriteSongs/IsFavorite/{songId}/{clientId}");
                if (!favoritoResponse.IsSuccessStatusCode)
                    return Json(new { success = false, message = "Favorito no encontrado" });
                var favorito = await favoritoResponse.Content.ReadFromJsonAsync<JsonElement>();
                if (!favorito.TryGetProperty("id", out var favoritoIdProp) || !favoritoIdProp.TryGetInt32(out int favoritoId))
                    return Json(new { success = false, message = "Id del favorito inválido" });
                var deleteResponse = await _httpClient.DeleteAsync($"FavoriteSongs/{favoritoId}");
                if (deleteResponse.IsSuccessStatusCode)
                {
                    var playlistResponse = await _httpClient.GetAsync($"Playlists/FavoritesPlaylist/{clientId}");
                    if (playlistResponse.IsSuccessStatusCode)
                    {
                        var playlistId = await playlistResponse.Content.ReadFromJsonAsync<int>();
                        var songPlaylistResponse = await _httpClient.GetAsync($"SongsPlaylists/ExistSongPlaylist/{songId}/{playlistId}");
                        if (songPlaylistResponse.IsSuccessStatusCode)
                        {
                            var songPlaylist = await songPlaylistResponse.Content.ReadFromJsonAsync<JsonElement>();
                            if (songPlaylist.TryGetProperty("id", out var songPlaylistIdProp) && songPlaylistIdProp.TryGetInt32(out int songPlaylistId))
                            {
                                await _httpClient.DeleteAsync($"SongsPlaylists/{songPlaylistId}");
                            }
                        }
                    }
                    return Json(new { success = true, message = "Canción eliminada de favoritos" });
                }
                return Json(new { success = false, message = "Error al eliminar favorito" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        // Obtiene las playlists del cliente actual, excluyendo la playlist de favoritos.
        [HttpGet]
        public async Task<IActionResult> GetPlaylistsCliente()
        {
            try
            {
                var clientId = await GetClientId();
                if (clientId == 0)
                    return Json(new { success = false, message = "Usuario no identificado." });
                var response = await _httpClient.GetAsync($"Playlists/Cliente/Playlist/{clientId}");
                if (response.IsSuccessStatusCode)
                {
                    var playlists = await response.Content.ReadFromJsonAsync<List<JsonElement>>();
                    var playlistsFiltradas = playlists?.Where(p =>
                        p.GetProperty("title").GetString() != "Tus Me Gusta"
                    ).ToList();
                    return Json(new { success = true, playlists = playlistsFiltradas });
                }
                return Json(new { success = false, message = "Error al obtener playlists" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        // Agrega una canción a una playlist específica.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AgregarAPlaylist(int songId, int playlistId)
        {
            try
            {
                var songPlaylist = new
                {
                    id = 0,
                    songId = songId,
                    playlistId = playlistId
                };
                var content = new StringContent(JsonSerializer.Serialize(songPlaylist), Encoding.UTF8, "application/json");
                var response = await _httpClient.PostAsync("SongsPlaylists", content);
                if (response.IsSuccessStatusCode)
                    return Json(new { success = true, message = "Canción añadida a la playlist" });
                return Json(new { success = false, message = "Error al agregar a la playlist" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        // Obtiene la letra de una canción específica utilizando una API externa.
        [HttpGet]
        public async Task<IActionResult> GetLetra(int id)
        {
            return null;

        }

        // Registra la reproducción de una canción para el cliente actual.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RegistrarReproduccion(int songId)
        {
            if (!User.Identity.IsAuthenticated)
            {
                TempData["Error"] = "Debes iniciar sesión para reproducir.";
                return RedirectToAction("Reproducir", new { id = songId });
            }
            var clientId = await GetClientId();
            if (clientId == 0)
            {
                TempData["Error"] = "No se pudo obtener el ID del usuario.";
                return RedirectToAction("Reproducir", new { id = songId });
            }
            var response = await _httpClient.PostAsync($"reproductor/play?songId={songId}&clientId={clientId}", null);
            if (response.IsSuccessStatusCode)
                TempData["Mensaje"] = "🎶 Reproducción registrada con éxito.";
            else
                TempData["Error"] = "❌ Error al registrar la reproducción.";
            return RedirectToAction("Reproducir", new { id = songId });
        }



        public async Task<string?> ObtenerNombreArtista(int songId)
        {
            try
            {
                var nombre = await _httpClient.GetFromJsonAsync<string>($"songs/Artista/{songId}");
                return nombre;
            }
            catch (Exception)
            {
                return null;
            }
        }

        // Obtiene el título de una canción específica por su ID.
        public async Task<string?> ObtenerNombreCancion(int songId)
        {
            try
            {
                var cancion = await _httpClient.GetFromJsonAsync<Song>($"songs/{songId}");
                if (cancion == null)
                    return null;
                return cancion.Title;
            }
            catch (Exception)
            {
                return null;
            }
        }

        [HttpGet]
        public async Task<ActionResult> ObtenerCancionesAleatorias()
        {
            try
            {
                var canciones = await Crud<Song>.GetCancionesAleatorias();

                return Ok(canciones);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al obtener canciones aleatorias: {ex.Message}");
                return StatusCode(500, "Error interno del servidor al obtener canciones aleatorias.");
            }
        }

        [HttpGet]
        public async Task<ActionResult> ObtenerAnuncios()
        {
            try
            {
                var anuncios = await Crud<ADS>.GetAnuncios();

                return Ok(anuncios);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al obtener canciones aleatorias: {ex.Message}");
                return StatusCode(500, "Error interno del servidor al obtener anuncios.");
            }
        }

        [HttpGet]
        public async Task<IActionResult> ComprobarSuscripcion()
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                var cliente = await Crud<Modelos.Tuneflow.User.Consumer.Client>.GetClientePorUsuarioId(userId);
                if (cliente == null)
                {
                    return Json(new { tieneSuscripcion = false, error = "Usuario no encontrado." });
                }

                var suscripcionId = cliente.SubscriptionId;

                if (suscripcionId == 0)
                {
                    return Json(new { tieneSuscripcion = false, error = "Usuario no tiene suscripción." });
                }
                var suscripcion = await Crud<Subscription>.GetByIdAsync(suscripcionId);
                if (suscripcion == null)
                {
                    return Json(new { tieneSuscripcion = false, error = "Suscripción no encontrada." });
                }

                if (suscripcion.SubscriptionTypeId == 1)
                {
                    return Json(new { tieneSuscripcion = false, error = "Error tiene suscripcion Free" });
                }
                else
                {
                    return Json(new { tieneSuscripcion = true, error = "No hay error" });
                }
            }
            catch (Exception ex)
            {
                return Json(new { tieneSuscripcion = false, error = ex.Message });
            }
        }

        [HttpGet]
        public async Task<ActionResult> ObtenerCancionesPorPlaylist(int idPlaylist)
        {
            try
            {
                var canciones = await Crud<SongPlaylist>.GetCancionesPorPlaylist(idPlaylist);

                return Ok(canciones);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al obtener canciones aleatorias: {ex.Message}");
                return StatusCode(500, "Error interno del servidor al obtener canciones aleatorias.");
            }
        }

        [HttpPut]
        public async Task<ActionResult> ActualizarEstadisticasReproduccion(int idCancion)
        {
            try
            {
                var cancion = await Crud<Song>.GetByIdAsync(idCancion);
                if (cancion == null)
                {
                    return NotFound("Canción no encontrada.");
                }
                var artista = await Crud<Modelos.Tuneflow.User.Production.Artist>.GetByIdAsync(cancion.ArtistId);
                if (artista == null)
                {
                    return NotFound("Artista no encontrado.");
                }
                var estadistica = await Crud<ArtistStatistics>.GetArtistStatisticsByArtist(artista.Id);

                var reproducciones = estadistica.TotalPlays;

                estadistica.TotalPlays = reproducciones + 1;

                var newEstadistica = await Crud<ArtistStatistics>.UpdateAsync(estadistica.Id, estadistica);
                return NoContent();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al obtener canciones por álbum: {ex.Message}");
                return StatusCode(500, "Error interno del servidor al obtener canciones por álbum.");
            }
        }
    }
}

