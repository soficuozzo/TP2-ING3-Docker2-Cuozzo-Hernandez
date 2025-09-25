using MySqlConnector;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
var app = builder.Build();

app.UseAuthorization();
app.MapControllers();

// -------- MySQL endpoints --------
// GET /db/ping  -> abre conexión
app.MapGet("/db/ping", async (IConfiguration cfg) =>
{
    var cs = cfg.GetConnectionString("Default");
    await using var con = new MySqlConnection(cs);
    await con.OpenAsync();
    return Results.Ok(new { ok = true });
});

// POST /db/init -> crea tabla si no existe
app.MapPost("/db/init", async (IConfiguration cfg) =>
{
    var cs = cfg.GetConnectionString("Default");
    const string sql = @"
        CREATE TABLE IF NOT EXISTS weather (
            id INT AUTO_INCREMENT PRIMARY KEY,
            date DATE NOT NULL,
            temp_c INT NOT NULL,
            summary VARCHAR(100) NOT NULL
        );";
    await using var con = new MySqlConnection(cs);
    await con.OpenAsync();
    await using var cmd = new MySqlCommand(sql, con);
    await cmd.ExecuteNonQueryAsync();
    return Results.Ok(new { created = true });
});

// POST /db/seed -> inserta 5 filas
app.MapPost("/db/seed", async (IConfiguration cfg) =>
{
    var cs = cfg.GetConnectionString("Default");
    const string insert = "INSERT INTO weather(date, temp_c, summary) VALUES (@d, @t, @s)";
    var summaries = new[] { "Helado", "Fresquito", "Fresco", "Cálido", "Calorón" };
    var rnd = new Random();

    await using var con = new MySqlConnection(cs);
    await con.OpenAsync();

    for (int i = 1; i <= 5; i++)
    {
        await using var cmd = new MySqlCommand(insert, con);
        cmd.Parameters.AddWithValue("@d", DateTime.UtcNow.AddDays(i).Date);
        cmd.Parameters.AddWithValue("@t", rnd.Next(-10, 40));
        cmd.Parameters.AddWithValue("@s", summaries[rnd.Next(summaries.Length)]);
        await cmd.ExecuteNonQueryAsync();
    }
    return Results.Ok(new { seeded = 5 });
});

// GET /db/all -> lista filas
app.MapGet("/db/all", async (IConfiguration cfg) =>
{
    var cs = cfg.GetConnectionString("Default");
    const string q = "SELECT id, date, temp_c, summary FROM weather ORDER BY id";
    var list = new List<object>();

    await using var con = new MySqlConnection(cs);
    await con.OpenAsync();
    await using var cmd = new MySqlCommand(q, con);
    await using var rd = await cmd.ExecuteReaderAsync();
    while (await rd.ReadAsync())
    {
        list.Add(new
        {
            id = rd.GetInt32("id"),
            date = rd.GetDateTime("date"),
            temp_c = rd.GetInt32("temp_c"),
            summary = rd.GetString("summary")
        });
    }
    return Results.Ok(list);
});

app.Run();
