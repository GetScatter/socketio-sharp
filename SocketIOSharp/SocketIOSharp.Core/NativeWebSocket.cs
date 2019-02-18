using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using WebSocketSharp;

namespace SocketIOSharp.Core
{
    /// <summary>
    /// Native socket using WebSocketSharp
    /// </summary>
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

        /// <summary>
        /// Connect asyncronoushly to websocket server
        /// </summary>
        /// <param name="url">url to connect to</param>
        /// <param name="onMessage">add callback on received messages</param>
        /// <returns></returns>
        public async Task ConnectAsync(Uri url, Action<byte[]> onMessage)
        {
            string protocol = url.Scheme;
            if (!protocol.Equals("ws") && !protocol.Equals("wss"))
                throw new ArgumentException("Unsupported protocol: " + protocol);

            Socket = new WebSocket(url.ToString());

            if (Proxy != null)
                Socket.SetProxy(Proxy.Url, Proxy.UserName, Proxy.Password);

            Socket.OnMessage += (sender, e) => {
                onMessage(e.RawData);
                Messages.Enqueue(e.RawData);  
            };

            Socket.OnOpen += (sender, e) => IsConnected = true;
            Socket.OnError += (sender, e) => Error = e.Message;

            Socket.ConnectAsync();

            while (!IsConnected && Error == null)
                await Task.Yield();
        }

        /// <summary>
        /// Close asyncronoushly from websocket server
        /// </summary>
        /// <returns></returns>
        public Task CloseAsync()
        {
            return Task.Run(() => { Socket.Close(); });
        }

        /// <summary>
        /// Send buffer asyncronoushly to websocket server
        /// </summary>
        /// <param name="buffer">data buffer</param>
        /// <returns></returns>
        public Task SendAsync(byte[] buffer)
        {
            return Task.Run(() => { Socket.Send(buffer); });
        }

        /// <summary>
        /// Send data as string asyncronoushly to websocket server
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public Task SendAsync(string data)
        {
            return Task.Run(() => { Socket.Send(data); });
        }

        /// <summary>
        /// Check asyncronoushly data received from websocket server
        /// </summary>
        /// <returns></returns>
        public Task<byte[]> ReceiveAsync()
        {
            return Task.Run(() => {
                if (Messages.Count == 0)
                    return null;
                return Messages.Dequeue();
            });
        }

        /// <summary>
        /// Check errors returned from websocket server
        /// </summary>
        /// <returns></returns>
        public string GetError()
        {
            return Error;
        }

        /// <summary>
        /// Get underlying websocket connection state
        /// </summary>
        /// <returns></returns>
        public WebSocketState GetState()
        {
            return Socket != null ? Socket.ReadyState : WebSocketState.Closed;
        }

        /// <summary>
        /// Dispose websocket
        /// </summary>
        public void Dispose()
        {
            Socket.Close(1001);
        }
    }
}