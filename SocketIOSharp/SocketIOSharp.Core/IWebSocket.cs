using System;
using System.Threading.Tasks;
using WebSocketSharp;

namespace SocketIOSharp.Core
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
