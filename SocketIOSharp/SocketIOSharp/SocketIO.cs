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
using WebSocketSharp;

namespace SocketIOSharp
{
    /// <summary>
    /// SocketIO client to connect to a socketIO server supporting Websockets only
    /// This implementation is generic version and uses websocketsharp library
    /// </summary>
    public class SocketIO : SocketIOBase
    {
        /// <summary>
        /// Construct socketIO client with given configuration
        /// </summary>
        /// <param name="config"></param>
        public SocketIO(SocketIOConfigurator config) :
                base(config)
        {
            Socket = new NativeWebSocket(config.Proxy, config.ConnectTimeout);
        }

        #region Utils

        protected override void ParseEngineIOInitValues(string jsonStr)
        {
            var jObj = JObject.Parse(jsonStr);

            var sidToken = jObj.SelectToken("sid");
            if (sidToken == null)
                throw new ArgumentException("sid field not found.");

            SocketID = sidToken.ToObject<string>();

            var pingIntervalToken = jObj.SelectToken("pingInterval");
            if (pingIntervalToken == null)
                throw new ArgumentException("pingInterval field not found.");

            PingInterval = pingIntervalToken.ToObject<UInt64>();

            var pingTimeoutToken = jObj.SelectToken("pingTimeout");
            if (pingTimeoutToken == null)
                throw new ArgumentException("pingTimeout field not found.");

            PingTimeout = pingTimeoutToken.ToObject<UInt64>();
        }

        protected override void EmitToEventListeners(string jsonStr)
        {
            var jArr = JArray.Parse(jsonStr);

            if (jArr.Count == 0)
                return;

            string type = jArr[0].ToObject<string>();

            List<Action<IEnumerable<object>>> eventListeners = null;
            if (EventListenersDict.TryGetValue(type, out eventListeners))
            {
                var args = jArr.Skip(1);
                foreach (var listener in eventListeners)
                {
                    listener(args);
                }
            }
        }

        protected override string SerializeEmitObject(object data)
        {
            return JsonConvert.SerializeObject(data);
        }

        #endregion
    }
}
