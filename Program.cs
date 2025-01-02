using Application.Interfaces;
using Application.Services;
using Domain.Repositories;
using Infrastructure.Persistence;
using LaunderManagerWebApi.API.Middlewares;
using LaunderManagerWebApi.Domain.Services.InfrastructureServices;
using LaunderWebApi.Infrastructure.Dao;
using Laundromat.Application.UseCases;
using Laundromat.Core.Interfaces;

var builder = WebApplication.CreateBuilder(args);

// Ajouter les services au conteneur
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddWebSocketMiddleware();

// Ajouter la configuration pour récupérer la chaîne de connexion depuis appsettings.json
string connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

// Enregistrer DbConnectionManager avec la chaîne de connexion
builder.Services.AddSingleton(new DbConnectionManager(connectionString));

// Enregistrer les services existants
builder.Services.AddScoped<IDaoProprietor, ProprietorDao>();
builder.Services.AddScoped<IWebSocketService, WebSocketService>();
builder.Services.AddScoped<UploadInitialConfigurationUseCase>();
builder.Services.AddScoped<IMachineRepository, MachineRepository>();
builder.Services.AddScoped<IMachineService, MachineService>();


var app = builder.Build();

// Configurer le pipeline HTTP
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}


app.UseWebSockets();
app.UseWebSocketMiddleware();
app.UseAuthorization();
app.MapControllers();

app.Run();
