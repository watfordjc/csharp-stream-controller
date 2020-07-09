using System;
using System.Collections.Generic;
using System.IO;
using System.Net.WebSockets;
using System.Text;

namespace uk.JohnCook.dotnet.WebSocketLibrary
{
    /// <summary>
    /// Object containing a WebSocketReceiveResult and a received byte[].
    /// </summary>
    public class WsClientReceivedMessage
    {
        public WebSocketReceiveResult Result { get; set; }
        public MemoryStream Message { get; set; }
    }

    /// <summary>
    /// Object containing an Exception and current reconnect delay.
    /// </summary>
    public class WsClientErrorMessage
    {
        public Exception Error { get; set; }
        public int ReconnectDelay { get; set; }
    }
}
