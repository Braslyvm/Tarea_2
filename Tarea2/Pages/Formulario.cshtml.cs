using Microsoft.Data.SqlClient;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Tarea2.Pages
{
    public class FormularioModel : PageModel
    {
        private readonly string _connectionString = "Server=BRASLY\\BRASLYMSSQL;Database=usurario;User Id=sa;Password=brasly2004;TrustServerCertificate=True;";


        [BindProperty]
        public int Cedula { get; set; }

        [BindProperty]
        public string Nombre { get; set; }

        [BindProperty]
        public string Apellido1 { get; set; }

        [BindProperty]
        public string Apellido2 { get; set; }

        public List<dynamic> Personas { get; set; }

        public async Task OnGetAsync()
        {
            // Recuperar la lista de personas cuando se carga la página
            Personas = await GetPersonasAsync();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                // Volver a cargar los datos si hay errores de validación
                Personas = await GetPersonasAsync();
                return Page();
            }

            string query = "INSERT INTO personas (Cedula, nombre, Apellido1, Apellido2) VALUES (@Cedula, @Nombre, @Apellido1, @Apellido2)";

            try
            {
                using (SqlConnection connection = new SqlConnection(_connectionString))
                {
                    SqlCommand command = new SqlCommand(query, connection);
                    command.Parameters.AddWithValue("@Cedula", Cedula);
                    command.Parameters.AddWithValue("@Nombre", Nombre);
                    command.Parameters.AddWithValue("@Apellido1", Apellido1);
                    command.Parameters.AddWithValue("@Apellido2", Apellido2);

                    await connection.OpenAsync();
                    await command.ExecuteNonQueryAsync();
                }

                // Refrescar la lista después de la inserción
                Personas = await GetPersonasAsync();
                return Page();
            }
            catch (SqlException ex)
            {
                ModelState.AddModelError("", "No se pudo conectar a la base de datos: " + ex.Message);
                Personas = await GetPersonasAsync();
                return Page();
            }
        }

        private async Task<List<dynamic>> GetPersonasAsync()
        {
            var personas = new List<dynamic>();

            string query = "SELECT Cedula, nombre, Apellido1, Apellido2 FROM personas";

            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                SqlCommand command = new SqlCommand(query, connection);
                await connection.OpenAsync();

                using (var reader = await command.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        var persona = new
                        {
                            Cedula = reader["Cedula"],
                            Nombre = reader["nombre"],
                            Apellido1 = reader["Apellido1"],
                            Apellido2 = reader["Apellido2"]
                        };
                        personas.Add(persona);
                    }
                }
            }

            return personas;
        }
    }
}
