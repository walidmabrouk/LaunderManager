using Infrastructure.Persistence;
using LaunderManagerWebApi.API.Middlewares;
using LaunderManagerWebApi.Application.Interfaces;
using LaunderManagerWebApi.Domain.InfrastructureServices;
using LaunderWebApi.Infrastructure.Dao;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // Services configuration
        ConfigureServices(builder.Services, builder.Configuration);

        var app = builder.Build();

        // Middleware configuration
        ConfigureMiddleware(app);

        app.Run();
    }

    private static void ConfigureServices(IServiceCollection services, IConfiguration configuration)
    {
        services.AddControllers();
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen();
        services.AddWebSocketMiddleware();

        // Configuration de la base de données
        string connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

        services.AddSingleton<DbConnectionManager>(sp =>
            new DbConnectionManager(
                connectionString,
                sp.GetRequiredService<ILogger<DbConnectionManager>>()
            )
        );

        // Services métier
        services.AddScoped<IProprietorRepository, ProprietorRepositoryImpl>();
        services.AddSingleton<IWebSocketService, WebSocketService>();
        services.AddScoped<IMachineRepository, MachineRepositoryImpl>();
        services.AddScoped<INotificationService, ManageNotificationUseCase>();
        services.AddScoped<IConfigurationService, ManageConfigurationUseCase>();
    }

    private static void ConfigureMiddleware(WebApplication app)
    {
        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }

        app.UseWebSockets(new WebSocketOptions
        {
            KeepAliveInterval = TimeSpan.FromMinutes(2)
        });
        app.UseWebSocketMiddleware();
        app.UseAuthorization();
        app.MapControllers();
    }
}