using GestionUsuariosAPI.Data;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// ─── Servicios ───
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new()
    {
        Title = "Gestión de Usuarios y Roles API",
        Version = "v1",
        Description = "API REST para gestión de usuarios, roles y autenticación. " +
                      "Práctica 2 - Despliegue de Software",
        Contact = new()
        {
            Name = "Equipo de Desarrollo",
            Email = "equipo@universidad.edu"
        }
    });
});

// Base de datos en memoria
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseInMemoryDatabase("GestionUsuariosDb"));

var app = builder.Build();

// ─── Inicializar datos semilla ───
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    context.Database.EnsureCreated();
}

// ─── Middleware ───
// Swagger habilitado en todos los ambientes (necesario para la práctica)
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Gestión Usuarios API v1");
    c.RoutePrefix = "swagger";
    c.DocumentTitle = "Gestión de Usuarios - Swagger";
});

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

// Redirigir raíz a Swagger
app.MapGet("/", () => Results.Redirect("/swagger"));

app.Run();
