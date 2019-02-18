using System;
using System.Collections;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using WebSocketSharp;

namespace SocketIOSharp.Core
{
    public class WebGLWebSocket : IWebSocket
    {
        private int NativeRef = 0;
        private MonoBehaviour ScriptInstance;

        public WebGLWebSocket(MonoBehaviour scriptInstance)
        {
            if (scriptInstance == null)
                throw new ArgumentNullException("scriptInstance");

            ScriptInstance = scriptInstance;
        }

        #region jslib import methods

        [DllImport("__Internal")]
	    private static extern int SocketCreate(string url);

	    [DllImport("__Internal")]
	    private static extern int SocketState(int socketInstance);

        [DllImport("__Internal")]
        private static extern void SocketSendBinary(int socketInstance, byte[] ptr, int length);

        [DllImport("__Internal")]
	    private static extern void SocketSend(int socketInstance, string data);

	    [DllImport("__Internal")]
	    private static extern void SocketRecv(int socketInstance, byte[] ptr, int length);

	    [DllImport("__Internal")]
	    private static extern int SocketRecvLength(int socketInstance);

	    [DllImport("__Internal")]
	    private static extern void SocketClose(int socketInstance);

	    [DllImport("__Internal")]
	    private static extern int SocketError(int socketInstance, byte[] ptr, int length);

        #endregion

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

            NativeRef = SocketCreate(url.ToString());

            while (SocketState(NativeRef) == 0)
                await Task.Yield();

            ScriptInstance.StartCoroutine(OnMessageCoroutine(onMessage));
        }

        /// <summary>
        /// Close asyncronoushly from websocket server
        /// </summary>
        /// <returns></returns>
        public Task CloseAsync()
        {
            SocketClose(NativeRef);
            return Task.CompletedTask;
        }

        /// <summary>
        /// Send buffer asyncronoushly to websocket server
        /// </summary>
        /// <param name="buffer">data buffer</param>
        /// <returns></returns>
        public Task SendAsync(byte[] buffer)
	    {
            SocketSendBinary(NativeRef, buffer, buffer.Length);
            return Task.CompletedTask;
        }

        /// <summary>
        /// Send data as string asyncronoushly to websocket server
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public Task SendAsync(string data)
        {
            SocketSend(NativeRef, data);
            return Task.CompletedTask;
        }

        /// <summary>
        /// Check asyncronoushly data received from websocket server
        /// </summary>
        /// <returns></returns>
        public Task<byte[]> ReceiveAsync()
	    {
            int length = SocketRecvLength(NativeRef);
            if (length == 0)
                return null;

            byte[] buffer = new byte[length];
            SocketRecv(NativeRef, buffer, length);
            return Task.FromResult(buffer);
        }

        /// <summary>
        /// Check errors returned from websocket server
        /// </summary>
        /// <returns></returns>
        public string GetError()
        {
            const int bufsize = 1024;
            byte[] buffer = new byte[bufsize];
            int result = SocketError(NativeRef, buffer, bufsize);

            if (result == 0)
                return null;

            return Encoding.UTF8.GetString(buffer);
        }

        /// <summary>
        /// Get underlying websocket connection state
        /// </summary>
        /// <returns></returns>
        public WebSocketState GetState()
        {
            return NativeRef != 0 ? (WebSocketState)SocketState(NativeRef) : WebSocketState.Closed;
        }

        /// <summary>
        /// Dispose websocket
        /// </summary>
        public void Dispose()
        {
            SocketClose(NativeRef);
        }

        #region Utils

        /// <summary>
        /// On message coroutine to work on WebGL builds
        /// </summary>
        /// <param name="onMessage"></param>
        /// <returns></returns>
        private IEnumerator OnMessageCoroutine(Action<byte[]> onMessage)
        {
            if (onMessage == null)
                throw new ArgumentNullException("onMessage");

            while (GetState() == WebSocketState.Open)
            {
                int length = 0;

                while (true)
                {
                    length = SocketRecvLength(NativeRef);
                    if (length == 0)
                        yield return null;
                    else
                        break;
                }

                byte[] buffer = new byte[length];
                SocketRecv(NativeRef, buffer, length);

                onMessage(buffer);
            }
        }

        #endregion
    }
}