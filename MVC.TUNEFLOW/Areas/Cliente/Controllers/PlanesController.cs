using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using API.Consumer;
using Modelos.Tuneflow.User.Consumer;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace MVC.TUNEFLOW.Areas.Cliente.Controllers
{
    [Area("Cliente")]
    [Authorize(Roles = "cliente")]
    public class PlanesController : Controller
    {
        public async Task<ActionResult> Planes()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var cliente = await Crud<Modelos.Tuneflow.User.Consumer.Client>.GetClientePorUsuarioId(userId);


            ViewBag.IdCliente = cliente.Id;
            var planes = await Crud<SubscriptionType>.GetAllAsync();
            return View(planes);
        }

        // GET: PlanesController/Details/5
        public async Task<ActionResult> Details(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var cliente = await Crud<Modelos.Tuneflow.User.Consumer.Client>.GetClientePorUsuarioId(userId);


            ViewBag.IdCliente = cliente.Id;
            var planes = await Crud<SubscriptionType>.GetByIdAsync(id);
            return View(planes);
        }

        public ActionResult UnirseCodigo()
        {
            return View();
        }

        [HttpPost]
        public async Task<ActionResult> UnirsePorCodigo(string codigo)
        {
            var suscripcion = await Crud<Subscription>.CombrobarCodigoUnion(codigo);

            if (suscripcion == null)
            {
                ModelState.AddModelError("", "El código de unión no es válido o no existe.");
                return View("UnirseCodigo");
            }

            var miembros = suscripcion.NumberMembers;

            if(miembros > 0)
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                var cliente = await Crud<Modelos.Tuneflow.User.Consumer.Client>.GetClientePorUsuarioId(userId);

                var suscripcionCliente = await Crud<Subscription>.GetByIdAsync(cliente.SubscriptionId);
                suscripcionCliente.SubscriptionTypeId = 2; 
                await Crud<Subscription>.UpdateAsync(suscripcionCliente.Id ,suscripcionCliente);

                suscripcion.NumberMembers = miembros - 1; 
                
                await Crud<Subscription>.UpdateAsync(suscripcion.Id,suscripcion);
                return RedirectToAction("Panel", "Panel", new { area = "Cliente" });
            }

            ModelState.AddModelError("", "Este plan ya tiene 4 miembros");
            return View("UnirseCodigo");

        }



    }
}
