using System;
using System.Collections.Generic;
using System.Text;

namespace SocketIOSharp
{
    public class SocketIOConfiguration
    {
        public int    Timeout = 60000;
        public string Namespace;
        public Proxy  Proxy;
    }
}
