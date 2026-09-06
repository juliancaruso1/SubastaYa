using Microsoft.EntityFrameworkCore;
using SubastaYa.Api.Data;
using SubastaYa.Api.Hubs;
using SubastaYa.Api.Middleware;
using SubastaYa.Api.Services;
using SubastaYa.Api.Workers;

var builder = WebApplication.CreateBuilder(args);

// --- Base de datos (Code-First: el esquema se genera con migraciones) ---
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

// --- Servicios de dominio ---
builder.Services.AddScoped<IAuctionService, AuctionService>();
builder.Services.AddScoped<IWalletService, WalletService>();
builder.Services.AddScoped<IAuditService, AuditService>();

// --- Worker en segundo plano para adjudicar subastas vencidas ---
builder.Services.AddHostedService<AuctionClosingWorker>();

// --- SignalR para la Sala de Subasta en Vivo ---
builder.Services.AddSignalR();

// --- Controladores + Swagger/OpenAPI ---
builder.Services.AddControllers().AddJsonOptions(opts =>
{
    opts.JsonSerializerOptions.Converters.Add(new System.Text.Json.Serialization.JsonStringEnumConverter());
});
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo { Title = "SubastaYa API", Version = "v1" });
});

// --- CORS: habilitado para que el frontend React (Vite, puerto 5173) consuma la API ---
const string CorsPolicy = "FrontendPolicy";
builder.Services.AddCors(options =>
{
    options.AddPolicy(CorsPolicy, policy =>
    {
        policy.WithOrigins("http://localhost:5173", "http://127.0.0.1:5173")
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials(); // requerido por SignalR
    });
});

var app = builder.Build();

// --- Aplica migraciones automáticamente al arrancar (entorno de desarrollo/demo) ---
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.Migrate();
}

app.UseSwagger();
app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "SubastaYa API v1"));

app.UseMiddleware<ExceptionHandlingMiddleware>();

app.UseCors(CorsPolicy);

app.UseAuthorization();

app.MapControllers();
app.MapHub<AuctionHub>("/hubs/auctions");

app.Run();
