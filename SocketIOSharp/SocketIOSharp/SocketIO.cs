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
    public class SocketIO : IDisposable
    {
        public enum EngineIOSessionInfo
        {
            CLOSE,
            PING,
            PONG,
            MESSAGE,
            UPGRADE,
            NOOP
        };

        public enum SocketIOSessionInfo
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
                return EngineIOSessionInfo.MESSAGE.ToString("d") +
                       SocketIOSessionInfo.CONNECT.ToString("d");
            }
        }

        private string IOEventOpcode
        {
            get
            {
                return EngineIOSessionInfo.MESSAGE.ToString("d") +
                       SocketIOSessionInfo.EVENT.ToString("d");
            }
        }

        private string Namespace { get; set; }
        private int TimeoutMS { get; set; }

        private IWebSocket Socket { get; set; }

        TaskCompletionSource<bool> OpenTask { get; set; }
        private Dictionary<string, TaskCompletionSource<Object>> OpenTasks { get; set; }
        private Dictionary<string, DateTime> OpenTaskTimes { get; set; }

        private Task ReceiverTask { get; set; }
        private Task TimoutTasksTask { get; set; }

        public SocketIO(int timeout = 60000, string ns = null)
        {

#if UNITY_WEBGL && !UNITY_EDITOR
            Socket = new WebGLWebSocket();
#else
            Socket = new NativeWebSocket();
#endif

            OpenTasks = new Dictionary<string, TaskCompletionSource<Object>>();
            OpenTaskTimes = new Dictionary<string, DateTime>();

            TimeoutMS = timeout;
            Namespace = ns;
        }

        public void Dispose()
        {
            Socket.Dispose();
        }

        public async Task Connect(Uri uri, CancellationToken? cancellationToken = null)
        {
            if (Socket.GetState() != WebSocketState.Open && Socket.GetState() != WebSocketState.Connecting)
            {
                await Socket.Connect(uri);
            }

            if (Socket.GetState() != WebSocketState.Open)
                throw new Exception("Socket closed.");

            //connect to socket.io
            await Socket.Send(Encoding.UTF8.GetBytes(string.Format("{0}/{1}", IOConnectOpcode, Namespace)));

            //{
            //    onOpen();


            //    /*await Send();



            //    await Pair(true);*/
            //}
            //else

            //    ReceiverTask = Receive(cancellationToken);
            //    TimoutTasksTask = Task.Run(() => TimeoutOpenTasksCheck());
        }

        public void On(string type, Action<object> callback)
        {

        }

        public Task Emit(string type, string data)
        {
            return Socket.Send(Encoding.UTF8.GetBytes(string.Format("{0}/{1},[{2},{3}]", IOEventOpcode, Namespace, type, data)));
        }

        public void Receive(CancellationToken? cancellationToken = null)
        {
            byte[] frame = new byte[4096];
            byte[] preamble = new byte[2];
            ArraySegment<byte> segment = new ArraySegment<byte>(frame, 0, frame.Length);

            while (Socket.GetState() == WebSocketState.Open)
            {
                byte[] result;
                MemoryStream ms = new MemoryStream();

                result = Socket.Receive();

                ms.Write(result, 0, result.Length);

                ms.Seek(0, SeekOrigin.Begin);
                ms.Read(preamble, 0, preamble.Length);

                // Disregarding Handshaking/Upgrading
                if (Encoding.UTF8.GetString(preamble) != IOEventOpcode)
                {
                    ms.Dispose();
                    continue;
                }

                //skip "," from packet
                ms.Seek(ms.Position + 1, SeekOrigin.Begin);

                string jsonStr = null;
                using (var sr = new StreamReader(ms))
                {
                    jsonStr = sr.ReadToEnd();
                }
                ms.Dispose();

                //var jArr = JArray.Parse(jsonStr);

                //if (jArr.Count == 0)
                //    continue;

                //string type = jArr[0].ToObject<string>();

                //switch (type)
                //{
                //    case "paired":
                //        if (jArr.Count == 2)
                //            HandlePairedResponse(jArr[1].ToObject<bool?>());
                //        break;
                //    case "rekey":
                //        HandleRekeyResponse();
                //        break;
                //    case "api":
                //        if (jArr.Count == 2)
                //            HandleApiResponse(jArr[1]);
                //        break;
                //}
            }
        }

        /*
        public async Task Pair(bool passthrough = false)
        {
            PairOpenTask = new TaskCompletionSource<bool>();

            await Send("pair", new
            {
                data = new
                {
                    appkey = StorageProvider.GetAppkey(),
                    passthrough,
                    origin = AppName
                },
                plugin = AppName
            });

            await PairOpenTask.Task;
        }

        public async Task<JToken> SendApiRequest(Request request)
        {
            if (request.type == "identityFromPermissions" && !Paired)
                return false;

            await Pair();

            if (!Paired)
                throw new Exception("The user did not allow this app to connect to their Scatter");

            var tcs = new TaskCompletionSource<JToken>();

            request.id = UtilsHelper.RandomNumber(24);
            request.appkey = StorageProvider.GetAppkey();
            request.nonce = StorageProvider.GetNonce() ?? "";

            var nextNonce = UtilsHelper.RandomNumberBytes();
            request.nextNonce = UtilsHelper.ByteArrayToHexString(Sha256Manager.GetHash(nextNonce));
            StorageProvider.SetNonce(UtilsHelper.ByteArrayToHexString(nextNonce));

            OpenTasks.Add(request.id, tcs);
            OpenTaskTimes.Add(request.id, DateTime.Now);

            await Send("api", new { data = request, plugin = AppName });

            return await tcs.Task;
        }*/

        public Task Disconnect(CancellationToken? cancellationToken = null)
        {
            return Socket.Close();
        }

        public bool IsConnected()
        {
            return Socket.GetState() == WebSocketState.Open;
        }

#region Utils

        private async void Ping()
        {
            await Socket.Send(Encoding.UTF8.GetBytes(EngineIOSessionInfo.PING.ToString("d")));
        }

        /*private void TimeoutOpenTasksCheck()
        {
            while (Socket.State == WebSocketState.Open)
            {
                var now = DateTime.Now;
                int count = 0;
                List<string> toRemoveKeys = new List<string>();

                foreach (var key in OpenTaskTimes.Keys.ToList())
                {
                    if ((now - OpenTaskTimes[key]).TotalMilliseconds >= TimeoutMS)
                    {
                        toRemoveKeys.Add(key);
                    }

                    //sleep checking each 10 requests
                    if ((count % 10) == 0)
                    {
                        count = 0;
                        Thread.Sleep(1000);
                    }

                    count++;
                }

                foreach (var key in toRemoveKeys)
                {
                    TaskCompletionSource<JToken> openTask = OpenTasks[key];

                    OpenTasks.Remove(key);
                    OpenTaskTimes.Remove(key);

                    openTask.SetResult(JToken.FromObject(new ApiError()
                    {
                        code = "0",
                        isError = "true",
                        message = "Request timeout."
                    }));
                }
            }
        }*/
/*
        private void HandleSession(string data)
        {
            var emt = data[0];
            var json = JObject.Parse(data.Substring(1));
            int pintv = (int)json.GetValue("pingInterval");
            _timer.Change(pintv, pintv);
        }

        private void HandleApiResponse(JToken data)
        {
            if (data == null && data.Children().Count() != 2)
                return;

            var idToken = data.SelectToken("id");

            if (idToken == null)
                throw new Exception("response id not found.");

            string id = idToken.ToObject<string>();

            TaskCompletionSource<JToken> openTask;
            if (!OpenTasks.TryGetValue(id, out openTask))
                return;

            OpenTasks.Remove(id);
            OpenTaskTimes.Remove(id);

            openTask.SetResult(data.SelectToken("result"));
        }

        
        private void HandleRekeyResponse()
        {
            GenerateNewAppKey();
            Send("rekeyed", new
            {
                plugin = AppName,
                data = new
                {
                    origin = AppName,
                    appkey = StorageProvider.GetAppkey()
                }
            });
        }

        private void HandlePairedResponse(bool? paired)
        {
            Paired = paired.GetValueOrDefault();

            if (Paired)
            {
                var storedAppKey = StorageProvider.GetAppkey();

                string hashed = storedAppKey.StartsWith("appkey:") ?
                    UtilsHelper.ByteArrayToHexString(Sha256Manager.GetHash(Encoding.UTF8.GetBytes(storedAppKey))) :
                    storedAppKey;

                if (string.IsNullOrWhiteSpace(storedAppKey) ||
                    storedAppKey != hashed)
                {
                    StorageProvider.SetAppkey(hashed);
                }
            }

            if (PairOpenTask != null)
            {
                PairOpenTask.SetResult(Paired);
            }
        }

        private void GenerateNewAppKey()
        {
            StorageProvider.SetAppkey("appkey:" + UtilsHelper.RandomNumber(24));
        }
        */

#endregion
    }
}
