using System;
using System.Threading.Tasks;
using WebSocketSharp;

namespace SocketIOSharp.Core
{
    /// <summary>
    /// Websocket interface for multiple platform implementations
    /// </summary>
    public interface IWebSocket : IDisposable
    {
        /// <summary>
        /// Connect asyncronoushly to websocket server
        /// </summary>
        /// <param name="url">url to connect to</param>
        /// <param name="onMessage">add callback on received messages</param>
        /// <returns></returns>
        Task ConnectAsync(Uri url, Action<byte[]> onMessage);

        /// <summary>
        /// Close asyncronoushly from websocket server
        /// </summary>
        /// <returns></returns>
        Task CloseAsync();

        /// <summary>
        /// Send buffer asyncronoushly to websocket server
        /// </summary>
        /// <param name="buffer">data buffer</param>
        /// <returns></returns>
        Task SendAsync(byte[] buffer);

        /// <summary>
        /// Send data as string asyncronoushly to websocket server
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        Task SendAsync(string data);

        /// <summary>
        /// Check asyncronoushly data received from websocket server
        /// </summary>
        /// <returns></returns>
        Task<byte[]> ReceiveAsync();

        /// <summary>
        /// Check errors returned from websocket server
        /// </summary>
        /// <returns></returns>
        string GetError();

        /// <summary>
        /// Get underlying websocket connection state
        /// </summary>
        /// <returns></returns>
        WebSocketState GetState();
    }
}
