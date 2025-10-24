using Npgsql;
using dotenv.net;
using task2Backend;
using CloudinaryDotNet;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

DotEnv.Load();
var databaseUrl = Environment.GetEnvironmentVariable("DATABASE_URL");

if (string.IsNullOrEmpty(databaseUrl))
    throw new Exception("DATABASE_URL not found");

// Converte o formato postgresql://user:pass@host:port/db -> Host=...;Port=...;Username=...;Password=...;Database=...
ConvertToConnectionString ConvertToConnectionString = new ConvertToConnectionString(databaseUrl);
var connectionString = ConvertToConnectionString.convert();
Console.WriteLine($"Converted connection string: {connectionString}");

var cloudinaryUrl = Environment.GetEnvironmentVariable("CLOUDINARY_URL");
if (string.IsNullOrEmpty(cloudinaryUrl))
    throw new Exception("CLOUDINARY_URL not found");

CloudinaryService cloudinary = new CloudinaryService(cloudinaryUrl);

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.MapGet("/test", () => "API está rodando!");

app.MapGet("/getproducts", async () =>
{
    using var conn = new NpgsqlConnection(connectionString);
    await conn.OpenAsync();

    using var SQL = new NpgsqlCommand("SELECT * FROM products", conn);
    using var reader = await SQL.ExecuteReaderAsync();

    var products = new List<GetProducts>();

    while (await reader.ReadAsync())
    {
        products.Add(new GetProducts
        {
            Id = reader.GetInt32(reader.GetOrdinal("id")),
            Name = reader.GetString(reader.GetOrdinal("name")),
            Desc = reader.GetString(reader.GetOrdinal("description")),
            Price = reader.GetString(reader.GetOrdinal("price")),
            ImgUrl = reader.GetString(reader.GetOrdinal("img_url")),
            CreatedAt = reader.GetDateTime(reader.GetOrdinal("created_at")).ToString()
        });
    }

    return Results.Ok(products);
});

app.MapPost("/postproducts", async (PostProducts product) =>
{
    if (string.IsNullOrWhiteSpace(product.Name))
    {
        return Results.BadRequest("Name is required");
    }
    else if (string.IsNullOrWhiteSpace(product.Desc))
    {
        return Results.BadRequest("Desc is required");
    }
    else if (string.IsNullOrWhiteSpace(product.Price))
    {
        return Results.BadRequest("Price is required");
    }
    else if (product.ImgUrl == null)
    {
        return Results.BadRequest("ImgUrl is required");
    }

    var imgUrl = await cloudinary.UploadImageAsync(product.ImgUrl, product.Name);
    Console.WriteLine(imgUrl);

    if (string.IsNullOrWhiteSpace(imgUrl))
    {
        return Results.StatusCode(500);
    }

    using var conn = new NpgsqlConnection(connectionString);
    await conn.OpenAsync();

    using var SQL = new NpgsqlCommand(
        "INSERT INTO products (name, description, price, img_url) VALUES (@name, @desc, @price, @imgUrl)",
        conn);
    
    SQL.Parameters.AddWithValue("@name", product.Name);
    SQL.Parameters.AddWithValue("@desc", product.Desc);
    SQL.Parameters.AddWithValue("@price", product.Price);
    SQL.Parameters.AddWithValue("@imgUrl", imgUrl);

    int rowsAffected = await SQL.ExecuteNonQueryAsync();
    Console.WriteLine($"Rows inserted: {rowsAffected}");

    return Results.Ok();
});

app.MapDelete("/deleteproducts", async (string id) =>
{
    Console.WriteLine(id);

    using var conn = new NpgsqlConnection(connectionString);
    await conn.OpenAsync();

    using var SQL = new NpgsqlCommand(
        "DELETE FROM products WHERE id = @id",
        conn);
    
    SQL.Parameters.AddWithValue("@id", id);

    int rowsAffected = await SQL.ExecuteNonQueryAsync(); 

    return Results.Ok();
});

app.Run();