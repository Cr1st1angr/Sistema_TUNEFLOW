using System.Data;
using API.Consumer;
using Dapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Modelos.Tuneflow.Media;
using Modelos.Tuneflow.User.Production;
using System.Linq;
using System.Threading.Tasks;
using System;
using System.Security.Claims;

namespace MVC.TUNEFLOW.Areas.Artista.Controllers
{
    [Area("Artista")]
    [Authorize(Roles = "artista")]
    public class ArtistaController : Controller
    {
        private readonly IDbConnection _db;

        public ArtistaController(IDbConnection db)
        {
            _db = db;
        }



        public IActionResult Perfil(int id)
        {
            var sql = @"
            SELECT * FROM Artists WHERE Id = @Id;
            SELECT * FROM Songs WHERE ArtistId = @Id;
        ";

            using (var multi = _db.QueryMultiple(sql, new { Id = id }))
            {
                var artista = multi.Read<Modelos.Tuneflow.User.Production.Artist>().FirstOrDefault();
                if (artista != null)
                {
                    artista.Songs = multi.Read<Song>().ToList();
                }

                return View(artista);
            }
        }




        // GET: ArtistaController
        public async Task<IActionResult> Index(int id) // id del artista
        {
            string userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            bool seguido = false;

            if (!string.IsNullOrEmpty(userId))
            {
                var client = await Crud<Modelos.Tuneflow.User.Consumer.Client>.GetClientePorUsuarioId(userId);
                Crud<Follow>.EndPoint = "https://localhost:7031/api/Follows";

                var follows = await Crud<Follow>.GetFollowsByClientIdAsync(client.Id);
                seguido = follows.Any(f => f.ArtistId == id);
            }

            ViewBag.Seguido = seguido;

            var artista = await Crud<Artist>.GetByIdAsync(id);

            var canciones = await Crud<Song>.GetCancionesPorArtistaId(id);
            ViewBag.Canciones = canciones;

            return View(artista);
        }

        private Modelos.Tuneflow.User.Production.Artist ObtenerArtista(int id)
        {
            var sql = @"
        SELECT * FROM Artists WHERE Id = @Id;
        SELECT * FROM Songs WHERE ArtistId = @Id;
    ";

            using (var multi = _db.QueryMultiple(sql, new { Id = id }))
            {
                var artista = multi.Read<Modelos.Tuneflow.User.Production.Artist>().FirstOrDefault();
                if (artista != null)
                {
                    artista.Songs = multi.Read<Song>().ToList();
                }
                return artista;
            }
        }





        // GET: ArtistaController/Details/5
        public ActionResult Details(int id)
        {
            return View();
        }

        // GET: ArtistaController/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: ArtistaController/Create
        [HttpPost]
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

        // GET: ArtistaController/Edit/5
        public ActionResult Edit(int id)
        {
            return View();
        }

        // POST: ArtistaController/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(int id, IFormCollection collection)
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

        // GET: ArtistaController/Delete/5
        public ActionResult Delete(int id)
        {
            return View();
        }

        // POST: ArtistaController/Delete/5
        [HttpPost]
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
        [Area("Artista")]
        [AllowAnonymous]
        [HttpGet("Artist/Profile/{nombreArtistico}")]
        public async Task<IActionResult> ArtistaPorNombre(string nombreArtistico)
        {
            var artistas = await Crud<Modelos.Tuneflow.User.Production.Artist>.GetAllAsync();
            var encontrado = artistas.FirstOrDefault(a => a.StageName == nombreArtistico);

            if (encontrado == null)
                return NotFound();

            var canciones = await Crud<Song>.GetAllAsync();
            encontrado.Songs = canciones.Where(c => c.ArtistId == encontrado.Id).ToList();

            return View("~/Areas/Artista/Views/Perfil/Index.cshtml", encontrado); // devuelve la vista Index con modelo 'Artista'
        }





        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> ObtenerIdArtistaPorCancion(int cancionId)
        {
            try
            {
                var cancion = await Crud<Song>.GetByIdAsync(cancionId);

                if (cancion == null)
                    return NotFound("Canción no encontrada");

                int artistaId = cancion.ArtistId;

                return Ok(artistaId); 
            }
            catch (Exception ex)
            {
                return BadRequest($"Error: {ex.Message}");
            }
        }
        [HttpGet]
        [AllowAnonymous]
        [Route("Artista/ObtenerCancionesPorArtista")]
        public async Task<IActionResult> ObtenerCancionesPorArtista(int artistaId)
        {
            try
            {
                var sql = @"SELECT * FROM ""Songs"" WHERE ""ArtistId"" = @ArtistId;";

                var canciones = await _db.QueryAsync<Song>(sql, new { ArtistaId = artistaId });

                if (!canciones.Any())
                    return NotFound("No se encontraron canciones para este artista.");

                return Ok(canciones);
            }
            catch (Exception ex)
            {
                return BadRequest($"Error: {ex.Message}");
            }
        }






    }
}
