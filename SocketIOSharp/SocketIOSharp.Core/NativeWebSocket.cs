using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using WebSocketSharp;

namespace SocketIOSharp.Core
{
    public class NativeWebSocket : IWebSocket
    {
        private WebSocket Socket;
        private Proxy Proxy;
        private Queue<byte[]> Messages = new Queue<byte[]>();
        private bool IsConnected = false;
        private string Error = null;

        public NativeWebSocket()
        {
        }

        public NativeWebSocket(Proxy proxy)
        {
            Proxy = proxy;
        }

        public async Task ConnectAsync(Uri url)
        {
            string protocol = url.Scheme;
            if (!protocol.Equals("ws") && !protocol.Equals("wss"))
                throw new ArgumentException("Unsupported protocol: " + protocol);

            Socket = new WebSocket(url.ToString());

            if (Proxy != null)
                Socket.SetProxy(Proxy.Url, Proxy.UserName, Proxy.Password);

            Socket.OnMessage += (sender, e) => Messages.Enqueue(e.RawData);
            Socket.OnOpen += (sender, e) => IsConnected = true;
            Socket.OnError += (sender, e) => Error = e.Message;

            Socket.ConnectAsync();

            while (!IsConnected && Error == null)
                await Task.Yield();
        }

        public Task SendAsync(byte[] buffer)
        {
            return Task.Run(() => { Socket.Send(buffer); });
        }

        public Task SendAsync(string data)
        {
            return Task.Run(() => { Socket.Send(data); });
        }

        public Task<byte[]> ReceiveAsync()
        {
            if (Messages.Count == 0)
                return null;
            return Task.FromResult(Messages.Dequeue());
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