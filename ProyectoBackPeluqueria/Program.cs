using ProyectoBackPeluqueria.Data;
using ProyectoBackPeluqueria.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.Cookies;
using ProyectoBackPeluqueria.Services;
using Azure.Storage.Blobs;
using Microsoft.Extensions.Configuration;
using NugetProyectoBackPeluqueria.Models;
using Azure.Security.KeyVault.Secrets;
using Microsoft.Extensions.Azure;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddAzureClients(factory =>
{
    factory.AddSecretClient(builder.Configuration.GetSection("KeyVault"));
});

SecretClient secretClient = builder.Services.BuildServiceProvider()
                                            .GetRequiredService<SecretClient>();

KeyVaultSecret secretConnectionString = await secretClient.GetSecretAsync("sqlpeluqueria");
KeyVaultSecret secretStorageAccount = await secretClient.GetSecretAsync("storageaccount");
//test

BlobServiceClient blobServiceClient = new BlobServiceClient(secretStorageAccount.Value);

builder.Services.AddTransient<BlobServiceClient>(sp => blobServiceClient);

// Add services to the container.
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(60);  // Tiempo de expiración de la sesión
});

builder.Services.AddAuthentication(
    options =>
    {
        options.DefaultSignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
        options.DefaultAuthenticateScheme = CookieAuthenticationDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = CookieAuthenticationDefaults.AuthenticationScheme;
    }
).AddCookie(options =>
{
    // Puedes agregar algunas configuraciones de cookies si es necesario
    options.LoginPath = "/Auth/Login";  // Ruta donde se redirige a Login
    options.AccessDeniedPath = "/Auth/AccessDenied";  // Ruta para acceso denegado
});

builder.Services.AddControllersWithViews();  // Configuración de controladores y vistas

builder.Services.AddHttpContextAccessor();  // Habilita el acceso al contexto HTTP

builder.Services.AddTransient<RepositoryPeluqueria>();
builder.Services.AddTransient<ServicePeluqueria>();
builder.Services.AddTransient<ServiceStorageBlobs>();

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(secretConnectionString.Value));

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();

app.UseAuthentication();  // Asegúrate de que la autenticación esté configurada antes de la autorización
app.UseAuthorization();   // Asegúrate de que la autorización esté configurada después de la autenticación

app.UseStaticFiles();  // Importante: habilita el uso de archivos estáticos (CSS, JS, imágenes)

app.UseSession();  // Asegúrate de que las sesiones estén habilitadas

// Configuración de las rutas de los controladores
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

// Inicia la aplicación
app.Run();
