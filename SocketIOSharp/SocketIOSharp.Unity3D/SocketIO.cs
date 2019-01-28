using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SocketIOSharp.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace SocketIOSharp.Unity3D
{
    public class SocketIO : SocketIOBase
    {
        public SocketIO(SocketIOConfigurator config, MonoBehaviour scriptInstance = null) :
            base(config)
        {
            if (Application.platform == RuntimePlatform.WebGLPlayer)
            {
                Socket = new WebGLWebSocket(scriptInstance);
            }
            else
            {
                Socket = new NativeWebSocket(config.Proxy);
            }
        }

        #region Utils

        protected override void CallMessageListeners(string jsonStr)
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
