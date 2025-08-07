using API.Consumer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Modelos.Tuneflow.Models;
using Modelos.Tuneflow.User.Production;
using Modelos.Tuneflow.User.Profiles;

namespace MVC.TUNEFLOW.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "admin")]
    public class PeticionesController : Controller
    {
        public async Task<IActionResult> Index()
        {
            var peticiones = await Crud<ArtistVerificationRequest>.GetAllAsync();

            foreach (var peticion in peticiones)
            {
                peticion.Artist = await Crud<Artist>.GetByIdAsync(peticion.ArtistId);
                if (peticion.Artist != null)
                {
                    peticion.Artist.Profile = await Crud<Profile>.GetPerfilPorArtistaId(peticion.Artist.Id);
                    peticion.Artist.Country = await Crud<Country>.GetByIdAsync(peticion.Artist.CountryId);
                }
            }

            return View(peticiones);
        }

        public async Task<IActionResult> Details(int id)
        {
            var peticion = await Crud<ArtistVerificationRequest>.GetByIdAsync(id);
            if (peticion == null) return NotFound();

            var artista = await Crud<Artist>.GetByIdAsync(peticion.ArtistId);
            if (artista != null)
            {
                artista.Profile = await Crud<Profile>.GetPerfilPorArtistaId(artista.Id);
                artista.Country = await Crud<Country>.GetByIdAsync(artista.CountryId);
            }

            ViewBag.Artista = artista;
            return View(peticion);
        }

        public async Task<IActionResult> Delete(int id)
        {
            var peticion = await Crud<ArtistVerificationRequest>.GetByIdAsync(id);
            if (peticion == null) return NotFound();

            var artista = await Crud<Artist>.GetByIdAsync(peticion.ArtistId);
            if (artista != null)
            {
                artista.Profile = await Crud<Profile>.GetPerfilPorArtistaId(artista.Id);
                artista.Country = await Crud<Country>.GetByIdAsync(artista.CountryId);
            }

            ViewBag.Artista = artista;
            return View(peticion);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            try
            {
                await Crud<ArtistVerificationRequest>.DeleteAsync(id);
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                var peticion = await Crud<ArtistVerificationRequest>.GetByIdAsync(id);
                ModelState.AddModelError("", "No se pudo eliminar la petición: " + ex.Message);
                return RedirectToAction(nameof(Index));
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> VerificarArtista(int artistId, int peticionId)
        {
            try
            {
                var artista = await Crud<Artist>.GetByIdAsync(artistId);
                if (artista == null) return NotFound();

                artista.Verified = true;
                await Crud<Artist>.UpdateAsync(artistId, artista);
                await Crud<ArtistVerificationRequest>.DeleteAsync(peticionId);

                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "Error al verificar artista: " + ex.Message);
                return RedirectToAction(nameof(Index));
            }
        }
    }
}
