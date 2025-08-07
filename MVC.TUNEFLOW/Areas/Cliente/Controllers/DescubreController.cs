using Microsoft.AspNetCore.Mvc;
using API.Consumer; // Asegúrate de tener tu clase Crud<T> aquí
using Modelos.Tuneflow.Media; // Modelo de canciones
using System.Threading.Tasks;
using MVC.TUNEFLOW.Models;
using Microsoft.AspNetCore.Authorization;
using Modelos.Tuneflow.User.Production;
using Modelos.Tuneflow.User;
using System.Security.Claims;
using Modelos.Tuneflow.Models;

namespace MVC.TUNEFLOW.Areas.Cliente.Controllers
{
    [Area("Cliente")]
    [Authorize(Roles = "cliente")]
    public class DescubreController : Controller
    {
        private readonly HttpClient _httpClient;

        public DescubreController(HttpClient httpClient)
        {
            _httpClient = httpClient;
            Crud<Song>.EndPoint = "https://localhost:7031/api/Songs";
        }

        public async Task<IActionResult> Index()
        {

            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                var cliente = await Crud<Modelos.Tuneflow.User.Consumer.Client>.GetClientePorUsuarioId(userId);


                ViewBag.IdCliente = cliente.Id;
                var canciones = await Crud<Song>.GetAllAsync();
                var generos = await Crud<Genre>.GetAllAsync();

                return View(generos);
            }
            catch (Exception ex)
            {
                return View("Error", new ErrorViewModel { ErrorMessage = ex.Message });
            }
        }

        public async Task<IActionResult> Genre(string genre)
        {
            if (string.IsNullOrEmpty(genre))
            {
                return RedirectToAction(nameof(Index));
            }

            try
            {
                var canciones = await Crud<Song>.GetCancionesPorPalabrasClave(genre);

                foreach(var cancion in canciones)
                {
                    cancion.Artist = await Crud<Artist>.GetByIdAsync(cancion.ArtistId);
                }

                ViewBag.Genre = genre;

                return View(canciones);
            }
            catch (Exception ex)
            {
                return View("Error", new ErrorViewModel { ErrorMessage = ex.Message });
            }
        }
    }
}