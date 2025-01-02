using Microsoft.Extensions.DependencyInjection;

namespace LaunderManagerWebApi.API.Middlewares
{
    public static class WebSocketMiddlewareExtensions
    {
        public static IServiceCollection AddWebSocketMiddleware(this IServiceCollection services)
        {
            services.AddSingleton<WebSocketService>();
            return services;
        }

        public static IApplicationBuilder UseWebSocketMiddleware(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<WebSocketServerMiddleware>();
        }
    }
}