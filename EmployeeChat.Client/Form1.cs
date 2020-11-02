using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Newtonsoft.Json;
using Shared;
using Shared.Models;

namespace EmployeeChat.Client
{
    public partial class Form1 : Form
    {
        private ClientWebSocket _socket = new ClientWebSocket();
        private Uri _connectionUri = new Uri("ws://localhost:5000/chat");
        private string _token;
        private bool _connectionEstablished => !string.IsNullOrEmpty(_token);
        private Thread _workingThread;

        public Form1()
        {
            InitializeComponent();
        }

        private async void Form1_Load(object sender, EventArgs e)
        {
            Thread.Sleep(1000);
            await Connect();
        }

        private async Task Connect()
        {
            if (_socket == null || _socket.State != WebSocketState.Open)
            {
                _socket?.Dispose();
                _socket = new ClientWebSocket();
            }

            await _socket.ConnectAsync(_connectionUri, CancellationToken.None);
            _workingThread = new Thread(Listen);
            _workingThread.Start();
            button1.Enabled = true;
        }

        private async void button1_Click(object sender, EventArgs e)
        {
            if (_socket == null || _socket.State != WebSocketState.Open)
                await Connect();
            while (!_connectionEstablished)
                Thread.Sleep(10);
            await SendAsync(new LoginModel {Name = textBox1.Text, Status = textBox2.Text, Token = _token});
            groupBox1.Visible = false;
            groupBox2.Visible = true;
        }

        private async void Listen()
        {
            while (_socket.State == WebSocketState.Open)
            {
                ArraySegment<byte> buffer = new ArraySegment<byte>(new byte[1024 * 4]);
                string raw;
                WebSocketReceiveResult result;
                using (var ms = new MemoryStream())
                {
                    do
                    {
                        result = await _socket.ReceiveAsync(buffer, CancellationToken.None)
                            ;
                        ms.Write(buffer.Array, buffer.Offset, result.Count);
                    } while (!result.EndOfMessage);

                    ms.Seek(0, SeekOrigin.Begin);

                    using (var reader = new StreamReader(ms, Encoding.UTF8))
                    {
                        raw = await reader.ReadToEndAsync().ConfigureAwait(false);
                    }
                }

                if (result.MessageType == WebSocketMessageType.Text)
                {
                    var message = JsonConvert.DeserializeObject<Header>(raw);
                    switch (message.Target)
                    {
                        case InvocationTarget.ChatMessage:
                            Invoke(new Action(() =>
                            {
                                richTextBox1.Text +=
                                    DateTime.Now.ToString("T") + " " + ConvertToModel<ChatMessageModel>(raw)
                                        .Content + Environment.NewLine;
                            }));
                            break;
                        case InvocationTarget.StateHasChanged:
                            Invoke(new Action(() =>
                            {
                                listBox1.Items.Clear();
                                ConvertToModel<LobbyStateModel>(raw).Members
                                    .ForEach(member => listBox1.Items.Add(member.MemberName));
                            }));

                            break;
                        case InvocationTarget.SetToken:
                            _token = ConvertToModel<SetTokenModel>(raw).Token;
                            break;
                    }
                }
            }
        }

        private T ConvertToModel<T>(string raw) => JsonConvert.DeserializeObject<T>(raw);

        public Task SendAsync<T>(T model)
        {
            var bytes = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(model));
            return _socket.SendAsync(new ArraySegment<byte>(bytes), WebSocketMessageType.Text, true,
                CancellationToken.None);
        }

        private async void button2_Click(object sender, EventArgs e)
        {
            await CloseConnection();
            listBox1.Items.Clear();
            groupBox2.Visible = false;
            groupBox1.Visible = true;
            richTextBox1.Text = string.Empty;
        }

        protected override async void OnClosing(CancelEventArgs e)
        {
            await CloseConnection();
            base.OnClosing(e);
        }

        private async Task CloseConnection()
        {
            if (_socket != null)
            {
                await _socket.CloseAsync(WebSocketCloseStatus.NormalClosure, "", CancellationToken.None);
                _socket?.Dispose();
            }

            _token = string.Empty;
        }

        private async void button3_Click(object sender, EventArgs e)
        {
            await SendAsync(new ChatMessageModel {Token = _token, Content = textBox3.Text});
            textBox3.Text = string.Empty;
        }
    }
}