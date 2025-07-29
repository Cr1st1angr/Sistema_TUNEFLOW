using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Modelos.Tuneflow.User.Administration;
using API.Consumer;
using Microsoft.AspNetCore.Authorization;

namespace MVC.TUNEFLOW.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "admin")]
    public class AdminController : Controller
    {
        // GET: AdminController
        public async Task<ActionResult> Index()
        {
            var admins = await Crud<Administrator>.GetAllAsync();
            return View(admins);
        }

        // GET: AdminController/Details/5
        public async Task<ActionResult> DetailsAsync(int id)
        {
            var admin = await Crud<Administrator>.GetByIdAsync(id);
            return View(admin);
        }

        // GET: AdminController/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: AdminController/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(Administrator admin)
        {
            try
            {
                Crud<Administrator>.CreateAsync(admin);
                return RedirectToAction(nameof(Index));
            }
            catch(Exception ex)
            {

                ModelState.AddModelError("", "No se pudo crear el administrador: " + ex.Message);
                
                return View();
            }
        }

        // GET: AdminController/Edit/5
        public ActionResult Edit(int id)
        {
            var admin = Crud<Administrator>.GetByIdAsync(id).Result;
            return View();
        }

        // POST: AdminController/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(int id, Administrator admin)
        {
            try
            {
                Crud<Administrator>.UpdateAsync(id, admin);
                return RedirectToAction(nameof(Index));
            }
            catch(Exception ex)
            {
                ModelState.AddModelError("", "No se pudo actualizar el administrador: " + ex.Message);
                
                return View();
            }
        }

        // GET: AdminController/Delete/5
        public async Task<ActionResult> DeleteAsync(int id)
        {
            var admin = await Crud<Administrator>.GetByIdAsync(id);
            return View(admin);
        }

        // POST: AdminController/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(int id, Administrator admin)
        {
            try
            {
                Crud<Administrator>.DeleteAsync(id);
                return RedirectToAction(nameof(Index));
            }
            catch(Exception ex)
            {
                ModelState.AddModelError("", "No se pudo eliminar el administrador: " + ex.Message);
                
                return View();
            }
        }
    }
}
