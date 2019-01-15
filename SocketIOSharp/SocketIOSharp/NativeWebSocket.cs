using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using WebSocketSharp;

namespace SocketIOSharp
{
    public class NativeWebSocket : IWebSocket
    {
        private WebSocket Socket;
        Queue<byte[]> m_Messages = new Queue<byte[]>();
        bool IsConnected = false;
        string Error = null;

        public NativeWebSocket()
        {
        }

        public async Task Connect(Uri url)
        {
            string protocol = url.Scheme;
            if (!protocol.Equals("ws") && !protocol.Equals("wss"))
                throw new ArgumentException("Unsupported protocol: " + protocol);

            Socket = new WebSocket(url.ToString());

            Socket.OnMessage += (sender, e) => m_Messages.Enqueue(e.RawData);
            Socket.OnOpen += (sender, e) => IsConnected = true;
            Socket.OnError += (sender, e) => Error = e.Message;

            Socket.ConnectAsync();

            while (!IsConnected && Error == null)
                await Task.Yield();
        }

        public Task Send(byte[] buffer)
        {
            Socket.Send(buffer);
            return Task.FromResult(0);
        }

        public byte[] Receive()
        {
            if (m_Messages.Count == 0)
                return null;
            return m_Messages.Dequeue();
        }

        public Task Close()
        {
            Socket.Close();
            return Task.FromResult(0);
        }

        public string GetError()
        {
            return Error;
        }

        public WebSocketState GetState()
        {
            return Socket.ReadyState;
        }

        public void Dispose()
        {
            Socket.Close(1001);
        }
    }
}