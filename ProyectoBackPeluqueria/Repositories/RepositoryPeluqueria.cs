using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using ProyectoBackPeluqueria.Data;
using NugetProyectoBackPeluqueria.Models;
using System.Data;

namespace ProyectoBackPeluqueria.Repositories
{
    public class RepositoryPeluqueria
    {
        private readonly AppDbContext _context;
        private readonly string _connectionString;

        public RepositoryPeluqueria(AppDbContext context, IConfiguration configuration)
        {
            _context = context;
            _connectionString = configuration.GetConnectionString("SqlPeluqueria");
        }

    public async Task<Usuario> LoginAsync(string email, string password)
        {
            var consulta = from Data in _context.Usuarios
                           where Data.Email == email && Data.Contrasena == password
                           select Data;
            return consulta.AsEnumerable().FirstOrDefault();
        }

        public async Task RegisterAsync(Usuario usuario)
        {
            _context.Usuarios.Add(usuario);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteUsuarioAsync(int id)
        {
            var usuario = await _context.Usuarios.FindAsync(id);
            _context.Usuarios.Remove(usuario);
            await _context.SaveChangesAsync();
        }

        // Obtener servicios
        public async Task<List<Servicio>> ObtenerServiciosAsync()
        {
            return await _context.Servicios.ToListAsync();
        }

        // Obtener disponibilidad por fecha
        public async Task<List<HorarioDisponible>> ObtenerDisponibilidadAsync(DateTime fecha)
        {
            return await _context.HorariosDisponibles
                .FromSqlRaw("EXEC ObtenerDisponibilidad @Fecha", new SqlParameter("@Fecha", fecha))
                .ToListAsync();
        }

        // Obtener días disponibles
        public async Task<List<DateTime>> ObtenerDiasDisponiblesAsync()
        {
            var dias = await _context.HorariosDisponibles
                .FromSqlRaw("EXEC ObtenerDiasDisponibles")
                .Select(h => h.Fecha)
                .Distinct()
                .ToListAsync();

            return dias;
        }

        // Insertar reserva
        public async Task InsertarReservaAsync(int clienteId, int servicioId, DateTime fechaHoraInicio)
        {
            await _context.Database.ExecuteSqlRawAsync(
                "EXEC InsertarReservaSimple @ClienteId, @ServicioId, @FechaHoraInicio",
                new SqlParameter("@ClienteId", clienteId),
                new SqlParameter("@ServicioId", servicioId),
                new SqlParameter("@FechaHoraInicio", fechaHoraInicio)
            );
        }

        public async Task<Usuario> FindUsuario(int usuarioId)
        {
            return await _context.Usuarios.FindAsync(usuarioId);
        }

        public async Task<Usuario> GetUsuarioByEmailAsync(string email)
        {
            return await _context.Usuarios.FirstOrDefaultAsync(u => u.Email == email);
        }

        public async Task UpdateUsuarioAsync(Usuario usuario)
        {
            _context.Usuarios.Update(usuario);
            await _context.SaveChangesAsync();
        }

        // Realizar compra
        public async Task RealizarCompraAsync(int clienteId, DataTable detallesCompra)
        {
            var paramCliente = new SqlParameter("@ClienteId", clienteId);
            var paramDetalles = new SqlParameter("@DetallesCompra", detallesCompra)
            {
                SqlDbType = SqlDbType.Structured,
                TypeName = "Tipo_CompraDetalles"
            };

            await _context.Database.ExecuteSqlRawAsync(
                "EXEC RealizarCompra @ClienteId, @DetallesCompra",
                paramCliente, paramDetalles
            );
        }

        // Obtener horarios disponibles por servicio y fecha
        public async Task<List<HorarioDisponible>> ObtenerHorariosDisponiblesPorFechaAsync(int servicioId, DateTime fecha)
        {
            return await _context.HorariosDisponibles
                .FromSqlRaw("EXEC ObtenerHorariosDisponiblesPorFecha @ServicioId, @Fecha",
                    new SqlParameter("@ServicioId", servicioId),
                    new SqlParameter("@Fecha", fecha))
                .ToListAsync();
        }

        // Obtener todos los días y horas disponibles por servicio
        public async Task<List<HorarioDisponible>> ObtenerDiasYHorasDisponiblesAsync(int servicioId)
        {
            return await _context.HorariosDisponibles
                .FromSqlRaw("EXEC ObtenerDiasYHorasDisponibles @ServicioId",
                    new SqlParameter("@ServicioId", servicioId))
                .ToListAsync();
        }

        public async Task<List<ReservaView>> ObtenerReservasClientesAsync()
        {
            return await _context.VistaReservas
                .FromSqlRaw("SELECT * FROM Vista_Reservas ORDER BY FechaHoraInicio DESC")
                .ToListAsync();
        }

        public async Task<ReservaView> FindReservaAsync(int reservaId)
        {
            return await _context.VistaReservas.FindAsync(reservaId);
        }


        public async Task<(int diasAgregados, int diasExistentes)> AgregarDisponibilidadRangoAsync(DateTime fechaInicio, DateTime fechaFin)
        {
            int diasAgregados = 0;
            int diasExistentes = 0;

            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();

                for (var fecha = fechaInicio; fecha <= fechaFin; fecha = fecha.AddDays(1))
                {
                    // Saltar sábados y domingos
                    if (fecha.DayOfWeek == DayOfWeek.Saturday || fecha.DayOfWeek == DayOfWeek.Sunday)
                        continue;

                    var command = new SqlCommand("AgregarDisponibilidad", connection)
                    {
                        CommandType = CommandType.StoredProcedure
                    };

                    command.Parameters.AddWithValue("@Fecha", fecha);

                    try
                    {
                        await command.ExecuteNonQueryAsync();
                        diasAgregados++;
                    }
                    catch (SqlException ex)
                    {
                        if (ex.Number == 50000) // RAISERROR personalizado desde el procedimiento
                            diasExistentes++;
                        else
                            throw; // Otros errores
                    }
                }
            }

            return (diasAgregados, diasExistentes);
        }


        public async Task<List<DateTime>> ObtenerDiasDisponibles()
        {
            var fechas = new List<DateTime>();
            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                using (var command = new SqlCommand("ObtenerDiasDisponibles", connection){
                        CommandType = CommandType.StoredProcedure
                    })
                {
                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            fechas.Add(DateTime.Parse(reader["Fecha"].ToString()));
                        }
                    }
                }
            }
            return fechas;
        }

        public async Task<List<(DateTime FechaInicio, DateTime FechaFin, string Servicio, int ReservaId)>> ObtenerCitasConHoras()
        {
            var citas = new List<(DateTime FechaInicio, DateTime FechaFin, string Servicio, int ReservaId)>();

            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                using (var command = new SqlCommand("SELECT FechaHoraInicio, FechaHoraFin, Servicio, ReservaID FROM Vista_Reservas", connection))
                {
                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            // Añadir la fecha de inicio, fecha de fin y servicio como una tupla a la lista
                            citas.Add((
                                reader.GetDateTime(0), // FechaHoraInicio
                                reader.GetDateTime(1), // FechaHoraFin
                                reader.GetString(2),   // Servicio
                                reader.GetInt32(3)    // ReservaID
                            ));
                        }
                    }
                }
            }
            return citas;
        }



        public async Task<List<Servicio>> GetServiciosAsync()
        {
            return await _context.Servicios.ToListAsync();
        }
        public async Task InsertarServicioAsync(Servicio servicio)
        {
            _context.Servicios.Add(servicio);
            await _context.SaveChangesAsync();
        }
        public async Task ActualizarServicioAsync(Servicio servicio)
        {
            _context.Servicios.Update(servicio);
            await _context.SaveChangesAsync();
        }
        public async Task EliminarServicioAsync(int id)
        {
            var servicio = await FindServicioAsync(id);
            _context.Servicios.Remove(servicio);
            await _context.SaveChangesAsync();
        }
        public async Task<Servicio> FindServicioAsync(int id)
        {
            return await _context.Servicios.FindAsync(id);
        }




        public async Task<UsuarioView> FindUsuarioView(int usuarioId)
        {
            return await _context.VistaUsuarios.FindAsync(usuarioId);
        }

        public async Task<List<Usuario>> GetClientesAsync()
        {
            return await _context.Usuarios.Where(u => u.IdRolUsuario == 1).ToListAsync();
        }

        public async Task<List<Reserva>> GetReservasUsuarioAsync(int usuarioId)
        {
            return await _context.Reservas
                .Where(r => r.ClienteId == usuarioId)
                .ToListAsync();
        }

        public async Task<Reserva> GetProximaReservaUsuarioAsync(int usuarioId)
        {
            return await _context.Reservas
                .Where(r => r.ClienteId == usuarioId && r.FechaHoraInicio >= DateTime.Now)
                .OrderBy(r => r.FechaHoraInicio)
                .FirstOrDefaultAsync();
        }


        public async Task<string> GetServicioReservaAsync(int servicioId)
        {
            Servicio servicio = await _context.Servicios.FindAsync(servicioId);
            return servicio.Nombre;
        }
        public async Task<Reserva> GetLastReservaUsuarioAsync(int usuarioId)
        {
            return await _context.Reservas
                .Where(r => r.ClienteId == usuarioId)
                .OrderByDescending(r => r.FechaHoraInicio)
                .FirstOrDefaultAsync();
        }

        public async Task<List<Compra>> GetComprasUsuarioAsync(int usuarioId)
        {
            return await _context.Compras
                .Where(c => c.ClienteId == usuarioId)
                .ToListAsync();
        }

        public async Task<List<CompraDetalle>> GetDetallesUltimaCompraAsync(int usuarioId)
        {
            var ultimaCompra = await _context.Compras
                .Where(c => c.ClienteId == usuarioId)
                .OrderByDescending(c => c.Fecha)
                .FirstOrDefaultAsync();

            if (ultimaCompra == null)
                return null;

            return await _context.CompraDetalles
                .Where(cd => cd.CompraId == ultimaCompra.Id)
                .ToListAsync();
        }

        public async Task EliminarReservaAsync(int reservaId)
        {
            var reserva = await _context.Reservas.FindAsync(reservaId);
            _context.Reservas.Remove(reserva);
            await _context.SaveChangesAsync();
        }

    }
}
