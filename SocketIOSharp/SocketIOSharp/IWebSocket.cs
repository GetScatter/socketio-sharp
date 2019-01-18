using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using WebSocketSharp;

namespace SocketIOSharp
{
    public interface IWebSocket : IDisposable
    {
        Task ConnectAsync(Uri url);
        Task CloseAsync();
        Task SendAsync(byte[] buffer);
        Task SendAsync(string text);
        Task<byte[]> ReceiveAsync();
        string GetError();
        WebSocketState GetState();
    }
}
