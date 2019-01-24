using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SocketIOSharp.Core;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using WebSocketSharp;

namespace SocketIOSharp.Unity3D
{
    public class SocketIO : IDisposable
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

        private string IOConnectOpcode
        {
            get
            {
                return EngineIOPacketOp.MESSAGE.ToString("d") +
                       SocketIOPacketOp.CONNECT.ToString("d");
            }
        }

        private string IOEventOpcode
        {
            get
            {
                return EngineIOPacketOp.MESSAGE.ToString("d") +
                       SocketIOPacketOp.EVENT.ToString("d");
            }
        }

        private string Namespace { get; set; }
        private int TimeoutMS { get; set; }

        private IWebSocket Socket { get; set; }

        private Dictionary<string, List<Action<IEnumerable<JToken>>>> EventListenersDict { get; set; }
        private Task ReceiverTask { get; set; }

        public SocketIO(SocketIOConfigurator config, MonoBehaviour scriptInstance = null)
        {
            if (config == null)
                config = new SocketIOConfigurator();

            if (Application.platform == RuntimePlatform.WebGLPlayer)
            {
                Socket = new WebGLWebSocket(scriptInstance);
            }
            else
            {
                Socket = new NativeWebSocket(config.Proxy);
            }

            TimeoutMS = config.Timeout;
            Namespace = config.Namespace;
            EventListenersDict = new Dictionary<string, List<Action<IEnumerable<JToken>>>>();
        }

        public void Dispose()
        {
            Socket.Dispose();
        }

        public async Task ConnectAsync(Uri uri)
        {
            if (GetState() != WebSocketState.Open && GetState() != WebSocketState.Connecting)
            {
                await Socket.ConnectAsync(uri, ReceiveMessage);
            }

            if (GetState() != WebSocketState.Open)
                throw new Exception("Socket closed.");

            //connect to socket.io
            await Socket.SendAsync(string.Format("{0}/{1}", IOConnectOpcode, Namespace));
        }

        public void On(string type, Action<IEnumerable<JToken>> callback)
        {
            List<Action<IEnumerable<JToken>>> eventListeners = null;

            if (EventListenersDict.TryGetValue(type, out eventListeners))
            {
                eventListeners.Add(callback);
            }
            else
            {
                EventListenersDict.Add(type, new List<Action<IEnumerable<JToken>>>() { callback });
            }
        }

        public Task EmitAsync(string type, object data)
        {
            return Socket.SendAsync(string.Format("{0}/{1},[\"{2}\",{3}]", IOEventOpcode, Namespace, type, JsonConvert.SerializeObject(data)));
        }

        public Task DisconnectAsync(CancellationToken? cancellationToken = null)
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

        #region Utils

        private void ReceiveMessage(byte[] result)
        {
            byte[] frame = new byte[4096];
            byte[] preamble = new byte[2];
            ArraySegment<byte> segment = new ArraySegment<byte>(frame, 0, frame.Length);

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

                var jArr = JArray.Parse(jsonStr);

                if (jArr.Count == 0)
                    return;

                string type = jArr[0].ToObject<string>();

                List<Action<IEnumerable<JToken>>> eventListeners = null;
                if (EventListenersDict.TryGetValue(type, out eventListeners))
                {
                    var args = jArr.Skip(1);
                    foreach (var listener in eventListeners)
                    {
                        listener(args);
                    }
                }
            }
        }

        private async void Ping()
        {
            await Socket.SendAsync(EngineIOPacketOp.PING.ToString("d"));
        }
        #endregion
    }
}
