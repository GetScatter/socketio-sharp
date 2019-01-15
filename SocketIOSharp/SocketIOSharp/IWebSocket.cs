using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using WebSocketSharp;

namespace SocketIOSharp
{
    public interface IWebSocket : IDisposable
    {
        Task Connect(Uri url);
        Task Close();
        Task Send(byte[] buffer);
        byte[] Receive();
        string GetError();
        WebSocketState GetState();
    }
}
