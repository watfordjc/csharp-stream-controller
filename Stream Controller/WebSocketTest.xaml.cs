﻿using System;
using System.Buffers.Text;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Sockets;
using System.Net.WebSockets;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Converters;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using OBSWebSocketLibrary;

namespace Stream_Controller
{
    /// <summary>
    /// Interaction logic for WebSocketTest.xaml
    /// </summary>
    public partial class WebSocketTest : Window, IDisposable
    {
        private readonly ObsWsClient webSocket;
        private bool autoscroll = false;
        private System.Timers.Timer heartBeatCheck = new System.Timers.Timer(8000);
        private Guid enableHeartbeatMessageId;
        private bool disposedValue;

        public WebSocketTest()
        {
            InitializeComponent();
            webSocket = new ObsWsClient(new Uri("ws://localhost:4444"));
            webSocket.StateChange += WebSocket_StateChange;
            webSocket.StateChange += WebSocket_EnableHeartBeat;
            webSocket.ReceiveTextMessage += WebSocket_NewTextMessage;
            heartBeatCheck.Elapsed += HeartBeatTimer_Elapsed;
        }

        private async void ButtonTest_Click(object sender, RoutedEventArgs e)
        {
            btnTest.IsEnabled = false;
            await webSocket.AutoReconnectConnectAsync();
            e.Handled = true;
        }

        private void WebSocket_StateChange(object sender, WebSocketState e)
        {
            txtStatus.Text = e.ToString();
            if (e == WebSocketState.Closed)
            {
                btnTest.IsEnabled = true;
            }
        }

        private async void WebSocket_EnableHeartBeat(object sender, WebSocketState e)
        {
            if (e == WebSocketState.Open)
            {
                webSocket.StartMessageReceiveLoop();
                enableHeartbeatMessageId = Guid.NewGuid();
                Dictionary<string, object> jsonDictionary = new Dictionary<string, object>();
                jsonDictionary.Add("request-type", "SetHeartbeat");
                jsonDictionary.Add("message-id", enableHeartbeatMessageId.ToString());
                jsonDictionary.Add("enable", true);
                await webSocket.SendMessageAsync(System.Text.Json.JsonSerializer.Serialize(jsonDictionary));
            }
        }

        private void WebSocket_NewTextMessage(object sender, string message)
        {
            txtOutput.Text += message + "\n\n";
            if (autoscroll == true)
            {
                svScroll.ScrollToBottom();
            }
            using (JsonDocument document = JsonDocument.Parse(message))
            {
                JsonElement root = document.RootElement;
                JsonElement jsonUpdateType, jsonStreamTimecode, jsonRecTimecode;
                if (!root.TryGetProperty("update-type", out jsonUpdateType))
                {
                    root.TryGetProperty("message-id", out JsonElement messageId);
                    if (messageId.GetString() == enableHeartbeatMessageId.ToString())
                    {
                        root.TryGetProperty("status", out JsonElement status);
                        Trace.WriteLine("Server response to enabling HeartBeat: " + status.GetString());
                    } else
                    {
                        Trace.WriteLine("Unexpected JSON.");
                    }
                    return;
                }
                bool isStreaming = root.TryGetProperty("stream-timecode", out jsonStreamTimecode);
                bool isRecording = root.TryGetProperty("rec-timecode", out jsonRecTimecode);
                switch (jsonUpdateType.GetString())
                {
                    case "Heartbeat":
                        root.TryGetProperty("pulse", out JsonElement pulse);
                        heartBeatCheck.Enabled = pulse.GetBoolean();
                        heartBeatCheck.Enabled = true;

                        break;
                }
            }
        }

        private void HeartBeatTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            heartBeatCheck.Stop();
            Application.Current.Dispatcher.Invoke(
                async () => await webSocket.ReconnectAsync()
                );
        }

        private async void ButtonClose_Click(object sender, RoutedEventArgs e)
        {
            await webSocket.DisconnectAsync();
            e.Handled = true;
        }

        private void AutoScroll_Checked(object sender, RoutedEventArgs e)
        {
            autoscroll = true;
            svScroll.ScrollToBottom();
        }

        private void AutoScroll_Unchecked(object sender, RoutedEventArgs e)
        {
            autoscroll = false;
        }

        private void txtOutput_ScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            if (svScroll.VerticalOffset < svScroll.ScrollableHeight)
            {
                cbAutoScroll.IsChecked = false;
            }
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    heartBeatCheck.Dispose();
                    // TODO: dispose managed state (managed objects)
                }

                // TODO: free unmanaged resources (unmanaged objects) and override finalizer
                // TODO: set large fields to null
                disposedValue = true;
            }
        }

        // // TODO: override finalizer only if 'Dispose(bool disposing)' has code to free unmanaged resources
        // ~WebSocketTest()
        // {
        //     // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        //     Dispose(disposing: false);
        // }

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}
