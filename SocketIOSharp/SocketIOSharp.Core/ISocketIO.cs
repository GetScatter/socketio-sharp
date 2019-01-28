using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using WebSocketSharp;

namespace SocketIOSharp.Core
{
    public interface ISocketIO : IDisposable
    {
        Task<WebSocketState> ConnectAsync(Uri uri);
        void On(string type, Action<IEnumerable<object>> callback);
        void Off(string type);
        void Off(string type, int index);
        void Off(Action<IEnumerable<object>> callback);
        void Off(string type, Action<IEnumerable<object>> callback);
        Task EmitAsync(string type, object data);
        Task DisconnectAsync();
        bool IsConnected();
        WebSocketState GetState();
    }
}
