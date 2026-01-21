using UnityEngine;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;
using System;
using Game.Protocol;
using Newtonsoft.Json;
using UnityTimer;
using System.Collections;
using Game.Flow;


namespace Game
{
    public partial class Net : Singleton<Net>
    {
        #region Inner Classes

        public class Bytes
        {
            public byte[] content;
            public int read = 0;
            public int write = 0;
            public int Length => write - read;
            public Bytes(byte[] bytes)
            {
                content = bytes;
                write = bytes.Length;
            }
        }

        public class Packet
        {
            public string Name;
            public string Json;
        }

        #endregion

        #region Fields and Properties

        private byte[] buffer = new byte[Config.Net.SocketBufferSize];
        private int startIndex = 0;
        private int usefulLen = 0;

        private Socket Socket { get; set; }
        private Queue<Bytes> Writes { get; set; } = new Queue<Bytes>();
        private Queue<byte[]> ReceiveBytesQueue = new Queue<byte[]>();
        private Queue<Packet> PacketQueue = new Queue<Packet>();

        private int reconnectAttempts = 0;
        private bool isReconnecting = false;
        private Coroutine reconnectCoroutine = null;

        public bool IsReconnecting => isReconnecting;

        #endregion

        #region Unity Lifecycle Methods

        void Awake()
        {
            Data.Instance.after.Register(Data.Type.LoginAccount, OnAfterLoginAccountChanged);
            Data.Instance.after.Register(Data.Type.Online, OnAfterOnlineChanged);
            Data.Instance.after.Register(Data.Type.Option, OnAfterOptionChanged);
            Data.Instance.befor.Register(Data.Type.OptionReturn, OnBeforOptionReturnChanged);
            Data.Instance.befor.Register(Data.Type.SocketMissedHeartbeats, OnBeforeMissedHeartbeatsChanged);
            Data.Instance.befor.Register(Data.Type.Ping, OnAfterPingChanged);
            Data.Instance.after.Register(Data.Type.Servers, OnAfterServersChanged);
        }

        void Update()
        {
            if (Socket != null && Socket.Connected)
            {
                DequeueRawBytes();
                DequeuePackets();
                TryReceiveFromSocket();
            }
        }

        #endregion

        #region Initialization and Connection

        public void Init()
        {
            Socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            Timer.Register(Config.Net.HeartbeatInterval, Heartbeat, null, true, true);
        }

        public void Connect(string ip, int port)
        {
            StartCoroutine(ConnectCoroutine(ip, port));
        }

        public IEnumerator ConnectCoroutine(string ip, int port)
        {
            if (Socket != null)
            {
                try
                {
                    Socket.Shutdown(SocketShutdown.Both);
                }
                catch (Exception e)
                {
                    Utils.Debug.LogWarning("Socket", $"Exception during socket shutdown: {e.Message}");
                }
                Socket.Close();
            }

            Socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            IAsyncResult result = Socket.BeginConnect(ip, port, null, null);
            float startTime = Time.time;
            while (!result.IsCompleted)
            {
            if (Time.time - startTime > Config.Net.ConnectionTimeoutMs / 1000f)
            {
                Utils.Debug.LogError("Socket", $"Connection timeout after {Time.time - startTime} seconds. Target: {ip}:{port}");
                Socket.Close();
                
                Data.Instance.Dark = null;
                Data.Instance.Tip = (UI.Tips.Fly, Localization.Instance.Get("connection_timeout"));
                Data.Instance.Online = false;
                yield break;
            }
                yield return null;
            }

            try
            {
                Socket.EndConnect(result);
                Data.Instance.Online = true;
                Data.Instance.Ping = DateTime.Now;
                Data.Instance.Pong = DateTime.Now;
                Data.Instance.SocketMissedHeartbeats = 0;
                Utils.Debug.LogSuccess("Socket", $"Successfully connected to server: {ip}:{port}");
            }
        catch (SocketException e)
        {
            Utils.Debug.LogError("Socket", $"Connection failed, SocketException: {e.Message}, ErrorCode: {e.ErrorCode}, SocketErrorCode: {e.SocketErrorCode}. Target: {ip}:{port}");
            Socket.Close();
            
            Data.Instance.Dark = null;
            string errorKey = "connection_failed";
            if (e.SocketErrorCode == SocketError.ConnectionRefused)
            {
                errorKey = "connection_refused";
            }
            Data.Instance.Tip = (UI.Tips.Fly, Localization.Instance.Get(errorKey));
            Data.Instance.Online = false;
        }
        catch (Exception e)
        {
            Utils.Debug.LogError("Socket", $"Connection failed, unexpected exception: {e.Message}. Target: {ip}:{port}");
            Socket.Close();
            
            Data.Instance.Dark = null;
            Data.Instance.Tip = (UI.Tips.Fly, Localization.Instance.Get("connection_failed"));
            Data.Instance.Online = false;
        }
        }

        #endregion

        #region Data Processing

        private void DequeueRawBytes()
        {
            while (ReceiveBytesQueue.Count > 0)
            {
                var bytes = ReceiveBytesQueue.Dequeue();
                var packet = ParsePacket(bytes);
                if (packet != null)
                {
                    PacketQueue.Enqueue(packet);
                }
            }
        }

        private void DequeuePackets()
        {
            while (PacketQueue.Count > 0)
            {
                HandlePacket(PacketQueue.Dequeue());
            }
        }

        private void TryReceiveFromSocket()
        {
            if (Socket.Poll(Config.Net.SocketPollIntervalMs, SelectMode.SelectRead))
            {
                bool loop = true;
                while (loop)
                {
                    loop = false;

                    if ((buffer.Length - (startIndex + usefulLen)) < Config.Net.SocketBlockSize)
                    {
                        Array.Copy(buffer, startIndex, buffer, 0, usefulLen);
                        startIndex = 0;
                    }

                    try
                    {
                        int receive = Socket.Receive(buffer, startIndex + usefulLen, Config.Net.SocketBlockSize, SocketFlags.None);
                        if (receive == 0)
                        {
                            Utils.Debug.LogWarning("Socket", "Server disconnected (Receive == 0)");
                            Socket.Close();
                            Data.Instance.Tip = (UI.Tips.Fly, Localization.Instance.Get("server_disconnected"));
                            Data.Instance.Online = false;
                            return;
                        }
                        else
                        {
                            usefulLen += receive;

                            while (usefulLen > 4)
                            {
                                int datalen = BitConverter.ToInt32(buffer, startIndex);
                                int totalLen = datalen + 4;

                                if (usefulLen < totalLen)
                                {
                                    if (totalLen > Config.Net.SocketBlockSize && receive == Config.Net.SocketBlockSize)
                                        loop = true;
                                    break;
                                }

                                byte[] payload = new byte[datalen];
                                Array.Copy(buffer, startIndex + 4, payload, 0, datalen);
                                ReceiveBytesQueue.Enqueue(payload);

                                usefulLen -= totalLen;
                                startIndex += totalLen;
                            }
                        }
                    }
                catch (SocketException e)
                {
                    Utils.Debug.LogError("Net", $"Receive data exception: {e.Message}, ErrorCode: {e.ErrorCode}, SocketErrorCode: {e.SocketErrorCode}");
                    Data.Instance.Tip = (UI.Tips.Fly, Localization.Instance.Get("network_communication_error"));
                    Data.Instance.Online = false;
                }
                catch (Exception e)
                {
                    Utils.Debug.LogError("Net", $"Receive data unexpected exception: {e.Message}\nStackTrace: {e.StackTrace}");
                    Data.Instance.Tip = (UI.Tips.Fly, Localization.Instance.Get("network_communication_error"));
                    Data.Instance.Online = false;
                }
                }
            }
        }

        #endregion

        #region Send Methods

        public void Send(Protocol.Base protocol)
        {
            string json = JsonUtility.ToJson(protocol);
            PrintSendMessage(protocol.GetType().Name, json);

            byte[] nameBytes = EncodeName(protocol.GetType().Name);
            byte[] bodyBytes = Encoding.UTF8.GetBytes(json);
            int len = nameBytes.Length + bodyBytes.Length;
            byte[] sendBytes = new byte[4 + len];

            byte[] lenBytes = BitConverter.GetBytes(len);
            Array.Copy(lenBytes, 0, sendBytes, 0, 4);
            Array.Copy(nameBytes, 0, sendBytes, 4, nameBytes.Length);
            Array.Copy(bodyBytes, 0, sendBytes, 4 + nameBytes.Length, bodyBytes.Length);

            Bytes byteArray = new Bytes(sendBytes);
            int count = 0;
            lock (Writes)
            {
                Writes.Enqueue(byteArray);
                count = Writes.Count;
            }

            if (count == 1)
            {
                try
                {
                    Socket.BeginSend(sendBytes, 0, sendBytes.Length, 0, SendCallback, Socket);
                }
                catch (SocketException e)
                {
                    Utils.Debug.LogError("Socket", $"SocketException during send: {e.Message}, ErrorCode: {e.ErrorCode}, SocketErrorCode: {e.SocketErrorCode}");
                    Data.Instance.Tip = (UI.Tips.Fly, Localization.Instance.Get("send_failed"));
                    Data.Instance.Online = false;
                }
                catch (Exception e)
                {
                    Utils.Debug.LogError("Socket", $"Unexpected exception during send: {e.Message}\nStackTrace: {e.StackTrace}");
                    Data.Instance.Tip = (UI.Tips.Fly, Localization.Instance.Get("send_failed"));
                    Data.Instance.Online = false;
                }
            }
        }

        public void SendCallback(IAsyncResult ar)
        {
            var socket = (System.Net.Sockets.Socket)ar.AsyncState;
            if (socket == null || !socket.Connected)
            {
                Utils.Debug.LogWarning("Socket", "SendCallback: Socket is null or not connected");
                return;
            }

            int count = 0;
            try
            {
                count = socket.EndSend(ar);
            }
            catch (SocketException e)
            {
                Utils.Debug.LogError("Socket", $"SocketException in SendCallback: {e.Message}, ErrorCode: {e.ErrorCode}, SocketErrorCode: {e.SocketErrorCode}");
                Data.Instance.Online = false;
                return;
            }
            catch (Exception e)
            {
                Utils.Debug.LogError("Socket", $"Unexpected exception in SendCallback: {e.Message}");
                Data.Instance.Online = false;
                return;
            }

            Bytes b;
            lock (Writes)
            {
                b = Writes.Peek();
            }

            b.read += count;
            if (b.Length == 0)
            {
                lock (Writes)
                {
                    Writes.Dequeue();
                    if (Writes.Count > 0)
                        b = Writes.Peek();
                    else
                        return;
                }
            }

            if (b != null)
            {
                try
                {
                    socket.BeginSend(b.content, b.read, b.Length, 0, SendCallback, socket);
                }
                catch (SocketException e)
                {
                    Utils.Debug.LogError("Socket", $"SocketException during SendCallback continuation: {e.Message}, ErrorCode: {e.ErrorCode}, SocketErrorCode: {e.SocketErrorCode}");
                    Data.Instance.Online = false;
                }
                catch (Exception e)
                {
                    Utils.Debug.LogError("Socket", $"Unexpected exception during SendCallback continuation: {e.Message}");
                    Data.Instance.Online = false;
                }
            }
        }

        private byte[] EncodeName(string name)
        {
            string[] parts = name.Split('+');
            byte[] nameBytes = Encoding.UTF8.GetBytes(parts[0]);
            Int16 len = (Int16)nameBytes.Length;
            byte[] bytes = new byte[2 + len];
            bytes[0] = (byte)(len % 256);
            bytes[1] = (byte)(len / 256);
            Array.Copy(nameBytes, 0, bytes, 2, len);
            return bytes;
        }

        #endregion

        #region Packet Processing

        private Packet ParsePacket(byte[] data)
        {
            if (data.Length < 2) return null;

            int nameLength = data[1] * 256 + data[0];
            if (data.Length < 2 + nameLength) return null;

            string name = Encoding.UTF8.GetString(data, 2, nameLength);
            string json = Encoding.UTF8.GetString(data, 2 + nameLength, data.Length - 2 - nameLength);

            return new Packet { Name = name, Json = json };
        }

        private void HandlePacket(Packet packet)
        {
            string typeName = $"Game.Protocol.{packet.Name}, Game";
            Type type = Type.GetType(typeName);
            if (type != null)
            {
                object obj = JsonConvert.DeserializeObject(packet.Json, type);
                if (obj is Base protocol)
                {
                    PrintReceiveMessage(protocol);
                    protocol.Processed();
                }
            }
        }

        private Base Decode(string name, byte[] bytes)
        {
            try
            {
                string content = Encoding.UTF8.GetString(bytes);
                return (Base)JsonConvert.DeserializeObject(content, Type.GetType($"Game.Protocol.{name}"));
            }
            catch (Exception)
            {
                return new Base();
            }
        }

        #endregion

        #region Heartbeat Management

        private void Heartbeat()
        {
            if (!Data.Instance.Online)
            {
                Utils.Debug.LogHeartbeat("Socket", "Heartbeat check: Not online, skipping");
                return;
            }

            if (Socket != null && Socket.Connected)
            {
                TimeSpan delay = DateTime.Now - Data.Instance.Pong;
                if (delay.TotalSeconds <= Config.Net.HeartbeatTimeout)
                {
                    if (Data.Instance.SocketMissedHeartbeats > 0)
                    {
                        Utils.Debug.LogSuccess("Socket", $"Heartbeat recovered. Previously missed heartbeats: {Data.Instance.SocketMissedHeartbeats}");
                    }

                Data.Instance.SocketMissedHeartbeats = 0;
                Data.Instance.Ping = DateTime.Now;
                Utils.Debug.LogHeartbeat("Net", $"Ping sent at {Data.Instance.Ping}, delay since last Pong: {delay.TotalSeconds:F2}s");
            }
                else
                {
                    Data.Instance.SocketMissedHeartbeats++;
                    Utils.Debug.LogHeartbeat("Socket", $"Heartbeat missed #{Data.Instance.SocketMissedHeartbeats}: Last Pong was {delay.TotalSeconds:F2}s ago (timeout: {Config.Net.HeartbeatTimeout}s)");
                }
            }
            else
            {
                Utils.Debug.LogHeartbeat("Socket", "Heartbeat check: Socket is null or not connected");
            }
        }

        private void OnAfterPingChanged(params object[] args)
        {
            DateTime v = (DateTime)args[0];
            Send(new Protocol.Ping(v));
        }

        private void OnBeforeMissedHeartbeatsChanged(params object[] args)
        {
            // befor event: args[0] = old value, args[1] = new value
            int newValue = (int)args[1];
            Utils.Debug.LogHeartbeat("Net", $"Missed heartbeats changing to {newValue} (max allowed: {Config.Net.MaxMissedHeartbeats})");

            if (newValue >= Config.Net.MaxMissedHeartbeats)
            {
                Utils.Debug.LogError("Net", $"Max missed heartbeats reached ({Config.Net.MaxMissedHeartbeats}), marking as disconnected");
                
                if (Data.Instance.Online)
                {
                    Data.Instance.Tip = (UI.Tips.Fly, Localization.Instance.Get("server_disconnected"));
                }
                
                Data.Instance.Online = false;
            }
        }

        #endregion

        #region Event Handlers

        private void OnAfterLoginAccountChanged(params object[] args)
        {
            Utils.Debug.Log("Net", $"LoginAccount changed. Online: {Data.Instance.Online}");
            if (Data.Instance.Online)
            {
                Utils.Debug.Log("Net", "Already online, sending Login protocol");
                Send(new Login(Data.Instance.SelectedAccount));
            }
            else
            {
                Utils.Debug.Log("Net", $"Not online, initiating TCP connection to {Data.Instance.SelectedServer.Ip}:{Data.Instance.SelectedServer.Port}");
                Connect(Data.Instance.SelectedServer.Ip, Data.Instance.SelectedServer.Port);
            }
        }

        private void OnAfterOnlineChanged(params object[] args)
        {
            bool isOnline = (bool)args[0];
            if (isOnline)
            {
                // During reconnect, don't auto-login; let user re-login from Start UI
                bool wasReconnecting = isReconnecting;
                ResetReconnectState();
                
                if (!wasReconnecting && Data.Instance.LoginAccount != null)
                {
                    Send(new Login(Data.Instance.SelectedAccount));
                }
            }
            else
            {
                Utils.Debug.LogWarning("Net", "Online changed to false, triggering auto-reconnect");
                TriggerAutoReconnect();
            }
        }

        private void OnAfterOptionChanged(params object[] args)
        {
            Game.Protocol.Option option = (Game.Protocol.Option)args[0];

            if (option != null && option.lefts != null)
            {
                foreach (var item in option.lefts)
                {
                    if (item.type == Protocol.Option.Item.Type.ToggleGroup && item.data.ContainsKey("ID") && item.data["ID"] == "heartbeat_log")
                    {
                        bool enableHeartbeatLog = item.data.ContainsKey("Value") && item.data["Value"] == "True";
                        Utils.Debug.EnableHeartbeatLog = enableHeartbeatLog;
                        Utils.Debug.Log("Option", $"Heartbeat logging {(enableHeartbeatLog ? "enabled" : "disabled")}");
                    }
                }
            }
        }

        private void OnBeforOptionReturnChanged(params object[] args)
        {
            DateTime o = (DateTime)args[0];
            DateTime v = (DateTime)args[1];
            if ((v - o).TotalSeconds > Math.Pow(0.618, 3))
            {
                Send(new OptionReturn());
            }
            else
            {
                Data.Instance.Tip = (UI.Tips.Fly, Localization.Instance.Get("rate_limit"));
            }
        }

        #endregion

        #region Debug Methods

        private void PrintSendMessage(string name, string json)
        {
            Utils.Debug.Log("Socket", $"[SEND] Protocol: {name}, Json: {json}");
        }

        private void PrintReceiveMessage(Base protocol)
        {
            string json = JsonUtility.ToJson(protocol);
            Utils.Debug.Log("Socket", $"[RECEIVE] Protocol: {protocol.GetType().Name}, Json: {json}");
        }

        #endregion

        #region Auto Reconnect

        private void TriggerAutoReconnect()
        {
            if (Data.Instance.Online)
            {
                Utils.Debug.Log("Reconnect", "Already online, skip auto-reconnect");
                return;
            }

            if (UI.Instance.Current == Config.UI.Start)
            {
                Utils.Debug.Log("Reconnect", "First connection failed in Start UI, not auto-reconnecting");
                return;
            }

            if (isReconnecting)
            {
                Utils.Debug.Log("Reconnect", "Already reconnecting, skip");
                return;
            }

            if (!Config.Net.EnableAutoReconnect)
            {
                Utils.Debug.Log("Reconnect", "Auto-reconnect is disabled");
                return;
            }

            Utils.Debug.Log("Reconnect", "Starting auto-reconnect coroutine");
            isReconnecting = true;
            reconnectAttempts = 0;
            reconnectCoroutine = StartCoroutine(AutoReconnectCoroutine());
        }

        private IEnumerator AutoReconnectCoroutine()
        {
            while (true)
            {
                reconnectAttempts++;

                float delay = Mathf.Min(
                    Config.Net.InitialReconnectDelay * Mathf.Pow(2, reconnectAttempts - 1),
                    Config.Net.MaxReconnectDelay
                );

                Data.Instance.Dark = Localization.Instance.Get(
                    "reconnecting_countdown",
                    reconnectAttempts.ToString(),
                    Mathf.CeilToInt(delay).ToString()
                );

                Utils.Debug.Log("Reconnect", $"Attempt #{reconnectAttempts}, waiting {delay}s");
                yield return new WaitForSeconds(delay);

                Data.Instance.Dark = Localization.Instance.Get("reconnecting_attempt", reconnectAttempts.ToString());

                yield return ConnectCoroutine(
                    Data.Instance.SelectedServer.Ip,
                    Data.Instance.SelectedServer.Port
                );

                if (Data.Instance.Online)
                {
                    Utils.Debug.LogSuccess("Reconnect", $"Reconnect successful after {reconnectAttempts} attempts");
                    Data.Instance.Tip = (UI.Tips.Fly, Localization.Instance.Get("reconnect_success"));
                    ResetReconnectState();
                    
                    // Restart the startup flow to show Start UI for user to re-login
                    StartupFlowManager.Start();
                    yield break;
                }
            }
        }

        public void CancelReconnect()
        {
            if (reconnectCoroutine != null)
            {
                StopCoroutine(reconnectCoroutine);
                ResetReconnectState();
                Data.Instance.Dark = null;
                Utils.Debug.Log("Reconnect", "User cancelled reconnect");
            }
        }

        private void ResetReconnectState()
        {
            isReconnecting = false;
            reconnectAttempts = 0;
            reconnectCoroutine = null;
        }

        #endregion

        private void OnAfterServersChanged(params object[] args)
        {
        }
    }
}
