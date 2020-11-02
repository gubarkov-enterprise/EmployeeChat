using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using MoreLinq;
using Newtonsoft.Json;
using Shared.Models;

namespace EmployeeChat
{
    public abstract class WebSocketHandler : ISocketHandler
    {
        protected ClientStorage Storage { get; }

        protected WebSocketHandler(ClientStorage storage)
        {
            Storage = storage;
        }

        protected abstract Task OnConnected(string token);
        protected abstract Task OnDisconnected();


        public async Task ClientConnected(WebSocket client)
        {
            var token = Storage.AddClient(client);
            await OnConnected(token);
        }

        public async Task ClientDisconnected(WebSocket client)
        {
            await SendMessageToAll(new ChatMessageModel
                {Content = $"{Storage.GetById(Storage.GetId(client)).Name} has leave"});

            await Storage.RemoveClient(Storage.GetId(client));
            await OnDisconnected();
        }


        protected Task SendMessage<T>(string token, T message)
        {
            var client = Storage.GetById(token);
            var bytes = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(message));
            if (client.Socket.State == WebSocketState.Open)
                return client.Socket.SendAsync(new ArraySegment<byte>(bytes), WebSocketMessageType.Text, true,
                    CancellationToken.None);
            return Task.CompletedTask;
        }

        protected Task SendMessageToAll<T>(T message)
        {
            var taskMessages = Storage.Where(model => model.IsActive)
                .Select(client => SendMessage(client.Token, message)).ToArray();
            Task.WaitAll(taskMessages);
            return Task.CompletedTask;
        }
    }
}