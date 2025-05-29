using Microsoft.AspNetCore.Mvc;
using NugetProyectoBackPeluqueria.Models;
using ProyectoBackPeluqueria.Repositories;
using ProyectoBackPeluqueria.Services;

namespace ProyectoBackPeluqueria.Controllers
{
    public class ClientesController : Controller
    {
        ServicePeluqueria _service;
        ServiceStorageBlobs _serviceBlobs;

        public ClientesController(ServicePeluqueria service, ServiceStorageBlobs serviceStorageBlobs)
        {
            _service = service;
            _serviceBlobs = serviceStorageBlobs;
        }

        public async Task<IActionResult> Index()
        {
            List<Usuario> clientes = await _service.GetClientesAsync();
            return View(clientes);
        }

        public async Task<IActionResult> Create()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(Usuario usuario, string adminCode)
        {
            if (!string.IsNullOrEmpty(adminCode) && adminCode == "Taj@mar365")
            {
                usuario.IdRolUsuario = 2; // Administrador
            }
            else
            {
                usuario.IdRolUsuario = 1; // Usuario normal
            }

            usuario.Contrasena = BCrypt.Net.BCrypt.HashPassword(usuario.Contrasena);

            string iniciales = AuthController.GetIniciales(usuario.Nombre + " " + usuario.Apellidos);

            byte[] imagenAvatar = AuthController.GenerarAvatar(iniciales, usuario.ColorAvatar);

            string nombreAvatar = $"{Guid.NewGuid()}.png";

            await _serviceBlobs.UploadBlobAsync("avatars", nombreAvatar, new MemoryStream(imagenAvatar));

            usuario.Imagen = nombreAvatar;

            await _service.RegisterAsync(usuario);
            return RedirectToAction("Index");
        }

        public async Task<IActionResult> Edit(int id)
        {
            Usuario usuario = await _service.FindUsuario(id);
            return View(usuario);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(Usuario usuario)
        {
            await _service.UpdateUsuarioAsync(usuario);
            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            await this._service.DeleteUsuarioAsync(id);
            return Ok();
        }
    }
}
