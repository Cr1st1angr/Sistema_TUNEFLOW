using API.Consumer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Threading.Tasks;
using Modelos.Tuneflow.User.Consumer;
using Microsoft.AspNetCore.Authentication;

namespace MVC.TUNEFLOW.Areas.Cliente.Controllers
{
    [Area("Cliente")]
    [Authorize(Roles = "cliente")]
    public class ClienteController : Controller
    {
        // Mostrar detalles del cliente autenticado
        public async Task<IActionResult> Details()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var cliente = await Crud<Modelos.Tuneflow.User.Consumer.Client>.GetClientePorUsuarioId(userId);
            var suscripcion = await Crud<Subscription>.GetByIdAsync(cliente.SubscriptionId);
            cliente.Subscription  = suscripcion;
            var tipoCuenta  = await Crud<SubscriptionType>.GetByIdAsync(suscripcion.SubscriptionTypeId);
            ViewBag.TipoCuenta = tipoCuenta.Name;
            if (cliente == null) return NotFound();
            return View(cliente);
        }

        // Editar cliente (GET) sin parámetro, basado en usuario autenticado
        public async Task<IActionResult> Edit()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var cliente = await Crud<Modelos.Tuneflow.User.Consumer.Client>.GetClientePorUsuarioId(userId);
            if (cliente == null) return NotFound();
            return View(cliente);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Modelos.Tuneflow.User.Consumer.Client cliente)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var clienteDb = await Crud<Modelos.Tuneflow.User.Consumer.Client>.GetClientePorUsuarioId(userId);
            if (clienteDb == null || clienteDb.Id != id)
            {
                return BadRequest();
            }

            if (!ModelState.IsValid)
            {
                return View(cliente);
            }

            try
            {
                clienteDb.FirstName = cliente.FirstName;
                clienteDb.LastName = cliente.LastName;
                clienteDb.Phone = cliente.Phone;
                clienteDb.BirthDate = cliente.BirthDate;

                await Crud<Modelos.Tuneflow.User.Consumer.Client>.UpdateAsync(clienteDb.Id, clienteDb);

                TempData["SuccessMessage"] = "Datos actualizados correctamente";

                return RedirectToAction(nameof(Details));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, $"Ocurrió un error al actualizar el cliente: {ex.Message}");
                return View(clienteDb);
            }
        }




    }
}
