using Azure.Storage.Blobs.Models;
using Microsoft.AspNetCore.Authentication;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NugetProyectoBackPeluqueria.Models;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace ProyectoBackPeluqueria.Services
{
    public class ServicePeluqueria
    {
        private string UrlApi;
        private MediaTypeWithQualityHeaderValue Header;
        private IHttpContextAccessor HttpContextAccessor;

        public ServicePeluqueria(IConfiguration configuration, IHttpContextAccessor httpContextAccessor)
        {
            this.UrlApi = configuration.GetValue<string>
            ("ApiUrls:ApiProyectoBackPeluqueria");
            this.Header = new
            MediaTypeWithQualityHeaderValue("application/json");
            HttpContextAccessor = httpContextAccessor;
        }


        public string GetToken()
        {
            // Verifica si el token existe en los claims
            var token = HttpContextAccessor.HttpContext.User.FindFirst(x => x.Type == "Token")?.Value;

            // Si el token es null o vacío, puedes imprimirlo para depuración
            Console.WriteLine($"Token: {token}");

            return token;
        }



        public async Task<T> CallApiAsync<T>(string request, HttpMethod method, object data = null)
        {
            using (HttpClient client = new HttpClient())
            {
                client.BaseAddress = new Uri(UrlApi);
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(Header);

                // Obtener el token y añadirlo a los headers
                string token = GetToken();
                if (!string.IsNullOrEmpty(token))
                {
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                }

                HttpRequestMessage httpRequest = new HttpRequestMessage(method, request);

                if (data != null)
                {
                    string json = JsonConvert.SerializeObject(data);
                    httpRequest.Content = new StringContent(json, Encoding.UTF8, "application/json");
                }

                HttpResponseMessage response = await client.SendAsync(httpRequest);

                // Imprimir el resultado de la respuesta para depuración
                Console.WriteLine($"Response: {response.StatusCode}");

                if (response.IsSuccessStatusCode)
                {
                    return await response.Content.ReadAsAsync<T>();
                }
                else
                {
                    return default(T);
                }
            }
        }


        public async Task<(string token, Usuario user)> LoginAsync(string email, string password)
        {
            using (HttpClient client = new HttpClient())
            {
                string request = "api/auth/login";
                client.BaseAddress = new Uri(this.UrlApi);
                client.DefaultRequestHeaders.Clear();
                client.DefaultRequestHeaders.Accept.Add(this.Header);

                LoginModel model = new LoginModel
                {
                    Email = email,
                    Password = password
                };

                string json = JsonConvert.SerializeObject(model);
                StringContent content = new StringContent(json, Encoding.UTF8, "application/json");

                HttpResponseMessage response = await client.PostAsync(request, content);

                if (response.IsSuccessStatusCode)
                {
                    string data = await response.Content.ReadAsStringAsync();
                    JObject keys = JObject.Parse(data);

                    string token = keys.GetValue("authToken")?.ToString();
                    Usuario usuario = keys["user"]?.ToObject<Usuario>();

                    return (token, usuario);
                }
                else
                {
                    return (null, null);
                }
            }
        }


        public async Task<bool> RegisterAsync(Usuario usuario)
        {
            using (HttpClient client = new HttpClient())
            {
                string request = "api/auth/register"; // Ruta de la API
                client.BaseAddress = new Uri(this.UrlApi);
                client.DefaultRequestHeaders.Clear();
                client.DefaultRequestHeaders.Accept.Add(this.Header);

                string json = JsonConvert.SerializeObject(usuario);
                StringContent content = new StringContent(json, Encoding.UTF8, "application/json");

                HttpResponseMessage response = await client.PostAsync(request, content);

                return response.IsSuccessStatusCode; // Devuelve `true` si se registró correctamente
            }
        }

        public async Task<bool> UpdateUsuarioAsync(Usuario usuario)
        {
            var response = await CallApiAsync<bool>("api/management/UpdateUsuario", HttpMethod.Put, usuario);
            return response;
        }

        public async Task<Usuario> FindUsuario(int id)
        {
            var response = await CallApiAsync<Usuario>($"api/management/GetPerfil/{id}", HttpMethod.Get);
            return response;
        }





        public async Task InsertarReservaAsync(int clienteId, int servicioId, string fechaHoraInicio)
        {
            var reserva = new ReservaModel
            {
                ClienteId = clienteId,
                ServicioId = servicioId,
                FechaHoraInicio = fechaHoraInicio
            };
            var response = await CallApiAsync<bool>("api/reservas/InsertarReserva", HttpMethod.Post, reserva);
        }
        public async Task DeleteReserva(int id)
        {
            var response = await CallApiAsync<bool>($"api/reservas/DeleteReserva/{id}", HttpMethod.Delete);
        }
        public async Task<Reserva> FindReservaAsync(int id)
        {
            var response = await CallApiAsync<Reserva>($"api/reservas/FindReserva/{id}", HttpMethod.Get);
            return response;
        }

        public async Task<Reserva> GetProximaReservaUsuarioAsync(int id)
        {
            var response = await CallApiAsync<Reserva>($"api/reservas/GetProximaReservaUsuario/{id}", HttpMethod.Get);
            return response;
        }

        public async Task<string> GetServicioReservaAsync(int id)
        {
            var response = await CallApiAsync<string>($"api/reservas/GetServicioReserva/{id}", HttpMethod.Get);
            return response;
        }

        public async Task<DisponibilidadResponse> AgregarDisponibilidadRangoAsync(DateTime fechaInicio, DateTime fechaFin)
        {
            AgregarDisponibilidadModel body = new AgregarDisponibilidadModel
            {
                FechaInicio = fechaInicio,
                FechaFin = fechaFin
            };
            var response = await CallApiAsync<DisponibilidadResponse>($"api/reservas/AgregarDisponibilidadRango", HttpMethod.Post, body);
            return response;
        }

        public async Task<List<(DateTime FechaInicio, DateTime FechaFin, string Servicio, int ReservaId)>> ObtenerCitasConHoras()
        {
            var response = await CallApiAsync<List<(DateTime FechaInicio, DateTime FechaFin, string Servicio, int ReservaId)>>("api/reservas/ObtenerCitasConHoras", HttpMethod.Get);
            return response;
        }

        public async Task<List<DateTime>> ObtenerDiasDisponibles()
        {
            var response = await CallApiAsync<List<DateTime>>("api/reservas/ObtenerDiasDisponibles", HttpMethod.Get);
            return response;
        }

        public async Task<List<ReservaView>> ObtenerReservasClientesAsync()
        {
            var response = await CallApiAsync<List<ReservaView>>("api/reservas/ObtenerReservasClientes", HttpMethod.Get);
            return response;
        }

        public async Task<List<HorarioDisponible>> ObtenerHorariosDisponiblesPorFechaAsync(int servicioId, string fecha)
        {
            var response = await CallApiAsync<List<HorarioDisponible>>($"api/reservas/ObtenerHorariosDisponiblesPorFecha/{servicioId}", HttpMethod.Post, fecha);
            return response;
        }






        public async Task<List<Usuario>> GetClientesAsync()
        {
            var response = await CallApiAsync<List<Usuario>>("api/clientes/GetClientes", HttpMethod.Get);
            return response;
        }

        public async Task DeleteUsuarioAsync(int id)
        {
            var response = await CallApiAsync<bool>($"api/clientes/DeleteUsuario/{id}", HttpMethod.Delete);
        }



        public async Task<List<Servicio>> GetServiciosAsync()
        {
            var response = await CallApiAsync<List<Servicio>>("api/servicios/GetServicios", HttpMethod.Get);
            return response;
        }
        public async Task<Servicio> FindServicioAsync(int id)
        {
            var response = await CallApiAsync<Servicio>($"api/servicios/FindServicio/{id}", HttpMethod.Get);
            return response;
        }
        public async Task InsertarServicioAsync(Servicio servicio)
        {
            var response = await CallApiAsync<bool>("api/servicios/InsertarServicio", HttpMethod.Post, servicio);
        }
        public async Task UpdateServicioAsync(Servicio servicio)
        {
            var response = await CallApiAsync<bool>("api/servicios/UpdateServicio", HttpMethod.Put, servicio);
        }
        public async Task DeleteServicioAsync(int id)
        {
            var response = await CallApiAsync<bool>($"api/servicios/DeleteServicio/{id}", HttpMethod.Delete);
        }
    }
}
