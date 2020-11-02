using System;
using System.IO;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using Shared;
using Shared.Models;

namespace EmployeeChat.Middleware
{
    public class WebSocketMiddleware
    {
        private readonly ISocketHandler _executionService;

        public WebSocketMiddleware(RequestDelegate next,
            ISocketHandler executionService)
        {
            _executionService = executionService;
        }


        public async Task Invoke(HttpContext context)
        {
            if (!context.WebSockets.IsWebSocketRequest)
                return;

            var socket = await context.WebSockets.AcceptWebSocketAsync();
            await _executionService.ClientConnected(socket);

            await ExtractData(socket, async (result, raw, header) =>
            {
                switch (result.MessageType)
                {
                    case WebSocketMessageType.Text:
                        _executionService
                            .GetType()
                            .GetMethod(header.Target.ToString())?
                            .Invoke(_executionService, new[] {raw});
                        return;
                    case WebSocketMessageType.Close:
                        await _executionService.ClientDisconnected(socket);
                        return;
                }
            });
        }

        private async Task ExtractData(WebSocket socket, Action<WebSocketReceiveResult, string, Header> handleMessage)
        {
            var buffer = new ArraySegment<byte>(new Byte[1024 * 4]);

            while (socket.State == WebSocketState.Open)
            {
                WebSocketReceiveResult result;
                Header header;
                string raw;
                await using (var ms = new MemoryStream())
                {
                    do
                    {
                        result = await socket.ReceiveAsync(buffer, CancellationToken.None).ConfigureAwait(false);
                        ms.Write(buffer.Array, buffer.Offset, result.Count);
                    } while (!result.EndOfMessage);

                    ms.Seek(0, SeekOrigin.Begin);

                    using (var reader = new StreamReader(ms, Encoding.UTF8))
                    {
                        raw = await reader.ReadToEndAsync().ConfigureAwait(false);
                        header = JsonConvert.DeserializeObject<Header>(raw);
                    }
                }

                handleMessage(result, raw, header);
            }
        }
    }
}