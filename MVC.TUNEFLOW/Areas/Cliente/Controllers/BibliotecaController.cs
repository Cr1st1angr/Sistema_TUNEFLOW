using API.Consumer;
using System.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Modelos.Tuneflow.Media;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace MVC.TUNEFLOW.Areas.Cliente.Controllers
{
    [Area("Cliente")]
    [Authorize(Roles = "cliente")]
    public class BibliotecaController : Controller
    {
        // GET: BibliotecaController
        public async Task<IActionResult> Index()
        {
            string userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(userId))
            {

                return RedirectToAction("Login", "Account");
            }
            var client = await Crud<Modelos.Tuneflow.User.Consumer.Client>.GetClientePorUsuarioId(userId);

            ViewBag.IdCliente = client.Id;
            Debug.WriteLine("Entrando a Index");

            // Obtiene todas las canciones sin filtro
            var canciones = await Crud<Song>.GetAllAsync();

            Console.WriteLine($"Número de canciones recibidas en Index: {canciones?.Count ?? 0}");

            return View(canciones);
        }

        // GET: BibliotecaController/Details/5
        public ActionResult Details(int id)
        {
            return View();
        }

        // GET: BibliotecaController/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: BibliotecaController/Create
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

        // GET: BibliotecaController/Edit/5
        public ActionResult Edit(int id)
        {
            return View();
        }

        // POST: BibliotecaController/Edit/5
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

        // GET: BibliotecaController/Delete/5
        public ActionResult Delete(int id)
        {
            return View();
        }

        // POST: BibliotecaController/Delete/5
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
    }
}
