using API.Consumer;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Threading.Tasks;
using Modelos.Tuneflow.User.Consumer;
using Modelos.Tuneflow.User.Profiles;
using Microsoft.AspNetCore.Authorization;
using System;
using System.Text.RegularExpressions;


namespace MVC.TUNEFLOW.Areas.Client.Controllers
{
    [Area("Cliente")]
    [Authorize(Roles = "cliente")]
    public class PerfilController : Controller
    {
        // GET: PerfilController
        public async Task<IActionResult> Index()
        {

            string userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            Console.WriteLine($"UserId: {userId}");

            if (string.IsNullOrEmpty(userId))
            {
                return RedirectToAction("Login", "Account");
            }

            var cliente = await Crud<Modelos.Tuneflow.User.Consumer.Client>.GetClientePorUsuarioId(userId);
            ViewBag.IdCliente = cliente.Id;
            if (cliente == null)
            {
                Console.WriteLine($"❌ No se encontró cliente para userId: {userId}");
                return RedirectToAction("Index", "Buscar");
            }


            var perfil = await Crud<Profile>.GetPerfilPorClienteId(cliente.Id);
            if (perfil == null)
            {
                Console.WriteLine($"❌ No se encontró perfil para clienteId: {cliente.Id}");
                return RedirectToAction("Create", "Perfil"); 
            }

            Console.WriteLine($"✅ Perfil encontrado para clienteId: {cliente.Id}");

            return View(perfil);
        }


        // GET: PerfilController/Details/5
        public ActionResult Details(int id)
        {
            return View();
        }

        // GET: PerfilController/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: PerfilController/Create
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

        // GET: PerfilController/Edit/5
        public async Task<ActionResult> Edit(int id)
        {
            var perfil = await Crud<Profile>.GetByIdAsync(id);
            return View(perfil);
        }

        [AllowAnonymous]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit(int id, IFormFile ImagenPerfil, string Biografia)
        {
            Console.WriteLine($"Perfil con id: {id}");
            try
            {
                string supabaseUrl = "https://kblhmjrklznspeijwzeg.supabase.co";
                string supabaseAnonKey = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJpc3MiOiJzdXBhYmFzZSIsInJlZiI6ImtibGhtanJrbHpuc3BlaWp3emVnIiwicm9sZSI6ImFub24iLCJpYXQiOjE3NTA4MDk2MDcsImV4cCI6MjA2NjM4NTYwN30.CpoCYjAUi4ijZzAEqi9R_3HeGq5xpWANMMIlAQjJx-o"; // La API key de anon pública
                string bucket = "imagenesusuarios"; 
                string carpeta = "Usuarios";   

                var perfil = await Crud<Profile>.GetByIdAsync(id);

                if (perfil == null)
                {
                    Console.WriteLine("Perfil no encontrado");
                    return NotFound();
                }

                if(ImagenPerfil != null && ImagenPerfil.Length > 0)
                {
                    var fileNameClean = Path.GetFileNameWithoutExtension(ImagenPerfil.FileName);
                    fileNameClean = Regex.Replace(fileNameClean, @"[^a-zA-Z0-9_\-]", "");
                    var extension = Path.GetExtension(ImagenPerfil.FileName);
                    var nombreArchivo = $"{DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()}_{fileNameClean}{extension}";
                    var rutaArchivo = $"{carpeta}/{nombreArchivo}";
                    
                    using (var client = new HttpClient())
                    {
                        client.BaseAddress = new Uri(supabaseUrl);

                        client.DefaultRequestHeaders.Add("apikey", supabaseAnonKey);
                        client.DefaultRequestHeaders.Add("Authorization", $"Bearer {supabaseAnonKey}");

                        using (var memori = new MemoryStream())
                        {
                            await ImagenPerfil.CopyToAsync(memori);
                            var contenido = new ByteArrayContent(memori.ToArray());
                            contenido.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(ImagenPerfil.ContentType);

                            var response = await client.PutAsync($"/storage/v1/object/{bucket}/{rutaArchivo}", contenido);

                            if (!response.IsSuccessStatusCode)
                            {
                                var error = await response.Content.ReadAsStringAsync();
                                ModelState.AddModelError("", "Error al subir la imagen: " + error);
                                Console.WriteLine($"Error al subir la imagen: {response.StatusCode} - {error}");
                                return View(perfil);
                            }
                        }
                    }

                    var urlDefault = "https://kblhmjrklznspeijwzeg.supabase.co/storage/v1/object/public/imagenestuneflow/PerfilesDefecto/ImagenDefault.jpeg";

                    if (!string.Equals(urlDefault, perfil.ProfileImage, StringComparison.OrdinalIgnoreCase))
                    {
                        var UrlimagenAntigua = perfil.ProfileImage;
                        var basePath = $"/storage/v1/object/public/{bucket}/";

                        if (UrlimagenAntigua.Contains(basePath))
                        {
                            var uri = new Uri(UrlimagenAntigua);

                            string nombreArchivoAEliminar = Path.GetFileName(uri.AbsolutePath);
                            Console.WriteLine($"Nombre del archivo a eliminar: {nombreArchivoAEliminar}");
                            var rutaAEliminar = $"{carpeta}/{nombreArchivoAEliminar}";

                            bool eliminado = await EliminarArchivoSupabaseAsync(supabaseUrl, supabaseAnonKey, bucket, rutaAEliminar);

                            Console.WriteLine(eliminado ? "Archivo eliminado con éxito" : "No se pudo eliminar el archivo");
                        }
                        else
                        {
                            Console.WriteLine("La URL no contiene la ruta esperada");
                        }
                    }

                    perfil.ProfileImage = $"{supabaseUrl}/storage/v1/object/public/{bucket}/{rutaArchivo}";
                }

                perfil.Biography = Biografia;

                await Crud<Profile>.UpdateAsync(id, perfil);

                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return RedirectToAction(nameof(Index));
            }
        }

        private async Task<bool> EliminarArchivoSupabaseAsync(string supabaseUrl, string apiKey, string bucket, string rutaArchivo)
        {
            try
            {
                using (var client = new HttpClient())
                {
                    client.BaseAddress = new Uri(supabaseUrl);
                    client.DefaultRequestHeaders.Add("apikey", apiKey);
                    client.DefaultRequestHeaders.Add("Authorization", $"Bearer {apiKey}");

                    var response = await client.DeleteAsync($"/storage/v1/object/{bucket}/{rutaArchivo}");
                    return response.IsSuccessStatusCode;
                }
                
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error eliminando archivo: {ex.Message}");
                return false;
            }
        }

        // GET: PerfilController/Delete/5
        public ActionResult Delete(int id)
        {
            return View();
        }

        // POST: PerfilController/Delete/5
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
