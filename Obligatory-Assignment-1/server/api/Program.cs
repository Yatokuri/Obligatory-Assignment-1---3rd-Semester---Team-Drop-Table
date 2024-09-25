using System.Text.Json.Serialization;
using dataAccess;
using Microsoft.EntityFrameworkCore;
using service; // Assuming your services are defined here

var builder = WebApplication.CreateBuilder(args);

// Load configuration from .env file
var envFilePath = Path.Combine(Directory.GetCurrentDirectory(), "..", "..", "config.env");
if (File.Exists(envFilePath))
{
    var envVariables = File.ReadAllLines(envFilePath);
    foreach (var variable in envVariables)
    {
        if (variable.Contains('='))
        {
            var parts = variable.Split('=');
            if (parts.Length == 2)
            {
                Environment.SetEnvironmentVariable(parts[0], parts[1]);
            }
        }
    }
}

// Construct the PostgreSQL connection string using environment variables
var user = Environment.GetEnvironmentVariable("POSTGRES_USER");
var password = Environment.GetEnvironmentVariable("POSTGRES_PASSWORD");
var database = Environment.GetEnvironmentVariable("POSTGRES_DB");

var connectionString = $"Host=localhost;Database={database};Username={user};Password={password};";

// Register services
builder.Services.AddScoped<CustomerService>(); // Add CustomerService
builder.Services.AddScoped<OrderService>();    // Add OrderService
builder.Services.AddScoped<OrderEntryService>(); // Add OrderEntryService

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.Preserve;
    });


// Register the DbContext with PostgreSQL using the constructed connection string
builder.Services.AddDbContext<DMIContext>(options =>
{
    options.UseNpgsql(connectionString);
    options.EnableSensitiveDataLogging();
});

// Add OpenAPI documentation
builder.Services.AddOpenApiDocument(configure =>
{
    configure.Title = "Dunder Mifflin Infinity"; // Set your API title
    configure.Description = "Try and test"; // Set your API description
    configure.Version = "v1"; //test of my branch
});

builder.Services.AddSingleton<ProductService>();
builder.Services.AddCors();


var app = builder.Build();
app.UseOpenApi();
app.UseSwaggerUi();

app.MapControllers();

app.UseCors( opts => {

    opts.AllowAnyOrigin();

    opts.AllowAnyMethod();

    opts.AllowAnyHeader();

});


app.Run();
