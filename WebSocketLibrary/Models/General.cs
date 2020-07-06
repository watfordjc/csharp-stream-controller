using System;
using System.Collections.Generic;
using System.IO;
using System.Net.WebSockets;
using System.Text;

namespace WebSocketLibrary.Models
{
    /// <summary>
    /// Object containing a WebSocketReceiveResult and a received byte[].
    /// </summary>
    public class ReceivedMessage
    {
        public WebSocketReceiveResult Result { get; set; }
        public MemoryStream Message { get; set; }
    }

    /// <summary>
    /// Object containing an Exception and current reconnect delay.
    /// </summary>
    public class ErrorMessage
    {
        public Exception Error { get; set; }
        public int ReconnectDelay { get; set; }
    }
}
