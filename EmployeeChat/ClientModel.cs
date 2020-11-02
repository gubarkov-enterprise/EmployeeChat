using System.Net.WebSockets;

namespace EmployeeChat
{
    public class ClientModel
    {
        public string Token { get; set; }
        public string Name { get; set; }
        public string Status { get; set; }
        public bool IsActive { get; set; }
        public WebSocket Socket { get; set; }
    }
}