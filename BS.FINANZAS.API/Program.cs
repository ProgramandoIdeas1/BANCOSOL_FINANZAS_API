using BS.FINANZAS.Application.Interfaces;
using BS.FINANZAS.Application.Services;
using BS.FINANZAS.Domain.Interfaces;
using BS.FINANZAS.Infrastructure.Repositories;
using BS.FINANZAS.Infrastructure.Services;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

if (!string.IsNullOrEmpty(connectionString))
{
    builder.Services.AddScoped<IIngresoRepository, DapperIngresoRepository>();
}
else 
{
    builder.Services.AddSingleton<IIngresoRepository, InMemoryIngresoRepository>();
}

builder.Services.AddHttpClient<IHexaRateService, HexaRateService>();

builder.Services.AddScoped<IIngresoService, IngresoService>();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "BancoSol - API de Gestión Financiera Personal",
        Version = "v1",
        Description = "API REST Clean Architecture en .NET 8 con Dapper + PostgreSQL. (Para la gestión de ingresos y reportes financieros)",
        Contact = new OpenApiContact
        {
            Name = "Wilson Luque",
            Email = "wilfabwork1@gmail.com"
        }
    });
});

var app = builder.Build();

if (!string.IsNullOrEmpty(connectionString))
{
    try
    {
        using var scope = app.Services.CreateScope();
        var repo = scope.ServiceProvider.GetRequiredService<IIngresoRepository>() as DapperIngresoRepository;
        repo?.InicializarTablaAsync().GetAwaiter().GetResult();
    }
    catch (Exception ex)
    {
        app.Logger.LogWarning(ex, "No se pudo conectar a PostgreSQL. Continuando con la ejecucion...");
        Console.WriteLine($"Error al inicializar la tabla Ingresos: {ex.Message}");
    }
}

app.UseSwagger();

app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "BancoSol API v1");
    c.RoutePrefix = "swagger";
});

app.UseCors("AllowAll");

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();