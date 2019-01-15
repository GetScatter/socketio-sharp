﻿using System;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using WebSocketSharp;

namespace SocketIOSharp
{
    public class WebGLWebSocket : IWebSocket
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

        public async Task Connect(Uri url)
        {
            string protocol = url.Scheme;
            if (!protocol.Equals("ws") && !protocol.Equals("wss"))
                throw new ArgumentException("Unsupported protocol: " + protocol);

            NativeRef = SocketCreate(url.ToString());

            while (SocketState(NativeRef) == 0)
                await Task.Yield();
        }

        public Task Close()
        {
            SocketClose(NativeRef);
            return Task.FromResult(0);
        }

        public Task Send(byte[] buffer)
	    {
		    SocketSend(NativeRef, buffer, buffer.Length);
            return Task.FromResult(0);
        }

	    public byte[] Receive()
	    {
		    int length = SocketRecvLength(NativeRef);
		    if (length == 0)
			    return null;

		    byte[] buffer = new byte[length];
		    SocketRecv(NativeRef, buffer, length);
		    return buffer;
	    }

        public string GetError()
        {
            const int bufsize = 1024;
            byte[] buffer = new byte[bufsize];
            int result = SocketError(NativeRef, buffer, bufsize);

            if (result == 0)
                return null;

            return Encoding.UTF8.GetString(buffer);
        }

        public WebSocketState GetState()
        {
            return (WebSocketState)SocketState(NativeRef);
        }

        public void Dispose()
        {
            SocketClose(NativeRef);
        }
    }
}