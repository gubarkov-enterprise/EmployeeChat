using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;

namespace EmployeeChat
{
    public class ClientStorage : IEnumerable<ClientModel>
    {
        private readonly ConcurrentDictionary<string, ClientModel> _clients =
            new ConcurrentDictionary<string, ClientModel>();

        public string AddClient(WebSocket socket)
        {
            var token = Guid.NewGuid().ToString();
            _clients.TryAdd(token, new ClientModel {Socket = socket, Token = token});
            return token;
        }

        public async Task RemoveClient(string id)
        {
            _clients.TryRemove(id, out var clientModel);
            if (clientModel != null)
                await clientModel.Socket.CloseAsync(WebSocketCloseStatus.NormalClosure,
                    string.Empty,
                    CancellationToken.None);
        }

        public string GetId(WebSocket socket) =>
            _clients.FirstOrDefault(client => client.Value.Socket == socket).Key;

        public ClientModel GetById(string token) => _clients[token];

        public IEnumerator<ClientModel> GetEnumerator() => _clients.Select(client => client.Value).GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}