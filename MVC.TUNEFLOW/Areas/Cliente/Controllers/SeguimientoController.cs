using API.Consumer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Modelos.Tuneflow.User.Consumer;
using Modelos.Tuneflow.User.Production;
using Modelos.Tuneflow.User.Profiles;
using Modelos.Tuneflow.User.Administration;

namespace MVC.TUNEFLOW.Areas.Cliente.Controllers
{

    [Area("Cliente")]
    [Authorize(Roles = "cliente")]
    public class SeguimientoController : Controller
    {
        // GET: SeguimientoController
        public async Task<ActionResult> Index()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var client = await Crud<Modelos.Tuneflow.User.Consumer.Client>.GetClientePorUsuarioId(userId);
            var follows = await Crud<Follow>.GetFollowsPorClienteId(client.Id);
            ViewBag.IdCliente = client.Id;
            foreach (var follow in follows)
            {
                var profile = await Crud<Profile>.GetPerfilPorArtistaId(follow.ArtistId);
                follow.Artist.Profile = profile;
            }
            return View(follows);
        }

        public async Task<ActionResult> Seguir(int id)
        {
            string userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(userId))
            {
                return RedirectToAction("Login", "Account");
            }

            var client = await Crud<Modelos.Tuneflow.User.Consumer.Client>.GetClientePorUsuarioId(userId);
            ViewBag.IdCliente = client.Id;

            Crud<Follow>.EndPoint = "https://localhost:7031/api/Follows";
            

            var follows = await Crud<Follow>.GetCustomAsync($"FollowsByCliemnte/{client.Id}");


            var followExistente = follows.FirstOrDefault(f => f.ArtistId == id);

            if (followExistente != null)
            {
                await Crud<Follow>.DeleteAsync(followExistente.Id);
            }
            else
            {
                var nuevoSeguimiento = new Follow
                {
                    ClientId = client.Id,
                    ArtistId = id
                };

                await Crud<Follow>.CreateAsync(nuevoSeguimiento);
            }

            var estadistica = await Crud<ArtistStatistics>.GetArtistStatisticsByArtist(id);

            var seguimientos = estadistica.TotalFollowers;
            estadistica.TotalFollowers = seguimientos + 1;

            await Crud<ArtistStatistics>.UpdateAsync(estadistica.Id, estadistica);

            return RedirectToAction("Index", "Perfil", new { area = "Artista", id = id });
        }


        // GET: SeguimientoController/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: SeguimientoController/Create
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

        // GET: SeguimientoController/Edit/5
        public ActionResult Edit(int id)
        {
            return View();
        }

        // POST: SeguimientoController/Edit/5
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

        // GET: SeguimientoController/Delete/5
        public ActionResult Delete(int id)
        {
            return View();
        }

        // POST: SeguimientoController/Delete/5
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
        [HttpPost]
[ValidateAntiForgeryToken]
public async Task<ActionResult> DejarSeguir(int artistId)
{
    string userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

    if (string.IsNullOrEmpty(userId))
    {
        return RedirectToAction("Login", "Account");
    }

    var client = await Crud<Modelos.Tuneflow.User.Consumer.Client>.GetClientePorUsuarioId(userId);

    // Obtén todos los follow del cliente
    Crud<Follow>.EndPoint = "https://localhost:7031/api/Follows";
    var follows = await Crud<Follow>.GetByAsync("ClientId", client.Id);

    // Busca el follow específico con ese artista
    var seguimiento = follows.FirstOrDefault(f => f.ArtistId == artistId);

    if (seguimiento != null)
    {
        bool eliminado = await Crud<Follow>.DeleteAsync(seguimiento.Id);

        if (eliminado)
        {
            var estadistica = await Crud<ArtistStatistics>.GetArtistStatisticsByArtist(artistId);
            var seguimientoEstadistica = estadistica.TotalFollowers;
            estadistica.TotalFollowers = seguimientoEstadistica - 1;
            await Crud<ArtistStatistics>.UpdateAsync(estadistica.Id, estadistica);
            Console.WriteLine($"Seguimiento eliminado para el artista con ID: {artistId}");
            return RedirectToAction("Index", "Perfil", new { area = "Artista", id = artistId, idCliente = client.Id });
        }
    }

    Console.WriteLine("No se encontró el seguimiento para eliminar.");
    return RedirectToAction("Panel", "Panel");
}


    }
}
