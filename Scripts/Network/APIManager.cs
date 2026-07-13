using Godot;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Text.Json;
using System.Net.Http;
using System.Text.Json.Serialization;

public partial class APIManager : Node
{
    // --- TUS CREDENCIALES ---
    // Reemplazá esto con tu Project URL
    private const string SupabaseUrl = "https://obqrarrujluothjlzsjc.supabase.co"; 
    // Reemplazá esto con tu anon public o publishable key
    private const string SupabaseKey = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJpc3MiOiJzdXBhYmFzZSIsInJlZiI6Im9icXJhcnJ1amx1b3Roamx6c2pjIiwicm9sZSI6ImFub24iLCJpYXQiOjE3ODM3MDA5NzQsImV4cCI6MjA5OTI3Njk3NH0.AqzGjVqTOc-QItX6vee7IW2ZUVrONxIkDwpoaUT-nZg"; 

    public List<LevelData> DownloadedLevels { get; private set; } = new List<LevelData>();
    public bool IsDataLoaded { get; private set; } = false;

    // Instanciamos el cliente web
    // LÍNEA NUEVA CORREGIDA:
    private static readonly System.Net.Http.HttpClient client = new System.Net.Http.HttpClient();

    public override void _Ready()
    {
        GD.Print("APIManager iniciado. Listo para conectarse a Supabase.");
    }

    public async Task FetchLevelsFromServer()
    {
        GD.Print("Haciendo la petición GET a la base de datos...");
        IsDataLoaded = false;

        try
        {
            // 1. Armamos la URL. Le decimos: "Traé todo (*) de la tabla 'levels'"
            string requestUrl = $"{SupabaseUrl}/rest/v1/levels?select=*";
            
            using var request = new HttpRequestMessage(HttpMethod.Get, requestUrl);
            
            // 2. Le pegamos las credenciales de seguridad a la petición
            request.Headers.Add("apikey", SupabaseKey);
            request.Headers.Add("Authorization", $"Bearer {SupabaseKey}");

            // 3. Enviamos la solicitud y esperamos la respuesta del servidor
            var response = await client.SendAsync(request);
            response.EnsureSuccessStatusCode(); 

            // 4. Leemos el paquete JSON que nos llegó
            string jsonContent = await response.Content.ReadAsStringAsync();

            // 5. Transformamos el JSON crudo en nuestra lista de clases de C#
            DownloadedLevels = JsonSerializer.Deserialize<List<LevelData>>(jsonContent, new JsonSerializerOptions
            {
                // Esto permite que aunque en Supabase esté en minúsculas, C# lo entienda igual
                PropertyNameCaseInsensitive = true 
            });

            GD.Print($"Éxito: Se descargaron {DownloadedLevels.Count} niveles desde la nube.");
        }
        catch (System.Exception ex)
        {
            GD.PrintErr("Fallo de conexión: " + ex.Message);
            // Nivel de emergencia por si el jugador abre la app sin WiFi
            DownloadedLevels.Add(new LevelData("ERROR", new List<string> { "Sin", "Internet", "Reintente" }));
        }

        IsDataLoaded = true;
    }
}