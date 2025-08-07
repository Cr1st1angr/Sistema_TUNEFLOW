using API.Consumer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Modelos.Tuneflow.User.Production;
using System.Threading.Tasks;

namespace MVC.TUNEFLOW.Areas.Cliente.Controllers
{
    [Area("Cliente")]
    [Authorize(Roles = "cliente")]
    public class RegistroController : Controller
    {
        [AcceptVerbs("GET", "POST")]
        public async Task<IActionResult> VerificarStageName([FromQuery(Name = "Input.StageName")] string stageName)
        {
            if (string.IsNullOrEmpty(stageName))
                return Json("El nombre artístico es obligatorio.");

            var esValido = await Crud<Artist>.ComprobarSiStageNameExiste(stageName);
            return Json(esValido ? true : "El nombre artístico ya está en uso.");
        }
    }
}
