namespace Laundromat.Core.Interfaces
{
    public interface IWebSocketService
    {
        Task SendMessageAsync(string connectionId, string message);
        Task BroadcastMessageAsync(string message);
    }
}
