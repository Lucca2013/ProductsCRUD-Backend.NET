using Npgsql;
using dotenv.net;
using task2Backend;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll",
        builder =>
        {
            builder.AllowAnyOrigin()
                   .AllowAnyMethod()
                   .AllowAnyHeader();
        });
});

builder.Services.AddAuthorization();

AppContext.SetSwitch("System.Net.DisableIPv6", true);

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

var app = builder.Build();

app.UseRouting();
app.UseCors("AllowAll");
app.UseAuthorization();

app.UseHttpsRedirection();


if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapGet("/test", () => "API está rodando!");

app.MapGet("/getproducts", async () =>
{
    try
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
    }
    catch (Exception err)
    {
        System.Console.WriteLine("err: " + err);
        return Results.Conflict("erro, entrou em catch");
    }
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

app.MapDelete("/deleteproducts", async (int id) =>
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
}).RequireCors("AllowAll");

app.MapPatch("/updateproducts", async (PatchProducts product) =>
{
    if (!product.Id.HasValue)
    {
        return Results.BadRequest("ID is required");
    }
    else if (string.IsNullOrWhiteSpace(product.Name))
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

    using var SQL = new NpgsqlCommand("""
        UPDATE products
        SET name = @name,
        description = @description,
        price = @price,
        img_url = @imgUrl
        WHERE id = @id;
    """, conn);

    SQL.Parameters.AddWithValue("@id", product.Id);
    SQL.Parameters.AddWithValue("@name", product.Name);
    SQL.Parameters.AddWithValue("@description", product.Desc);
    SQL.Parameters.AddWithValue("@price", product.Price);
    SQL.Parameters.AddWithValue("@imgUrl", imgUrl);

    using var reader = await SQL.ExecuteReaderAsync();
    Console.WriteLine($"Rows updated: {reader}");

    return Results.Ok();

}).RequireCors("AllowAll");

app.MapPut("/updatepartsofproducts", async (PutProducts product) =>
{
    if (product.Id == null)
    {
        return Results.BadRequest("ID is required");
    }

    if (!string.IsNullOrWhiteSpace(product.Name))
    {
        using var conn = new NpgsqlConnection(connectionString);
        await conn.OpenAsync();

        using var SQL = new NpgsqlCommand("""
            UPDATE products
            SET name = @name
            WHERE id = @id;
        """, conn);

        SQL.Parameters.AddWithValue("@id", product.Id);
        SQL.Parameters.AddWithValue("@name", product.Name);

        using var reader = await SQL.ExecuteReaderAsync();
        Console.WriteLine($"Rows updated: {reader}");
    }

    if (!string.IsNullOrWhiteSpace(product.Desc))
    {
        using var conn = new NpgsqlConnection(connectionString);
        await conn.OpenAsync();

        using var SQL = new NpgsqlCommand("""
            UPDATE products
            SET description = @description
            WHERE id = @id;
        """, conn);

        SQL.Parameters.AddWithValue("@id", product.Id);
        SQL.Parameters.AddWithValue("@description", product.Desc);

        using var reader = await SQL.ExecuteReaderAsync();
        Console.WriteLine($"Rows updated: {reader}");
    }

    if (!string.IsNullOrWhiteSpace(product.Price))
    {
        using var conn = new NpgsqlConnection(connectionString);
        await conn.OpenAsync();

        using var SQL = new NpgsqlCommand("""
            UPDATE products
            SET price = @price
            WHERE id = @id;
        """, conn);

        SQL.Parameters.AddWithValue("@id", product.Id);
        SQL.Parameters.AddWithValue("@price", product.Price);

        using var reader = await SQL.ExecuteReaderAsync();
        Console.WriteLine($"Rows updated: {reader}");
    }

    if (product.ImgUrl != null)
    {
        var imgUrl = await cloudinary.UploadImageAsync(product.ImgUrl, product.Id.ToString());
        Console.WriteLine(imgUrl);

        if (string.IsNullOrWhiteSpace(imgUrl))
        {
            return Results.StatusCode(500);
        }

        using var conn = new NpgsqlConnection(connectionString);
        await conn.OpenAsync();

        using var SQL = new NpgsqlCommand("""
            UPDATE products
            SET img_url = @imgUrl
            WHERE id = @id;
        """, conn);

        SQL.Parameters.AddWithValue("@id", product.Id);
        SQL.Parameters.AddWithValue("@imgUrl", product.ImgUrl);

        using var reader = await SQL.ExecuteReaderAsync();
        Console.WriteLine($"Rows updated: {reader}");
    }

    return Results.Ok();
}).RequireCors("AllowAll");

app.Run();