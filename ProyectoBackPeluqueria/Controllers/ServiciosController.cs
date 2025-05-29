using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProyectoBackPeluqueria.Filters;
using NugetProyectoBackPeluqueria.Models;
using ProyectoBackPeluqueria.Repositories;
using ProyectoBackPeluqueria.Services;

namespace ProyectoBackPeluqueria.Controllers
{
    public class ServiciosController : Controller
    {
        ServicePeluqueria _service;

        public ServiciosController(ServicePeluqueria service)
        {
            _service = service;
        }

        [AuthorizeUsers]
        public async Task<IActionResult> Index()
        {
            List<Servicio> servicios = await this._service.GetServiciosAsync();
            return View(servicios);
        }

        [AuthorizeUsers]
        public async Task<IActionResult> Create()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(Servicio servicio)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    // Verificar y corregir el precio en caso de problemas con la cultura
                    var precioTexto = Request.Form["Precio"].ToString().Replace(",", ".");
                    servicio.Precio = decimal.Parse(precioTexto, System.Globalization.CultureInfo.InvariantCulture);

                    await this._service.InsertarServicioAsync(servicio);
                    return RedirectToAction("Index");
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", "Error al guardar el servicio.");
                }
            }
            return View();
        }

        [AuthorizeUsers]
        public async Task<IActionResult> Edit(int id)
        {
            Servicio servicio = await this._service.FindServicioAsync(id);
            return View(servicio);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(Servicio servicio)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    // Verificar y corregir el precio en caso de problemas con la cultura
                    var precioTexto = Request.Form["Precio"].ToString().Replace(",", ".");
                    servicio.Precio = decimal.Parse(precioTexto, System.Globalization.CultureInfo.InvariantCulture);

                    await this._service.UpdateServicioAsync(servicio);
                    return RedirectToAction("Index");
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", "Error al guardar el servicio.");
                }
            }
            return View(servicio);
        }


        public async Task<IActionResult> Delete(int id)
        {
            await this._service.DeleteServicioAsync(id);
            return RedirectToAction("Index");
        }
    }
}
