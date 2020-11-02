using System.Net.WebSockets;
using System.Threading.Tasks;

namespace EmployeeChat
{
    public interface ISocketHandler
    {
        Task ClientConnected(WebSocket client);
        Task ClientDisconnected(WebSocket client);
    }
}