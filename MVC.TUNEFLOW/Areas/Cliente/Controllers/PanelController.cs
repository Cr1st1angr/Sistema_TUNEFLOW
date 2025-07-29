using Microsoft.AspNetCore.Mvc;
using Modelos.Tuneflow.Models;
using API.Consumer;
using Microsoft.AspNetCore.Authorization;
using System.Text.Json;
using Modelos.Tuneflow.Media;
using System.Security.Claims;
using Modelos.Tuneflow.User.Consumer;
using Modelos.Tuneflow.User.Production;

namespace MVC.TUNEFLOW.Areas.Cliente.Controllers
{
    [Area("Cliente")]
    [Authorize(Roles = "cliente")]
    public class PanelController : Controller
    {
        public async Task<IActionResult> Panel()
        {

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var client = await Crud<Modelos.Tuneflow.User.Consumer.Client>.GetClientePorUsuarioId(userId);
            ViewBag.IdCliente = client.Id;
            var subscription = await Crud<Subscription>.GetByIdAsync(client.SubscriptionId);
            ViewBag.TipoSuscripcion = subscription.SubscriptionType.Id;
            var paises = await Crud<Country>.GetAllAsync();
            return View(paises);
        }

        [HttpGet]
        public async Task<IActionResult> CancionesPorPais(int paisId)
        {
            string userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(userId))
            {

                return RedirectToAction("Login", "Account");
            }
            var client = await Crud<Modelos.Tuneflow.User.Consumer.Client>.GetClientePorUsuarioId(userId);

            ViewBag.IdCliente = client.Id;

            using var httpClient = new HttpClient();

            var response = await httpClient.GetAsync($"https://localhost:7031/api/Countries/{paisId}/songs");

            if (!response.IsSuccessStatusCode)
                return NotFound($"No se pudo obtener canciones del país con ID {paisId}.");

            var json = await response.Content.ReadAsStringAsync();

            var canciones = JsonSerializer.Deserialize<List<Song>>(json, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            foreach (var cancion in canciones)
            {
                var artista = await Crud<Artist>.GetByIdAsync(cancion.ArtistId);
                cancion.Artist = artista;
            }
            return View(canciones);
        }
    }
}
