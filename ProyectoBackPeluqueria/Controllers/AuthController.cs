using System.Drawing;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using ProyectoBackPeluqueria.Repositories;
using System.Drawing.Imaging;
using System.Text.RegularExpressions;
using ProyectoBackPeluqueria.Services;
using NugetProyectoBackPeluqueria.Models;
using Azure.Storage.Files.Shares;

namespace ProyectoBackPeluqueria.Controllers
{
    public class AuthController : Controller
    {
        public ServicePeluqueria _service;
        public ServiceStorageBlobs _serviceBlobs;

        public AuthController(ServicePeluqueria service, ServiceStorageBlobs serviceStorageBlobs)
        {
            _service = service;
            _serviceBlobs = serviceStorageBlobs;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(string email, string password)
        {
            var result = await _service.LoginAsync(email, password);

            if (string.IsNullOrEmpty(result.token) || result.user == null)
            {
                ViewData["Error"] = "Credenciales incorrectas";
                return View();
            }

            var usuario = result.user;
            string token = result.token;

            // Crear claims para la identidad
            var claims = new List<Claim>
    {
        new Claim(ClaimTypes.Name, usuario.Nombre ?? ""),
        new Claim(ClaimTypes.NameIdentifier, usuario.Id.ToString()),
        new Claim("Imagen", usuario.Imagen ?? ""),
        new Claim(ClaimTypes.Role, usuario.IdRolUsuario == 2 ? "Admin" : "User"),
        new Claim("Token", token) // Guardar el token en los claims (opcional)
    };

            // Crear la identidad y el principal
            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var claimsPrincipal = new ClaimsPrincipal(identity);

            // Iniciar sesión con autenticación por cookies
            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, claimsPrincipal,
                new AuthenticationProperties
                {
                    IsPersistent = true, // Mantener la sesión activa
                    ExpiresUtc = DateTime.UtcNow.AddHours(1) // Expira en 1 hora
                });

            return RedirectToAction("Index", "Home"); // Redirigir después del login
        }





        public async Task<IActionResult> Register()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Register(Usuario usuario, string adminCode)
        {

            // Asignar rol
            usuario.IdRolUsuario = (!string.IsNullOrEmpty(adminCode) && adminCode == "20Santervas") ? 2 : 1;

            // Encriptar la contraseña
            usuario.Contrasena = BCrypt.Net.BCrypt.HashPassword(usuario.Contrasena);

            // Generar avatar
            string iniciales = GetIniciales(usuario.Nombre + " " + usuario.Apellidos);

            byte[] imagenAvatar = GenerarAvatar(iniciales, usuario.ColorAvatar);

            string nombreAvatar = $"{Guid.NewGuid()}.png";

            await _serviceBlobs.UploadBlobAsync("avatars", nombreAvatar, new MemoryStream(imagenAvatar));

            usuario.Imagen = nombreAvatar;

            bool registrado = await _service.RegisterAsync(usuario);

            if (!registrado)
            {
                return View(usuario);
            }

            return RedirectToAction("Login");
        }





        public IActionResult Denied()
        {
            return View();
        }



        public static string GetIniciales(string nombre)
        {
            // Paso 1
            string[] palabras = nombre.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            string iniciales = "";

            // Este bucle recorre cada palabra del array
            foreach (var palabra in palabras)
            {
                // Paso 2
                iniciales += char.ToUpper(palabra[0]);

                // Paso 3
                if (iniciales.Length == 2) break;
            }

            return iniciales;
        }

        public static byte[] GenerarAvatar(string iniciales, string colorHex)
        {
            // Dimensiones de la imagen
            int ancho = 150, alto = 150;

            // Paso 1
            Bitmap bitmap = new Bitmap(ancho, alto);

            // Paso 2
            using (Graphics g = Graphics.FromImage(bitmap))
            {
                // Paso 3
                Color color = ColorTranslator.FromHtml(colorHex);

                // Paso 4
                g.Clear(color);

                // Paso 5
                Font fuente = new Font("Arial", 50, FontStyle.Bold);

                // Paso 6
                Brush textoBlanco = Brushes.White;

                // Paso 7
                SizeF tamano = g.MeasureString(iniciales, fuente);

                // Paso 8
                float x = (ancho - tamano.Width) / 2;
                float y = (alto - tamano.Height) / 2;

                // Paso 9
                g.DrawString(iniciales, fuente, textoBlanco, x, y);
            }

            // Paso 10
            using MemoryStream ms = new MemoryStream();

            // Paso 11
            bitmap.Save(ms, ImageFormat.Png);

            // Paso 12
            return ms.ToArray();
        }
    }
}
