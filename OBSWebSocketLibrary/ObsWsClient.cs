using System;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Net.WebSockets;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;

namespace OBSWebSocketLibrary
{
    public class ObsWsClient : WebSocketLibrary.GenericClient
    {
        public ObsWsClient(Uri url) : base(url)
        {
        }
    }
}