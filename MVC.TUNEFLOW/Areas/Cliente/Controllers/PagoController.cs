using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using API.Consumer;
using Modelos.Tuneflow.Payments;
using Microsoft.AspNetCore.Authorization;
using System.Reflection.PortableExecutable;
using Modelos.Tuneflow.User.Consumer;
using System.Security.Claims;

namespace MVC.TUNEFLOW.Areas.Cliente.Controllers
{
    [Area("Cliente")]
    [Authorize(Roles = "cliente")]
    public class PagoController : Controller
    {
        
        [Authorize]
        // GET: PagoController
        public async Task<IActionResult> Index()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var cliente = await Crud<Modelos.Tuneflow.User.Consumer.Client>.GetClientePorUsuarioId(userId);
            ViewBag.IdCliente = cliente.Id;
            var pagos = await Crud<Modelos.Tuneflow.Payments.Payment>.GetAllAsync();
            return View(pagos);
        }

        // GET: PagoController/Details/5
        public async Task<IActionResult> Details(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var cliente = await Crud<Modelos.Tuneflow.User.Consumer.Client>.GetClientePorUsuarioId(userId);


            ViewBag.IdCliente = cliente.Id;
            var pago = await Crud<Modelos.Tuneflow.Payments.Payment>.GetByIdAsync(id);
            return View(pago);
        }

        // GET: PagoController/Create
        [HttpGet]
        public async Task<ActionResult> Create(int id)
        {
            Console.WriteLine($"El tipo de Subcripcion es de id: {id}");
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var client = await Crud<Modelos.Tuneflow.User.Consumer.Client>.GetClientePorUsuarioId(userId);
            ViewBag.IdCliente = client.Id;
            var TypePago = await Crud<SubscriptionType>.GetByIdAsync(id);
            if (TypePago == null)
            {
                return RedirectToAction("Planes", "Planes", new { area = "Cliente" });
            }
            ViewBag.Precio = TypePago.Price;
            ViewBag.ClienteId = client.Id; 
            TempData["Tipo"] = TypePago.Name;
            return View();
        }

        // POST: PagoController/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create(Payment pago)
        {
            try
            {
                Console.WriteLine($"Entroooo a Crearrrrr");
                Console.WriteLine($"ClientId: {pago.ClientId}");
                Console.WriteLine($"PaymentMethod: {pago.PaymentMethod}");
                Console.WriteLine($"PaymentDate: {pago.PaymentDate}");
                Console.WriteLine($"Amount: {pago.Amount}");
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                var client = await Crud<Modelos.Tuneflow.User.Consumer.Client>.GetClientePorUsuarioId(userId);
                ViewBag.IdCliente = client.Id;
                var pay = await Crud<Modelos.Tuneflow.Payments.Payment>.CreateAsync(pago);
                if(pay != null)
                {
                    var subscription = await Crud<Subscription>.GetByIdAsync(client.SubscriptionId);
                    var tipo = TempData.Peek("Tipo")?.ToString()?.Trim();
                    Console.WriteLine($"tipo ==== {tipo}");
                    if (tipo == "Plan Premium"){
                        subscription.SubscriptionTypeId = 2; 
                        subscription.NumberMembers = 0; 
                        subscription.JoinCode = null; 
                    }
                    else if(tipo == "Plan Familiar") {
                        var codigo = GenerarCodigoAleatorio();
                        subscription.SubscriptionTypeId = 3;
                        subscription.NumberMembers = 3; 
                        subscription.JoinCode = codigo; 
                    }
                    
                    Console.WriteLine($"FechaIncio: {subscription.StartDate}");
                    Console.WriteLine($"JoinCode: {subscription.JoinCode}");
                    Console.WriteLine($"SubscriptionTypeId: {subscription.SubscriptionTypeId}");
                    Console.WriteLine($"NumberMembers: {subscription.NumberMembers}");
                    var actualizado = await Crud<Subscription>.UpdateAsync(subscription.Id,subscription);
                    Console.WriteLine("Actualización: " + (actualizado ? "Exitosa" : "Fallida"));
                    return RedirectToAction("Panel", "Panel", new { area = "Cliente" });

                }
                else
                {
                    Console.WriteLine($"No se pudo crear el pago");
                    return RedirectToAction("Planes", "Planes", new { area = "Cliente" });
                }
                
            }
            catch(Exception EX)
            {

                ModelState.AddModelError("", "Hubo un error al procesar el pago.");
                Console.WriteLine($"ERRORASO: {EX.Message}");
                return View("Create", pago); 
            }
        }

        public static string GenerarCodigoAleatorio()
        {
            const string letras = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
            Random random = new Random();
            char[] codigo = new char[6];

            for (int i = 0; i < codigo.Length; i++)
            {
                codigo[i] = letras[random.Next(letras.Length)];
            }

            return new string(codigo);
        }

        // GET: PagoController/Edit/5
        public ActionResult Edit(int id)
        {
            return View();
        }

        public ActionResult Delete(int id)
        {
            return View();
        }

        // POST: PagoController/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(int id, Payment pago)
        {
            try
            {
                Crud<Modelos.Tuneflow.Payments.Payment>.DeleteAsync(id);
                return RedirectToAction(nameof(Index));
            }
            catch(Exception ex)
            {
                return View(pago);
            }
        }
    }
}
