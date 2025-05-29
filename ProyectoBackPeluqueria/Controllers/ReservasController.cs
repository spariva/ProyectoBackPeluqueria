using Microsoft.AspNetCore.Mvc;
using NugetProyectoBackPeluqueria.Models;
using ProyectoBackPeluqueria.Repositories;
using System.Net.Mail;
using System.Net;
using System.Runtime.Intrinsics.X86;
using ProyectoBackPeluqueria.Filters;
using ProyectoBackPeluqueria.Services;
using Azure.Communication.Email;
using Azure;

namespace ProyectoBackPeluqueria.Controllers
{
    public class ReservasController : Controller
    {

        private ServicePeluqueria _service;
        private IConfiguration _configuration;

        public ReservasController(ServicePeluqueria service, IConfiguration configuration)
        {
            _service = service;
            _configuration = configuration;
        }

        [AuthorizeUsers]
        public async Task<IActionResult> Index()
        {
            var reservas = await _service.ObtenerReservasClientesAsync();
            return View(reservas);
        }


        [AuthorizeUsers]
        public async Task<IActionResult> Create()
        {
            var clientes = await _service.GetClientesAsync();
            var servicios = await _service.GetServiciosAsync();
            ViewData["Clientes"] = clientes;
            return View(servicios);
        }


        // Obtener los horarios disponibles por servicio y fecha
        [HttpGet]
        public async Task<IActionResult> ObtenerHorariosDisponibles(int servicioId, string fecha)
        {
             // Convertir el string de fecha a DateTime
            var horarios = await _service.ObtenerHorariosDisponiblesPorFechaAsync(servicioId, fecha);

            var horariosDisponibles = horarios.Where(h => h.Disponible).Select(h => new
            {
                id = h.Id,
                horaInicio = h.HoraInicio.ToString(@"hh\:mm")
            }).ToList();

            return Json(horariosDisponibles);
        }

       

[HttpPost]
    public async Task<IActionResult> InsertarReserva(int clienteId, int servicioId, string fechaHoraInicio)
    {
        await _service.InsertarReservaAsync(clienteId, servicioId, fechaHoraInicio);

        Usuario cliente = await _service.FindUsuario(clienteId);
        Servicio servicio = await _service.FindServicioAsync(servicioId);

            var fechaHora = DateTime.Parse(fechaHoraInicio);
            await SendEmailAsync(cliente, servicio, fechaHora);

        return Ok();
    }

    public async Task SendEmailAsync(Usuario cliente, Servicio servicio, DateTime fechaHora)
    {
        string user = _configuration.GetValue<string>("MailSettings:SenderEmail");
        string password = _configuration.GetValue<string>("MailSettings:AppPassword");
        string host = _configuration.GetValue<string>("MailSettings:Host");
        int port = _configuration.GetValue<int>("MailSettings:Port");
        bool ssl = bool.Parse(_configuration.GetValue<string>("MailSettings:EnableSSL"));

            //// Crear el mensaje
            //MailMessage email = new MailMessage();
            //email.From = new MailAddress(user);
            //email.To.Add(cliente.Email);
            //email.Subject = "Reserva realizada";
            //email.Body = $@"
            //    <!DOCTYPE html>
            //    <html lang='es'>
            //    <head>
            //        <meta charset='UTF-8'>
            //        <meta name='viewport' content='width=device-width, initial-scale=1.0'>
            //        <title>Reserva realizada</title>
            //        <style>
            //            body {{
            //                font-family: Arial, sans-serif;
            //                background-color: #f4f4f9;
            //                margin: 0;
            //                padding: 0;
            //            }}
            //            .container {{
            //                max-width: 600px;
            //                margin: 20px auto;
            //                padding: 20px;
            //                background-color: #ffffff;
            //                border-radius: 8px;
            //                box-shadow: 0 4px 8px rgba(0, 0, 0, 0.1);
            //            }}
            //            .header {{
            //                text-align: center;
            //                background-color: #dc8a3e;
            //                color: white;
            //                padding: 10px 0;
            //                border-radius: 8px 8px 0 0;
            //            }}
            //            .header h1 {{
            //                margin: 0;
            //            }}
            //            .content {{
            //                margin-top: 20px;
            //                font-size: 16px;
            //            }}
            //            .content p {{
            //                line-height: 1.5;
            //            }}
            //            .content strong {{
            //                color: #dc8a3e;
            //            }}
            //            .footer {{
            //                margin-top: 20px;
            //                text-align: center;
            //                font-size: 14px;
            //                color: #777;
            //            }}
            //            .button {{
            //                background-color: #dc8a3e;
            //                padding: 10px 15px;
            //                text-align: center;
            //                display: inline-block;
            //                border-radius: 5px;
            //                text-decoration: none;
            //                margin-top: 20px;
            //            }}
            //            .button:hover {{
            //                background-color: #c68a54;
            //            }}
            //            a{{
            //                color: white;
            //                text-decoration: none;
            //            }}
            //        </style>
            //    </head>
            //    <body>
            //        <div class='container'>
            //            <div class='header'>
            //                <h1>Iris De Vicent: {servicio.Nombre}</h1>
            //            </div>
            //            <div class='content'>
            //                <p>Estimado/a <strong>{cliente.Nombre}</strong>,</p>
            //                <p>Se ha realizado con éxito una reserva para el servicio de <strong>{servicio.Nombre.ToLower()}</strong>.</p>
            //                <p>Nos vemos el día <strong>{fechaHora:dd-MM-yyyy}</strong> a las <strong>{fechaHora:HH:mm}</strong>.</p>
            //                <p>Estamos emocionados de recibirte en nuestra peluquería, <strong>Iris De Vicent</strong>, en Madrid. ¡Gracias por elegirnos!</p>

            //                <p>Para más información o si necesitas indicaciones, puedes ver nuestra ubicación en Google Maps haciendo clic en el siguiente enlace:</p>
            //                <a href='https://maps.app.goo.gl/fAFkqDdcBxLE8iyZ8' class='button'>Ver en Google Maps</a>
            //            </div>
            //            <div class='footer'>
            //                <p>¡Nos vemos pronto!</p>
            //                <p><strong>Iris D'Vicent</strong></p>
            //                <p>Madrid, España</p>
            //            </div>
            //        </div>
            //    </body>
            //    </html>";

            //email.IsBodyHtml = true;
            //email.Priority = MailPriority.Normal;

            //SmtpClient client = new SmtpClient();
            //client.Host = host;
            //client.Port = port;
            //client.EnableSsl = ssl;
            //client.UseDefaultCredentials = false;
            //NetworkCredential credentials = new NetworkCredential(user, password);
            //client.Credentials = credentials;
            //await client.SendMailAsync(email);

            // Variables necesarias (ajústalas según lo que uses)
            string connectionString = _configuration.GetValue<string>("AzureUrls:EmailAccount");
            var emailClient = new EmailClient(connectionString);

            // Aquí defines tu HTML como un string interpolado si quieres usar variables dinámicas
            string htmlBody = $@"
<!DOCTYPE html>
<html lang='es'>
<head>
    <meta charset='UTF-8'>
    <meta name='viewport' content='width=device-width, initial-scale=1.0'>
    <title>Reserva realizada</title>
    <style>
        body {{
            font-family: Arial, sans-serif;
            background-color: #f4f4f9;
            margin: 0;
            padding: 0;
        }}
        .container {{
            max-width: 600px;
            margin: 20px auto;
            padding: 20px;
            background-color: #ffffff;
            border-radius: 8px;
            box-shadow: 0 4px 8px rgba(0, 0, 0, 0.1);
        }}
        .header {{
            text-align: center;
            background-color: #dc8a3e;
            color: white;
            padding: 10px 0;
            border-radius: 8px 8px 0 0;
        }}
        .header h1 {{
            margin: 0;
        }}
        .content {{
            margin-top: 20px;
            font-size: 16px;
        }}
        .content p {{
            line-height: 1.5;
        }}
        .content strong {{
            color: #dc8a3e;
        }}
        .footer {{
            margin-top: 20px;
            text-align: center;
            font-size: 14px;
            color: #777;
        }}
        .button {{
            background-color: #dc8a3e;
            padding: 10px 15px;
            text-align: center;
            display: inline-block;
            border-radius: 5px;
            text-decoration: none;
            margin-top: 20px;
        }}
        .button:hover {{
            background-color: #c68a54;
        }}
        a{{
            color: white;
            text-decoration: none;
        }}
    </style>
</head>
<body>
    <div class='container'>
        <div class='header'>
            <h1>Iris De Vicent: {servicio.Nombre}</h1>
        </div>
        <div class='content'>
            <p>Estimado/a <strong>{cliente.Nombre}</strong>,</p>
            <p>Se ha realizado con éxito una reserva para el servicio de <strong>{servicio.Nombre.ToLower()}</strong>.</p>
            <p>Nos vemos el día <strong>{fechaHora:dd-MM-yyyy}</strong> a las <strong>{fechaHora:HH:mm}</strong>.</p>
            <p>Estamos emocionados de recibirte en nuestra peluquería, <strong>Iris De Vicent</strong>, en Madrid. ¡Gracias por elegirnos!</p>
            <p>Para más información o si necesitas indicaciones, puedes ver nuestra ubicación en Google Maps haciendo clic en el siguiente enlace:</p>
            <a href='https://maps.app.goo.gl/fAFkqDdcBxLE8iyZ8' class='button'>Ver en Google Maps</a>
        </div>
        <div class='footer'>
            <p>¡Nos vemos pronto!</p>
            <p><strong>Iris D'Vicent</strong></p>
            <p>Madrid, España</p>
        </div>
    </div>
</body>
</html>";

            var emailMessage = new EmailMessage(
    senderAddress: "DoNotReply@7723826b-c890-4bce-930c-9e8049167dcd.azurecomm.net",
    content: new EmailContent("Reserva realizada")
    {
        PlainText = "Tu reserva ha sido realizada con éxito. Consulta el correo en formato HTML para más detalles.",
        Html = htmlBody
    },
    recipients: new EmailRecipients(new List<EmailAddress>
    {
        new EmailAddress("castrilloredondovictor@gmail.com")
    }));

            EmailSendOperation emailSendOperation = emailClient.Send(
                WaitUntil.Completed,
                emailMessage);
        }

        public async Task<IActionResult> Delete(int id)
        {
            await this._service.DeleteReserva(id);
            return RedirectToAction("Index");
        }

    }
}
