using System;
using System.Collections;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using WebSocketSharp;

namespace SocketIOSharp.Core
{
    public class WebGLWebSocket : MonoBehaviour, IWebSocket
    {
        int NativeRef = 0;

        public WebGLWebSocket()
        {
        }

        #region jslib import methods

        [DllImport("__Internal")]
	    private static extern int SocketCreate (string url);

	    [DllImport("__Internal")]
	    private static extern int SocketState (int socketInstance);

	    [DllImport("__Internal")]
	    private static extern void SocketSend (int socketInstance, byte[] ptr, int length);

	    [DllImport("__Internal")]
	    private static extern void SocketRecv (int socketInstance, byte[] ptr, int length);

	    [DllImport("__Internal")]
	    private static extern int SocketRecvLength (int socketInstance);

	    [DllImport("__Internal")]
	    private static extern void SocketClose (int socketInstance);

	    [DllImport("__Internal")]
	    private static extern int SocketError (int socketInstance, byte[] ptr, int length);

        #endregion

        public async Task ConnectAsync(Uri url, Action<byte[]> onMessage)
        {
            Console.WriteLine("WebGLWebSocket ConnectAsync.");

            string protocol = url.Scheme;
            if (!protocol.Equals("ws") && !protocol.Equals("wss"))
                throw new ArgumentException("Unsupported protocol: " + protocol);

            NativeRef = SocketCreate(url.ToString());

            while (SocketState(NativeRef) == 0)
                await Task.Yield();

            StartCoroutine(OnMessageCoroutine(onMessage));
        }

        public Task CloseAsync()
        {
            Console.WriteLine("WebGLWebSocket CloseAsync.");

            SocketClose(NativeRef);
            return Task.CompletedTask;
        }

        public Task SendAsync(byte[] buffer)
	    {
            Console.WriteLine("WebGLWebSocket SendAsync.");

            SocketSend(NativeRef, buffer, buffer.Length);
            return Task.CompletedTask;
        }
        public Task SendAsync(string data)
        {
            Console.WriteLine("WebGLWebSocket SendAsync.");

            var buffer = Encoding.UTF8.GetBytes(data);
            SocketSend(NativeRef, buffer, buffer.Length);
            return Task.CompletedTask;
        }

        public Task<byte[]> ReceiveAsync()
	    {
            Console.WriteLine("WebGLWebSocket ReceiveAsync.");

            int length = SocketRecvLength(NativeRef);
            if (length == 0)
                return null;

            byte[] buffer = new byte[length];
            SocketRecv(NativeRef, buffer, length);
            return Task.FromResult(buffer);
        }

        public string GetError()
        {
            Console.WriteLine("WebGLWebSocket GetError.");

            const int bufsize = 1024;
            byte[] buffer = new byte[bufsize];
            int result = SocketError(NativeRef, buffer, bufsize);

            if (result == 0)
                return null;

            return Encoding.UTF8.GetString(buffer);
        }

        public WebSocketState GetState()
        {
            return NativeRef != 0 ? (WebSocketState)SocketState(NativeRef) : WebSocketState.Closed;
        }

        public void Dispose()
        {
            SocketClose(NativeRef);
        }

        #region Utils

        private IEnumerator OnMessageCoroutine(Action<byte[]> onMessage)
        {
            if (onMessage == null)
                throw new ArgumentNullException("onMessage");

            while (GetState() == WebSocketState.Open)
            {
                int length = SocketRecvLength(NativeRef);
                if (length == 0)
                    continue;

                byte[] buffer = new byte[length];
                SocketRecv(NativeRef, buffer, length);

                onMessage(buffer);

                yield return null;
            }
        }

        #endregion
    }
}