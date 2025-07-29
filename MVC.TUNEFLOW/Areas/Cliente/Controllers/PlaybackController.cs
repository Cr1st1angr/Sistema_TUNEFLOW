using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace MVC.TUNEFLOW.Areas.Cliente.Controllers
{

    [Area("Cliente")]
    [Authorize(Roles = "cliente")]
    public class PlaybackController : Controller
    {
        // GET: PlayBackController
        public ActionResult Index()
        {
            return View();
        }

        // GET: PlayBackController/Details/5
        public ActionResult Details(int id)
        {
            return View();
        }

        // GET: PlayBackController/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: PlayBackController/Create
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

        // GET: PlayBackController/Edit/5
        public ActionResult Edit(int id)
        {
            return View();
        }

        // POST: PlayBackController/Edit/5
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

        // GET: PlayBackController/Delete/5
        public ActionResult Delete(int id)
        {
            return View();
        }

        // POST: PlayBackController/Delete/5
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
