using API.Consumer;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Modelos.Tuneflow.User.Administration;
using System.Security.Claims;

namespace MVC.TUNEFLOW.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "admin")]
    public class CuentaController : Controller
    {
        public async Task<IActionResult> Details()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var admin = await Crud<Modelos.Tuneflow.User.Administration.Administrator>.GetAdminPorUsuarioId(userId);
            if (admin == null) return NotFound();
            return View(admin);
        }

        public async Task<IActionResult> Edit()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var admin = await Crud<Modelos.Tuneflow.User.Administration.Administrator>.GetAdminPorUsuarioId(userId);
            if (admin == null) return NotFound();
            return View(admin);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Modelos.Tuneflow.User.Administration.Administrator admin)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var adminDb = await Crud<Administrator>.GetAdminPorUsuarioId(userId);

            if (adminDb == null || adminDb.Id != admin.Id)
                return BadRequest();

            if (!ModelState.IsValid)
                return View(admin);

            try
            {
                adminDb.FirstName = admin.FirstName;
                adminDb.LastName = admin.LastName;
                adminDb.Phone = admin.Phone;
                adminDb.BirthDate = admin.BirthDate;

                await Crud<Administrator>.UpdateAsync(adminDb.Id, adminDb);

                TempData["SuccessMessage"] = "Datos actualizados correctamente";
                return RedirectToAction(nameof(Details));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, $"Ocurrió un error al actualizar: {ex.Message}");
                return View(adminDb);
            }
        }

    }
}