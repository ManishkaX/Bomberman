using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Collections.Concurrent;
using System.Text.RegularExpressions;
using Server.Messaging;
using System.Text.Json;

namespace Server
{
    public static class ServerModel
    {
        private const string _swkGUID = "258EAFA5-E914-47DA-95CA-C5AB0DC85B11";

        private static bool _isDebug = false;

        private static Socket? _sListener;

        private static ConcurrentDictionary<int, User> _users = new ConcurrentDictionary<int, User>();


        public static void Start(string dns, int port)
        {
            try
            {
                IPHostEntry ipHost = Dns.GetHostEntry(dns);
                IPAddress ipAddr = ipHost.AddressList[0];
                IPEndPoint ipEndPoint = new IPEndPoint(ipAddr, port);

                _sListener = new Socket(ipAddr.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

                _sListener.Bind(ipEndPoint);
                _sListener.Listen(10);

                AcceptNewClients(ipEndPoint);
            }
            catch (Exception ex)
            {
                ShowError(ex);
            }
            finally
            {
                foreach (var handler in _users.Select(u => u.Value.Socket))
                    CloseClient(handler);

            }
        }

        private static void SendMessage(Socket handler, Message message) =>
            handler.Send(EncodeReply(message.ToByteArray()));

        private static void SendMessage(Socket handler, string json) =>
            handler.Send(EncodeReply(Encoding.UTF8.GetBytes(json)));

        public static void SetDebugMode(bool isDebug) => _isDebug = isDebug;

        private static void AcceptNewClients(IPEndPoint ipEndPoint)
        {
            Task.Run(() =>
            {
                try
                {
                    while (true)
                    {
                        ShowDebug($"Waiting for a connection through {ipEndPoint}");

                        Socket handler = _sListener.Accept();

                        ShowDebug("Handler created");

                        //_clientsHandlers.Enqueue(handler);

                        var id = User.GetNewId();
                        _users.TryAdd(id, new User(handler, id));

                        Task.Run(() => ProcessClientRequests(handler));
                    }
                }
                catch (Exception ex)
                {
                    ShowError(ex);
                    return;
                }
            });
        }

        private static void ProcessClientRequests(Socket handler)
        {
            try
            {
                ShowDebug("Processing started");

                while (true)
                {
                    byte[] requestBin = new byte[1024];
                    int requestByteCount = handler.Receive(requestBin);

                    string request = Encoding.UTF8.GetString(requestBin, 0, requestByteCount);

                    if (Regex.IsMatch(request, "^GET", RegexOptions.IgnoreCase))
                    {
                        HandshakingWithClient(handler, request);
                        continue;
                    }

                    string text = DecodeRequest(requestBin);
                    if (text != string.Empty)
                        Console.WriteLine($"RECIEVED: {text}");

                    switch (text[15])
                    {
                        case '0':
                            int id = _users.Where(u => u.Value.Socket == handler)
                                .First().Value.UserID;

                            foreach (var user in _users.Select(u => u.Value))
                            {
                                var reply = (ConnectionMessage?)JsonSerializer.Deserialize(text, typeof(ConnectionMessage));
                                reply.UserID = id;
                                SendMessage(user.Socket, reply);

                                if (user.UserID != id)
                                    continue;

                                foreach (var block in Map.SoftBlocks)
                                {
                                    var message = new ModifyBlockMessage(block.X, block.Y, false);
                                    SendMessage(user.Socket, message);
                                }
                            }

                            var usersList = _users.Select(u => u.Value).ToList();
                            SendMessage(_users[id].Socket, new NotifyMessage(usersList));

                            break;

                        case '1':
                            foreach (var user in _users.Select(u => u.Value))
                            {
                                if (!user.Socket.Equals(handler))
                                    SendMessage(user.Socket, text);
                                else
                                    _users.TryRemove(user.UserID, out var _);
                            }

                            break;

                        case '2':
                            foreach (var user in _users.Select(u => u.Value))
                            {
                                if (user.Socket.Equals(handler))
                                {
                                    var message = (MoveMessage?)JsonSerializer.Deserialize(text, typeof(MoveMessage));

                                    user.PositionX = message.PositionX;
                                    user.PositionY = message.PositionY;
                                }
                                else 
                                    SendMessage(user.Socket, text);
                            }
                            break;

                        case '3':
                            var modMessage = (ModifyBlockMessage?)JsonSerializer.Deserialize(text, typeof(ModifyBlockMessage));
                            Console.WriteLine(modMessage);
                            Map.MapTemplate[modMessage.PositionX, modMessage.PositionY] = Map.CellTypes.Empty;
                            
                            
                            Map.SoftBlocks.Remove(new System.Drawing.Point(modMessage.PositionX, modMessage.PositionY));

                            foreach (var user in _users.Select(u => u.Value))
                            {
                                if (user.Socket.Equals(handler))
                                    continue;
                                    
                                SendMessage(user.Socket, text);

                            }
                            break;

                        case '4':
                            foreach (var user in _users.Select(u => u.Value))
                            {
                                if (user.Socket.Equals(handler))
                                    continue;

                                SendMessage(user.Socket, text);
                            }

                            break;
                    }

                    if (!IsSocketConnected(handler))
                        throw new Exception("Client has been disconnected");
                }
            }
            catch (Exception ex)
            {
                ShowError(ex);
                CloseClient(handler);
                return;
            }
        }

        private static void CloseClient(Socket handler)
        {
            try
            {
                string reply = "--exit";
                byte[] msg = Encoding.UTF8.GetBytes(reply);
                handler.Send(msg);

                handler.Shutdown(SocketShutdown.Both);
                handler.Close();

                ShowDebug("Handler closed");
            }
            catch (Exception ex)
            {
                ShowError(ex);
                return;
            }
        }

        // 1. Obtain the value of the "Sec-WebSocket-Key" request header without any leading or trailing whitespace
        // 2. Concatenate it with "258EAFA5-E914-47DA-95CA-C5AB0DC85B11" (a special GUID specified by RFC 6455)
        // 3. Compute SHA-1 and Base64 hash of the new value
        // 4. Write the hash back as the value of "Sec-WebSocket-Accept" response header in an HTTP response
        private static void HandshakingWithClient(Socket handler, string clientHandshake)
        {
            ShowDebug($"Handshaking from client\n{clientHandshake}");

            string swk = Regex.Match(clientHandshake, "Sec-WebSocket-Key: (.*)").Groups[1].Value.Trim();
            string swka = swk + _swkGUID;
            byte[] swkaSha1 = System.Security.Cryptography.SHA1.Create().ComputeHash(Encoding.UTF8.GetBytes(swka));
            string swkaSha1Base64 = Convert.ToBase64String(swkaSha1);

            // HTTP/1.1 defines the sequence CR LF as the end-of-line marker
            string responseString =
                $"HTTP/1.1 101 Switching Protocols\r\nConnection: Upgrade\r\nUpgrade: websocket\r\nSec-WebSocket-Accept: {swkaSha1Base64}\r\n\r\n";
            byte[] response = Encoding.UTF8.GetBytes(responseString);

            ShowDebug($"Handshaking from server\n{responseString}");

            handler.Send(response);
        }

        public static byte[] EncodeReply(byte[] bytesRaw)
        {
            int newLength = bytesRaw.Length;
            if (bytesRaw.Length <= 125)
                newLength += 2;
            else if (bytesRaw.Length >= 126 && bytesRaw.Length <= 65535)
                newLength += 4;
            else
                newLength += 10;

            byte[] bytesFormatted = new byte[newLength];

            bytesFormatted[0] = 129;

            int offset = 0; // it doesn't matter what value is set here - it will be set now:

            if (bytesRaw.Length <= 125)
            {
                bytesFormatted[1] = (byte)bytesRaw.Length;

                offset = 2;
            }
            else if (bytesRaw.Length >= 126 && bytesRaw.Length <= 65535)
            {
                bytesFormatted[1] = 126;
                bytesFormatted[2] = (byte)((bytesRaw.Length >> 8) & 255);
                bytesFormatted[3] = (byte)((bytesRaw.Length) & 255);

                offset = 4;
            }

            // put raw data at the correct index
            Buffer.BlockCopy(bytesRaw, 0, bytesFormatted, offset, bytesRaw.Length);

            return bytesFormatted;
        }

        public static string DecodeRequest(byte[] requestBin)
        {
            //bool fin = (requestBin[0] & 0b10000000) != 0;
            bool mask = (requestBin[1] & 0b10000000) != 0; // must be true, "All messages from the client to the server have this bit set"
                                                           //int opcode = requestBin[0] & 0b00001111; // expecting 1 - text message
            ulong offset = 2;
            ulong msglen = (ulong)(requestBin[1] & 0b01111111);

            if (msglen == 126)
            {
                // bytes are reversed because websocket will print them in Big-Endian, whereas
                // BitConverter will want them arranged in little-endian on windows
                msglen = BitConverter.ToUInt16(new byte[] { requestBin[3], requestBin[2] }, 0);
                offset = 4;
            }

            if (mask)
            {
                byte[] decoded = new byte[msglen];
                byte[] masks = new byte[4]
                {
                    requestBin[offset],
                    requestBin[offset + 1],
                    requestBin[offset + 2],
                    requestBin[offset + 3]
                };
                offset += 4;

                for (ulong i = 0; i < msglen; ++i)
                    decoded[i] = (byte)(requestBin[offset + i] ^ masks[i % 4]);

                return Encoding.UTF8.GetString(decoded);
            }
            else
            {
                ShowDebug("Bad request: mask bit not set");
                return string.Empty;
            }
        }

        private static bool IsSocketConnected(Socket s) => !(s.Poll(1000, SelectMode.SelectRead) && s.Available == 0);

        private static void ShowError(Exception ex) => Console.WriteLine($"\nEXCEPTION: {ex.ToString()}\n");

        private static void ShowDebug(string s, string? owner = null)
        {
            if (_isDebug)
                Console.WriteLine(owner == null ? $"\nDEBUG: {s}\n" : $"\nDEBUG: {owner}: {s}\n");
        }
    }
}
