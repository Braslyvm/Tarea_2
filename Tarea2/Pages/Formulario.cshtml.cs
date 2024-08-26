using Microsoft.Data.SqlClient; // Para trabajar con SQL Server
using System.Collections.Generic; // Para manejar colecciones como List
using System.Threading.Tasks; // Para programación asíncrona
using Microsoft.AspNetCore.Mvc; // Para manejar peticiones y respuestas en ASP.NET Core
using Microsoft.AspNetCore.Mvc.RazorPages; // Para trabajar con Razor Pages

namespace Tarea2.Pages
{
    public class FormularioModel : PageModel
    {
        // Cadena de conexión a la base de datos SQL Server
        private readonly string _connectionString = "Server=BRASLY\\BRASLYMSSQL;Database=usurario;User Id=sa;Password=brasly2004;TrustServerCertificate=True;";

        // Propiedades que se enlazarán con los datos del formulario
        [BindProperty]
        public int Cedula { get; set; }

        [BindProperty]
        public string Nombre { get; set; }

        [BindProperty]
        public string Apellido1 { get; set; }

        [BindProperty]
        public string Apellido2 { get; set; }

        // Lista para almacenar las personas recuperadas de la base de datos
        public List<dynamic> Personas { get; set; }

        // Método que se ejecuta cuando se carga la página por primera vez (GET request)
        public async Task OnGetAsync()
        {
            // Recuperar la lista de personas cuando se carga la página
            Personas = await GetPersonasAsync();
        }

        // Método que se ejecuta cuando se envía el formulario (POST request)
        public async Task<IActionResult> OnPostAsync()
        {
            // Verificar si los datos del formulario son válidos
            if (!ModelState.IsValid)
            {
                // Si hay errores, recargar la lista de personas y volver a la página
                Personas = await GetPersonasAsync();
                return Page();
            }

            // Consulta SQL para insertar una nueva persona en la base de datos
            string query = "INSERT INTO personas (Cedula, nombre, Apellido1, Apellido2) VALUES (@Cedula, @Nombre, @Apellido1, @Apellido2)";

            try
            {
                // Establecer una conexión con la base de datos
                using (SqlConnection connection = new SqlConnection(_connectionString))
                {
                    // Crear un comando SQL para ejecutar la consulta de inserción
                    SqlCommand command = new SqlCommand(query, connection);
                    // Asignar valores a los parámetros de la consulta
                    command.Parameters.AddWithValue("@Cedula", Cedula);
                    command.Parameters.AddWithValue("@Nombre", Nombre);
                    command.Parameters.AddWithValue("@Apellido1", Apellido1);
                    command.Parameters.AddWithValue("@Apellido2", Apellido2);

                    // Abrir la conexión de manera asíncrona
                    await connection.OpenAsync();
                    // Ejecutar la consulta de inserción de manera asíncrona
                    await command.ExecuteNonQueryAsync();
                }

                // Refrescar la lista de personas después de la inserción
                Personas = await GetPersonasAsync();
                return Page();
            }
            catch (SqlException ex)
            {
                // Manejar cualquier error de conexión o consulta SQL
                ModelState.AddModelError("", "No se pudo conectar a la base de datos: " + ex.Message);
                // Recargar la lista de personas en caso de error
                Personas = await GetPersonasAsync();
                return Page();
            }
        }

        // Método para recuperar la lista de personas desde la base de datos
        private async Task<List<dynamic>> GetPersonasAsync()
        {
            var personas = new List<dynamic>(); // Crear una lista para almacenar las personas

            // Consulta SQL para seleccionar las personas de la tabla
            string query = "SELECT Cedula, nombre, Apellido1, Apellido2 FROM personas";

            // Establecer una conexión con la base de datos
            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                // Crear un comando SQL para ejecutar la consulta
                SqlCommand command = new SqlCommand(query, connection);
                // Abrir la conexión de manera asíncrona
                await connection.OpenAsync();

                // Ejecutar la consulta y obtener un lector de datos de manera asíncrona
                using (var reader = await command.ExecuteReaderAsync())
                {
                    // Iterar sobre los registros retornados por la consulta
                    while (await reader.ReadAsync())
                    {
                        // Crear un objeto dinámico para representar cada persona
                        var persona = new
                        {
                            Cedula = reader["Cedula"],
                            Nombre = reader["nombre"],
                            Apellido1 = reader["Apellido1"],
                            Apellido2 = reader["Apellido2"]
                        };
                        // Agregar la persona a la lista
                        personas.Add(persona);
                    }
                }
            }

            return personas; // Retornar la lista de personas
        }
    }
}
