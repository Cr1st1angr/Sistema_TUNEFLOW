using API.Consumer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Modelos.Tuneflow.User.Administration;
using System.Security.Claims;

namespace MVC.TUNEFLOW.Areas.Artista.Controllers
{
    [Area("Artista")]
    [Authorize(Roles = "artista")]
    public class EstadisticasController : Controller
    {
        // GET: EstadisticasController
        public async Task<ActionResult> Index()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return RedirectToAction("Login", "Account");
            }
            var artista = await Crud<Modelos.Tuneflow.User.Production.Artist>.GetArtistaPorUsuarioId(userId);
            var estadistica = await Crud<ArtistStatistics>.GetArtistStatisticsByArtist(artista.Id);
            return View(estadistica);
        }

        // GET: EstadisticasController/Details/5
        public ActionResult Details(int id)
        {
            var estadistica = Crud<ArtistStatistics>.GetByIdAsync(id);
            return View(estadistica);
        }

        // GET: EstadisticasController/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: EstadisticasController/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(ArtistStatistics estadisticas)
        {
            try
            {
                Crud<ArtistStatistics>.CreateAsync(estadisticas);
                return RedirectToAction(nameof(Index));
            }
            catch(Exception ex)
            {
                ModelState.AddModelError("", "Unable to create statistics. Please try again.");
                return View(estadisticas);
    
            }
        }

        // GET: EstadisticasController/Edit/5
        public ActionResult Edit(int id)
        {
            var estadistica = Crud<ArtistStatistics>.GetByIdAsync(id);
            return View(estadistica);
        }

        // POST: EstadisticasController/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(int id, ArtistStatistics estadisticas)
        {
            try
            {
                Crud<ArtistStatistics>.UpdateAsync(id, estadisticas);
                return RedirectToAction(nameof(Index));
            }
            catch(Exception ex)
            {
                ModelState.AddModelError("", "Unable to update statistics. Please try again.");
                return View(estadisticas);
            }
        }

        // GET: EstadisticasController/Delete/5
        public ActionResult Delete(int id)
        {
            var estadistica = Crud<ArtistStatistics>.GetByIdAsync(id);
            return View(estadistica);
        }

        // POST: EstadisticasController/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(int id, ArtistStatistics estadistica)
        {
            try
            {
                Crud<ArtistStatistics>.DeleteAsync(id);
                return RedirectToAction(nameof(Index));
            }
            catch(Exception ex)
            {

                ModelState.AddModelError("", "Unable to delete statistics. Please try again.");

                    return View(estadistica);
            }
        }
    }
}
