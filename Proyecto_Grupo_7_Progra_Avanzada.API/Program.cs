using Microsoft.EntityFrameworkCore;
using Proyecto_Grupo_7_Progra_Avanzada.Data;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();

// DbContext (misma conexión que el proyecto principal)
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseMySql(
        builder.Configuration.GetConnectionString("MySqlConnection"),
        new MySqlServerVersion(new Version(8, 0, 36))
    )
);

// OJO: aquí ya NO usamos AddEndpointsApiExplorer ni AddSwaggerGen

var app = builder.Build();

// Aquí tampoco usamos Swagger
// if (app.Environment.IsDevelopment())
// {
//     app.UseSwagger();
//     app.UseSwaggerUI();
// }

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
