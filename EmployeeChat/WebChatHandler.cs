using System;
using System.Linq;
using System.Net.NetworkInformation;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Shared;
using Shared.Models;

namespace EmployeeChat
{
    public class WebChatHandler : WebSocketHandler
    {
        public WebChatHandler(ClientStorage storage) : base(storage)
        {
        }

        public Task Authorization(string raw)
        {
            var model = JsonConvert.DeserializeObject<LoginModel>(raw);
            var client = Storage.GetById(model.Token);
            client.Name = model.Name;
            client.Status = model.Status;
            client.IsActive = true;
            SendMessageToAll(new ChatMessageModel {Content = $"{client.Name} has joined the chat"});
            return StateHasChanged();
        }

        public Task ChatMessage(string raw)
        {
            var msg = JsonConvert.DeserializeObject<ChatMessageModel>(raw);
            var sender = Storage.GetById(msg.Token);
            return SendMessageToAll(new ChatMessageModel
                {Content = $"{sender.Name}:{msg.Content}"});
        }

        protected Task StateHasChanged()
        {
            return SendMessageToAll(new LobbyStateModel
            {
                Members = Storage
                    .Where(client => client.IsActive)
                    .Select(client => new ChatMember
                    {
                        MemberName = client.Name, MemberStatus = client.Status
                    }).ToList()
            });
        }

        protected override Task OnConnected(string token) => SendMessage(token, new SetTokenModel {Token = token});

        protected override Task OnDisconnected() => StateHasChanged();
    }
}