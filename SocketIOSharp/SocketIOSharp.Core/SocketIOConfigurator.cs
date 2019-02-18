namespace SocketIOSharp.Core
{
    /// <summary>
    /// Socket io configurator
    /// </summary>
    public class SocketIOConfigurator
    {
        /// <summary>
        /// Namespace for opening socket io channel
        /// </summary>
        public string Namespace;
        /// <summary>
        /// proxy to route traffic
        /// </summary>
        public Proxy  Proxy;
    }
}
