using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using WebSocketSharp;

namespace SocketIOSharp.Core
{
    /// <summary>
    /// SocketIO interface for multiple platform implementations
    /// </summary>
    public interface ISocketIO : IDisposable
    {
        /// <summary>
        /// Connect asyncronoushly to a socket io server
        /// </summary>
        /// <param name="uri">uri to connect to</param>
        /// <returns></returns>
        Task<WebSocketState> ConnectAsync(Uri uri);

        /// <summary>
        /// Create Listener to a socket io event
        /// </summary>
        /// <param name="type">event type</param>
        /// <param name="callback">callback with event arguments</param>
        void On(string type, Action<IEnumerable<object>> callback);

        /// <summary>
        /// Remove listeners by event type
        /// </summary>
        /// <param name="type">event type</param>
        void Off(string type);

        /// <summary>
        /// Remove listeners by event type and index position
        /// </summary>
        /// <param name="type">event type</param>
        /// <param name="index">position</param>
        void Off(string type, int index);

        /// <summary>
        /// Remove listener by callback instance
        /// </summary>
        /// <param name="callback">callback instance</param>
        void Off(Action<IEnumerable<object>> callback);

        /// <summary>
        /// Remove listener by type and callback instance
        /// </summary>
        /// /// <param name="type">event type</param>
        /// <param name="callback">callback instance</param>
        void Off(string type, Action<IEnumerable<object>> callback);

        /// <summary>
        /// Emit data to socket io server for a given event
        /// </summary>
        /// <param name="type"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        Task EmitAsync(string type, object data);

        /// <summary>
        /// Disconnect asyncronoushly from socket io server 
        /// </summary>
        /// <returns></returns>
        Task DisconnectAsync();

        /// <summary>
        /// Check is the socket is open and connected
        /// </summary>
        /// <returns></returns>
        bool IsConnected();

        /// <summary>
        /// Get underlying websocket connection state
        /// </summary>
        /// <returns></returns>
        WebSocketState GetState();
    }
}
