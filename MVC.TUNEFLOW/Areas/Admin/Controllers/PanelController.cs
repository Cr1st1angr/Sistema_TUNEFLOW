
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using API.Consumer;
using Modelos.Tuneflow.User.Consumer;
using Modelos.Tuneflow.Media;
using Modelos.Tuneflow.Payments;
using Modelos.Tuneflow.Playlists;
using Modelos.Tuneflow.User.Production;
using Modelos.Tuneflow.Models;

namespace MVC.TUNEFLOW.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "admin")]
    public class PanelController : Controller
    {
        public IActionResult Panel()
        {
            return View();
        }

        public async Task<IActionResult> Clientes()
        {
            Console.WriteLine("Inicio método Clientes");

            var clientes = await Crud<Modelos.Tuneflow.User.Consumer.Client>.GetAllAsync();
            if (clientes == null)
            {
                Console.WriteLine("clientes es null");
            }
            else
            {
                Console.WriteLine($"clientes.Count = {clientes.Count()}");
            }

            if (clientes == null || !clientes.Any())
            {
                Console.WriteLine("No hay clientes o lista vacía");
                ViewData["TotalClientes"] = 0;
                ViewBag.Data = new List<object>();
                return View();
            }

            var clientesValidos = clientes.Where(c => c.RegistrationDate > DateTime.MinValue).ToList();
            Console.WriteLine($"clientesValidos.Count = {clientesValidos.Count}");

            var clientesPorFecha = clientesValidos
                .GroupBy(c => c.RegistrationDate.Date)
                .Select(g =>
                {
                    Console.WriteLine($"Fecha: {g.Key}, Cantidad: {g.Count()}");
                    return new ChartDataModel
                    {
                        Fecha = g.Key,
                        Cantidad = g.Count()
                    };
                })
                .OrderBy(x => x.Fecha)
                .ToList();

            Console.WriteLine($"clientesPorFecha.Count = {clientesPorFecha.Count}");

            ViewData["TotalClientes"] = clientesValidos.Count();
            ViewBag.Data = clientesPorFecha;

            Console.WriteLine("Fin método Clientes");
            return View();
        }

        public async Task<IActionResult> Artistas()
        {
            Console.WriteLine("Inicio método Artistas");

            var artistas = await Crud<Artist>.GetAllAsync();
            if (artistas == null)
            {
                Console.WriteLine("artistas es null");
            }
            else
            {
                Console.WriteLine($"artistas.Count = {artistas.Count()}");
            }

            if (artistas == null || !artistas.Any())
            {
                Console.WriteLine("No hay artistas o lista vacía");
                ViewData["TotalArtistas"] = 0;
                ViewBag.Data = new List<object>(); 
                return View();
            }

            var artistasValidos = artistas.Where(a => a.RegistrationDate > DateTime.MinValue);
            Console.WriteLine($"artistasValidos.Count = {artistasValidos.Count()}");

            var artistasPorFecha = artistasValidos
                .GroupBy(a => a.RegistrationDate.Date)
                .Select(g =>
                {
                    Console.WriteLine($"Fecha: {g.Key}, Cantidad: {g.Count()}");
                    return new ChartDataModel
                    {
                        Fecha = g.Key,
                        Cantidad = g.Count()
                    };
                })
                .OrderBy(x => x.Fecha)
                .ToList();

            Console.WriteLine($"artistasPorFecha.Count = {artistasPorFecha.Count}");

            ViewData["TotalArtistas"] = artistasValidos.Count();
            ViewBag.Data = artistasPorFecha;

            Console.WriteLine("Fin método Artistas");
            return View();
        }

        public async Task<IActionResult> Canciones()
        {

            var canciones = await Crud<Song>.GetAllAsync();

            if (canciones == null || !canciones.Any())
            {
                Console.WriteLine("No hay canciones o lista vacía");
                ViewData["TotalCanciones"] = 0;
                ViewBag.Data = new List<object>();
                return View();
            }


            foreach (var s in canciones)
            {
                Console.WriteLine($"Song ID {s.Id} - ReleaseDate: {s.ReleaseDate} - Fecha como texto: {s.ReleaseDate.ToString("yyyy-MM-dd")}");
            }

            var cancionesPorFecha = canciones
                .Where(s => s.ReleaseDate.Year > 2000) 
                .GroupBy(s => s.ReleaseDate.ToString("yyyy-MM-dd"))
                .Select(g =>
                {
                    Console.WriteLine($"Fecha agrupada: {g.Key}, Total: {g.Count()}");
                    return new
                    {
                        Fecha = g.Key,
                        Cantidad = g.Count()
                    };
                })
                .OrderBy(x => x.Fecha)
                .ToList();

            Console.WriteLine($"cancionesPorFecha.Count = {cancionesPorFecha.Count}");

            ViewData["TotalCanciones"] = canciones.Count;
            ViewBag.Data = cancionesPorFecha;

            Console.WriteLine("Fin método Canciones");
            return View();
        }


        public async Task<IActionResult> Albums()
        {
            Console.WriteLine("Inicio método Albums");

            var albums = await Crud<Album>.GetAllAsync();
            if (albums == null)
            {
                Console.WriteLine("albums es null");
            }
            else
            {
                Console.WriteLine($"albums.Count = {albums.Count()}");
            }

            if (albums == null || !albums.Any())
            {
                Console.WriteLine("No hay albums o lista vacía");
                ViewData["TotalAlbums"] = 0;
                ViewBag.Data = new List<object>(); 
                return View();
            }

            var albumsValidos = albums.Where(a => a.ReleaseDate > DateTime.MinValue);
            Console.WriteLine($"albumsValidos.Count = {albumsValidos.Count()}");

            var albumsPorFecha = albumsValidos
                .GroupBy(a => a.ReleaseDate.Date)
                .Select(g =>
                {
                    Console.WriteLine($"Fecha: {g.Key}, Cantidad: {g.Count()}");
                    return new ChartDataModel
                    {
                        Fecha = g.Key,
                        Cantidad = g.Count()
                    };
                })
                .OrderBy(x => x.Fecha)
                .ToList();

            Console.WriteLine($"albumsPorFecha.Count = {albumsPorFecha.Count}");

            ViewData["TotalAlbums"] = albumsValidos.Count();
            ViewBag.Data = albumsPorFecha;

            Console.WriteLine("Fin método Albums");
            return View();
        }

        public async Task<IActionResult> Pagos()
        {
            Console.WriteLine("Inicio método Pagos");

            var pagos = await Crud<Payment>.GetAllAsync();
            if (pagos == null)
            {
                Console.WriteLine("pagos es null");
            }
            else
            {
                Console.WriteLine($"pagos.Count = {pagos.Count()}");
            }

            if (pagos == null || !pagos.Any())
            {
                Console.WriteLine("No hay pagos o lista vacía");
                ViewData["TotalPagos"] = 0;
                ViewBag.Data = new List<object>(); 
                return View();
            }

            var pagosValidos = pagos.Where(p => p.PaymentDate > DateTime.MinValue);
            Console.WriteLine($"pagosValidos.Count = {pagosValidos.Count()}");

            var pagosPorFecha = pagosValidos
                .GroupBy(p => p.PaymentDate.Date)
                .Select(g =>
                {
                    Console.WriteLine($"Fecha: {g.Key}, Cantidad: {g.Count()}");
                    return new ChartDataModel
                    {
                        Fecha = g.Key,
                        Cantidad = g.Count()
                    };
                })
                .OrderBy(x => x.Fecha)
                .ToList();

            Console.WriteLine($"pagosPorFecha.Count = {pagosPorFecha.Count}");

            ViewData["TotalPagos"] = pagosValidos.Count();
            ViewBag.Data = pagosPorFecha;

            Console.WriteLine("Fin método Pagos");
            return View();
        }
    }
}