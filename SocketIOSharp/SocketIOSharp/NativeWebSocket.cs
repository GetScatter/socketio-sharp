using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using WebSocketSharp;

namespace SocketIOSharp
{
    public class NativeWebSocket : IWebSocket
    {
        private WebSocket Socket;
        private Queue<byte[]> Messages = new Queue<byte[]>();
        private bool IsConnected = false;
        private string Error = null;

        public NativeWebSocket()
        {
        }

        public async Task ConnectAsync(Uri url)
        {
            string protocol = url.Scheme;
            if (!protocol.Equals("ws") && !protocol.Equals("wss"))
                throw new ArgumentException("Unsupported protocol: " + protocol);

            Socket = new WebSocket(url.ToString());

            Socket.OnMessage += (sender, e) => Messages.Enqueue(e.RawData);
            Socket.OnOpen += (sender, e) => IsConnected = true;
            Socket.OnError += (sender, e) => Error = e.Message;

            Socket.ConnectAsync();

            while (!IsConnected && Error == null)
                await Task.Yield();
        }

        public Task SendAsync(byte[] data)
        {
            return Task.Run(() => { Socket.Send(data); });
        }

        public Task SendAsync(string data)
        {
            return Task.Run(() => { Socket.Send(data); });
        }

        public Task<byte[]> ReceiveAsync()
        {
            return Task.Run(() => {
                if (Messages.Count == 0)
                    return null;
                return Messages.Dequeue();
            });
        }

        public Task CloseAsync()
        {
            return Task.Run(() => { Socket.Close(); });
        }

        public string GetError()
        {
            return Error;
        }

        public WebSocketState GetState()
        {
            return Socket != null ? Socket.ReadyState : WebSocketState.Closed;
        }

        public void Dispose()
        {
            Socket.Close(1001);
        }
    }
}