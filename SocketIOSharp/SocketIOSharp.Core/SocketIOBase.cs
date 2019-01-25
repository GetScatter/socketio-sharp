using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using WebSocketSharp;

namespace SocketIOSharp.Core
{
    public abstract class SocketIOBase : ISocketIO
    {
        public enum EngineIOPacketOp
        {
            OPEN,
            CLOSE,
            PING,
            PONG,
            MESSAGE,
            UPGRADE,
            NOOP
        };

        public enum SocketIOPacketOp
        {
            CONNECT,
            DISCONNECT,
            EVENT,
            ACK,
            ERROR,
            BINARY_EVENT,
            BINARY_ACK
        }

        protected string IOConnectOpcode
        {
            get
            {
                return EngineIOPacketOp.MESSAGE.ToString("d") +
                       SocketIOPacketOp.CONNECT.ToString("d");
            }
        }

        protected string IOEventOpcode
        {
            get
            {
                return EngineIOPacketOp.MESSAGE.ToString("d") +
                       SocketIOPacketOp.EVENT.ToString("d");
            }
        }

        protected string Namespace { get; set; }
        protected int TimeoutMS { get; set; }

        protected IWebSocket Socket { get; set; }

        protected Dictionary<string, List<Action<IEnumerable<object>>>> EventListenersDict { get; set; }
        protected Task ReceiverTask { get; set; }

        public SocketIOBase(SocketIOConfigurator config)
        {
            if (config == null)
                config = new SocketIOConfigurator();

            TimeoutMS = config.Timeout;
            Namespace = config.Namespace;
            EventListenersDict = new Dictionary<string, List<Action<IEnumerable<object>>>>();
        }

        public void Dispose()
        {
            Socket.Dispose();
        }

        public async Task<WebSocketState> ConnectAsync(Uri uri)
        {
            if (GetState() != WebSocketState.Open && GetState() != WebSocketState.Connecting)
            {
                await Socket.ConnectAsync(uri, ReceiveMessage);
            }

            if (GetState() != WebSocketState.Open)
                return GetState();

            //connect to socket.io
            await Socket.SendAsync(string.Format("{0}/{1}", IOConnectOpcode, Namespace));
            return GetState();
        }

        public void On(string type, Action<IEnumerable<object>> callback)
        {
            List<Action<IEnumerable<object>>> eventListeners = null;

            if (EventListenersDict.TryGetValue(type, out eventListeners))
            {
                eventListeners.Add(callback);
            }
            else
            {
                EventListenersDict.Add(type, new List<Action<IEnumerable<object>>>() { callback });
            }
        }

        public Task EmitAsync(string type, object data)
        {
            return Socket.SendAsync(string.Format("{0}/{1},[\"{2}\",{3}]", IOEventOpcode, Namespace, type, SerializeEmitObject(data)));
        }

        public Task DisconnectAsync()
        {
            return Socket.CloseAsync();
        }

        public bool IsConnected()
        {
            return GetState() == WebSocketState.Open;
        }

        public WebSocketState GetState()
        {
            return Socket != null ? Socket.GetState() : WebSocketState.Closed;
        }

        private void ReceiveMessage(byte[] result)
        {
            byte[] preamble = new byte[2];

            if (result == null)
                return;

            using (MemoryStream ms = new MemoryStream())
            {
                ms.Write(result, 0, result.Length);

                ms.Seek(0, SeekOrigin.Begin);
                ms.Read(preamble, 0, preamble.Length);

                // Disregarding Handshaking/Upgrading
                if (Encoding.UTF8.GetString(preamble) != IOEventOpcode)
                {
                    ms.Dispose();
                    return;
                }

                //skip "," from packet
                ms.Seek(ms.Position + Namespace.Length + 2, SeekOrigin.Begin);

                string jsonStr = null;
                using (var sr = new StreamReader(ms))
                {
                    jsonStr = sr.ReadToEnd();
                }

                CallMessageListeners(jsonStr);
            }
        }

        protected abstract void CallMessageListeners(string jsonStr);

        protected abstract string SerializeEmitObject(object data);
    }
}
