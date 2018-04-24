using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading.Tasks;

namespace XenoEngine.Network
{
    public delegate void SendFunction(NetworkDataPacket packet);
    public delegate void ReceiveFunction(NetworkDataPacket packet);

    enum NetworkIdentity
    {
        LocalClient
    }

    [Serializable]
    public enum NetworkSendMethod
    {
        InOrder,
        FireForget
    }

    [Serializable]
    public class EntityIDInfo
    {
        public int SendingEntityID { get; set; }
        public int ReceiveEntityID { get; set; }
    }

    [Serializable]
    public class NetworkDataPacket //: ISerializable
    {
        public NetworkDataPacket()
        {
            EntityIDs = new EntityIDInfo();
        }
//        dynamic m_data;
        dynamic m_UserData;

        public EntityIDInfo EntityIDs { get; set; }
        public int PacketID { get; set; }
        public int NetworkID { get; set; }
        public IPAddress IPAddress { get; set; }
        public MessageType MessageType { get; set; }
        public NetworkSendMethod SendMethod { get; set; }
        public string MethodInvoke { get; set; }

//        public NetworkDataPacket() { }

//         protected NetworkDataPacket(SerializationInfo si, StreamingContext context)
//         {
// 
//         }
// 
//         public virtual void GetObjectData(SerializationInfo si, StreamingContext context)
//         {
//             si.AddValue("PacketID", PacketID);
//             si.AddValue("NetworkID", NetworkID);
//             si.AddValue("IPAddress", IPAddress);
//             si.AddValue("MessageType", MessageType);
//             si.AddValue("NetworkSendMethod", SendMethod);
//             si.AddValue("MethodInvoke", MethodInvoke);
// 
//             si.AddValue("ExpandoObject", ((IDictionary)m_data).);
//             si.AddValue("UserData", UserData);
//         }

//         public dynamic Data
//         {
//             get
//             {
//                 if (m_data == null) m_data = new ExpandoObject();
// 
//                 return m_data;
//             }
//         }

        public dynamic UserData
        {
            get { return m_UserData; }
            set { m_UserData = value; }
        }
    }

    public class NetworkSendItem
    {
        public NetworkDataPacket Packet { get; set; }
        public IPAddress IPAdress { get; set; }
    }

    public class ClientConnectedInfo
    {
        public ClientConnectedInfo(int nNetworkID, UdpClient client, IPAddress address)
        {
            Client = client;
            Address = address;
            NetworkID = nNetworkID;
        }

        public int NetworkID { get; set; }
        public UdpClient Client { get; private set; }
        public IPAddress Address { get; private set; }
    }

    //This class is going to wrap a tcp and a udp client so that both connections can be used simultaniously
    public class HybridClient
    {
        private UdpClient                                       m_udpClient;
        private Dictionary<IPAddress, ClientConnectedInfo>      m_UdpClients;
        private Queue<NetworkSendItem>                          m_OrderedSendQueue;
        private int                                             m_AckClientsMask, m_ConnectedClientsMask, m_nUdpPort;

        private MemoryStream                                    m_SendMemoryBuffer, m_ReceiveMemoryBuffer;
        private BinaryFormatter                                 m_formatter;

        private bool                                            m_bAllowConnections, m_bAckReceived;
        private object                                          m_receiveQueueLock, m_SendQueueLock, m_connectionLock, m_AckLock;

        private const int                                       InvalidIndex = -1;

        public event ReceiveFunction                            ReceiveCallback, SystemMessageReceivedCallback;
        public event Action<int>                                ClientConnectedCallback;
        public event Action<int>                                ClientDisconnectedCallback;

        public HybridClient(int nUdpPort)
        {
            m_udpClient = new UdpClient(nUdpPort, AddressFamily.InterNetwork);
            m_UdpClients = new Dictionary<IPAddress, ClientConnectedInfo>();
            m_OrderedSendQueue = new Queue<NetworkSendItem>();

            //Begin receive straight away to kick of the system.
            m_udpClient.BeginReceive(ReceivePacket, null);

            m_SendMemoryBuffer = new MemoryStream();
            m_ReceiveMemoryBuffer = new MemoryStream();
            m_formatter = new BinaryFormatter();

            m_receiveQueueLock = new object();
            m_SendQueueLock = new object();
            m_connectionLock = new object();

            m_nUdpPort = nUdpPort;
        }

        public void FindPeers()
        {
            byte[] data;
            NetworkDataPacket packet = new NetworkDataPacket();
            
            packet.NetworkID = 0;
            packet.MessageType = MessageType.SystemMessage;
            packet.SendMethod = NetworkSendMethod.FireForget;
            packet.MethodInvoke = "LookingForPeers";

            m_udpClient.EnableBroadcast = true;

            data = SerializePacket(packet);
            m_bAllowConnections = true;
            //m_udpClient.BeginSend(data, data.Length, new IPEndPoint(IPAddress.Broadcast, m_nUdpPort), SendCompleteCallback, m_udpClient);
            m_udpClient.BeginSend(data, data.Length, new IPEndPoint(IPAddress.Loopback, m_nUdpPort), SendCompleteCallback, m_udpClient);
            //SendPacket(packet, IPAddress.Broadcast);
            //SendPacket(packet, IPAddress.Loopback);
        }

        public void SendPacket(NetworkDataPacket packet, IPAddress address = null)
        {
            IPEndPoint endPoint = null;

            if(address != null)
                endPoint= new IPEndPoint(address, m_nUdpPort);

            byte[] data = SerializePacket(packet);

//             if (packet.SendMethod == NetworkSendMethod.InOrder)
//             {
//                 NetworkSendItem item = new NetworkSendItem();
//                 item.IPAdress = address;
//                 item.Packet = packet;
// 
//                 lock (m_SendQueueLock)
//                     m_OrderedSendQueue.Enqueue(item);
//             }
//             else
            {
                ClientConnectedInfo clientInfo = m_UdpClients[endPoint.Address];
                clientInfo.Client.BeginSend(data, data.Length, SendCompleteCallback, null);
            }
        }

        public void StartSendProcess()
        {
            Task task = Task.Factory.StartNew(ProcessSendQueue, m_OrderedSendQueue);
        }

        private void ProcessSendQueue(object queue)
        {
            lock(m_SendQueueLock)
            {
                if (m_OrderedSendQueue.Count > 0)
                {
                    while (m_OrderedSendQueue.Count > 0)
                    {
                        NetworkSendItem item = m_OrderedSendQueue.Dequeue();
                        NetworkDataPacket packet = item.Packet;
                        IPEndPoint endpoint = null;
                        
                        byte[] data = SerializePacket(packet);
                        int nConnectionMask;
                        int nCounter = 50;
                        int nSpins = 0;
                        m_AckClientsMask = 0;

                        if (item.IPAdress != null)
                            endpoint = new IPEndPoint(item.IPAdress, m_nUdpPort);

                        if (endpoint == null)
                        {
                            //send the packet to all connected clients.
                            foreach (ClientConnectedInfo clientInfo in m_UdpClients.Values)
                            {
                                clientInfo.Client.BeginSend(data, data.Length, SendCompleteCallback, clientInfo.Client);
                            }

                            //We don't obtain a double nested lock on the AckMask as 
                            //we want the thread to overwrite as necessary 
                            lock (m_connectionLock)
                                nConnectionMask = m_ConnectedClientsMask;

                            //We hold execution here until all acks have come back.
                            while (0 != (m_AckClientsMask ^ nConnectionMask))
                            {
                                ++nSpins;

                                if (nSpins > nCounter)
                                {
                                    //if a certain amount of time passes and not all replies have come back.
                                    // disconnect that client.
                                    int nDisconnectionMask = m_ConnectedClientsMask ^ m_AckClientsMask;

                                    for (int i = 0; i < 32; ++i)
                                    {
                                        if (0 != (m_ConnectedClientsMask & i))
                                        {
                                            DisconnectPeer(i);
                                        }
                                    }
                                }

                                //We need to keep updating this every cycle to check if anything has changed.
                                lock (m_connectionLock)
                                    nConnectionMask = m_ConnectedClientsMask;
                            }
                        }
                        else
                        {
                            //This is if we are only sending an inorder packet to a single client.
                            ClientConnectedInfo clientInfo = m_UdpClients[endpoint.Address];

                            clientInfo.Client.BeginSend(data, data.Length, SendCompleteCallback, clientInfo.Client);

                            //We hold execution here till we get a response..
                            //if we dont we assume the client has disconnected.
                            while (0 != (m_AckClientsMask & clientInfo.NetworkID))
                            {
                                ++nSpins;

                                if (nSpins > nCounter)
                                {
                                    DisconnectPeer(clientInfo.Address);
                                }
                            }
                        }
                    }
                }
            }
        }

        private void SendAck(IPAddress address)
        {
            NetworkDataPacket packet = new NetworkDataPacket();

            packet.MessageType = MessageType.SystemMessage;
            packet.MethodInvoke = "ReceiveAck";
            packet.SendMethod = NetworkSendMethod.FireForget;

            SendPacket(packet, address);
        }

        private void ReceiveAck(NetworkDataPacket packet)
        {
            int nMaskIndex = m_UdpClients[packet.IPAddress].NetworkID;

            m_AckClientsMask |= nMaskIndex;
        }

        public void ReceivePacket(IAsyncResult msg)
        {
            NetworkDataPacket packet = null;
            IPEndPoint endPoint = new IPEndPoint(0, 0);
            
            byte[] data = m_udpClient.EndReceive(msg, ref endPoint);
            
            packet = Deserialize(data);

            packet.IPAddress = endPoint.Address;
            packet.NetworkID = m_UdpClients[endPoint.Address].NetworkID;

            if (packet.SendMethod == NetworkSendMethod.InOrder)
            {
                SendAck(endPoint.Address);
            }

            //we divert it at this point based on if its a system or entity message
            if (packet.MessageType == MessageType.SystemMessage)
            {
                ProcessSystemMessage(packet);
            }
            else
            {
                if (ReceiveCallback != null)
                    ReceiveCallback(packet);
            }

            m_udpClient.BeginReceive(ReceivePacket, null);
        }
        
        private int FindAvailableIndex()
        {
            int nResult = InvalidIndex;
            //I use 32 because that it the size of an int in bits.
            for (int i = 1; i < 32; ++i)
            {
                if (0 == (m_ConnectedClientsMask & i))
                {
                    nResult = i;
                    break;
                }
            }

            return nResult;
        }

        private void LookingForPeers(NetworkDataPacket packet)
        {
            if (m_bAllowConnections)
            {
                byte[] data;
                int nMaskIndex = FindAvailableIndex();
                
                if(nMaskIndex != InvalidIndex)
                {
                    NetworkDataPacket replyPacket = new NetworkDataPacket();
                    UdpClient newClient = new UdpClient();
                    newClient.Connect(new IPEndPoint(packet.IPAddress, m_nUdpPort));
                    m_UdpClients.Add(packet.IPAddress, new ClientConnectedInfo(nMaskIndex, newClient, packet.IPAddress));

                    lock(m_connectionLock)
                        m_ConnectedClientsMask |= nMaskIndex;

                    if (ClientConnectedCallback != null) ClientConnectedCallback(nMaskIndex);
                
                    replyPacket.MessageType = MessageType.SystemMessage;
                    replyPacket.SendMethod = NetworkSendMethod.FireForget;
                    replyPacket.MethodInvoke = "PeerConnectionReply";
                
                    data = SerializePacket(replyPacket);
                
                    newClient.BeginSend(data, data.Length, SendCompleteCallback, newClient);
                }
            }
        }

        private void PeerConnectionReply(NetworkDataPacket packet)
        {
            int nMaskIndex = FindAvailableIndex();

            if(nMaskIndex != InvalidIndex)
            {
                UdpClient newClient = new UdpClient(new IPEndPoint(packet.IPAddress, m_nUdpPort));
                m_UdpClients.Add(packet.IPAddress, new ClientConnectedInfo(nMaskIndex, newClient, packet.IPAddress));
            
                lock(m_connectionLock)
                    m_ConnectedClientsMask |= nMaskIndex;

                if (ClientConnectedCallback != null) ClientConnectedCallback(nMaskIndex);
            }
        }

        public void DisconnectPeer(IPAddress address)
        {
            ClientConnectedInfo clientInfo = m_UdpClients[address];
            int nMaskIndex = clientInfo.NetworkID;

            m_UdpClients.Remove(address);
            clientInfo.Client.Close();

            lock(m_connectionLock)
                m_ConnectedClientsMask &= ~nMaskIndex;

            if (ClientDisconnectedCallback != null) ClientDisconnectedCallback(nMaskIndex);
        }

        public void DisconnectPeer(int nMaskIndex)
        {
            ClientConnectedInfo client = null;
            //Unfortunately we have to a linear search in order to find it this way.
            foreach (ClientConnectedInfo clientInfo in m_UdpClients.Values)
            {
                if (clientInfo.NetworkID == nMaskIndex)
                {
                    client = clientInfo;
                    break;
                }
            }

            if (client != null)
            {
                m_UdpClients.Remove(client.Address);
                client.Client.Close();

                lock (m_connectionLock)
                    m_ConnectedClientsMask &= ~nMaskIndex;

                if (ClientDisconnectedCallback != null) ClientDisconnectedCallback(nMaskIndex);
            }
        }

        private bool ProcessSystemMessage(NetworkDataPacket packet)
        {
            bool bResult = false;
            Type systemType = typeof(HybridClient);

            MethodInfo method = systemType.GetMethod(
                packet.MethodInvoke, 
                BindingFlags.Instance | 
                BindingFlags.NonPublic |
                BindingFlags.Public);

            //if we find the message call...the we handle it. it not we pass it on to the next section.
            if (method != null)
            {
                method.Invoke(this, new object[] { packet });
                bResult = true;
            }
            else
            {
                if (SystemMessageReceivedCallback != null) SystemMessageReceivedCallback(packet);
            }

            return bResult;
        }

        private void SendCompleteCallback(IAsyncResult client) { ((UdpClient)client.AsyncState).EndSend(client); }

        private byte[] SerializePacket(NetworkDataPacket packet)
        {
            //We reset the buffer back to the start.
            try
            {
                m_SendMemoryBuffer.Position = 0;
                m_formatter.Serialize(m_SendMemoryBuffer, packet);
                m_SendMemoryBuffer.Flush();
            }
            catch (System.Exception ex) { Debug.Assert(false, "a network packet failed to serialize"); }

            return Compress(m_SendMemoryBuffer.ToArray());
        }

        private NetworkDataPacket Deserialize(byte[] data)
        {
            NetworkDataPacket packet = null;
            m_ReceiveMemoryBuffer.Position = 0;

            try
            {
                byte[] decompressData = Decompress(data);
                m_ReceiveMemoryBuffer.Write(decompressData, 0, decompressData.Length);
                m_ReceiveMemoryBuffer.Flush();
                m_ReceiveMemoryBuffer.Seek(0, SeekOrigin.Begin);
                m_ReceiveMemoryBuffer.Position = 0;

                packet = (NetworkDataPacket)m_formatter.Deserialize(m_ReceiveMemoryBuffer);
            }
            catch (System.Exception ex)
            {
                Debug.Assert(false);
            }

            return packet;
        }

        private byte[] Compress(byte[] data)
        {
            byte[] returnData;
            var memStream = new MemoryStream();
            var compressStream = new DeflateStream(memStream, CompressionMode.Compress, true);
            compressStream.Write(data, 0, data.Length);
            compressStream.Dispose();
            memStream.Seek(0, SeekOrigin.Begin);
            returnData = memStream.ToArray();
            memStream.Dispose();

            return returnData;
        }

        private byte[] Decompress(byte[] data)
        {
            byte[] writeData = new byte[4096];
            byte[] returnData;
            var memStream = new MemoryStream();
            var decompressStream = new DeflateStream(new MemoryStream(data), CompressionMode.Decompress, true);
            int nSize;

            while ((nSize = decompressStream.Read(writeData, 0, writeData.Length)) > 0)
            {
                memStream.Write(writeData, 0, nSize);
            }

            decompressStream.Dispose();
            memStream.Flush();

            returnData = memStream.ToArray();

            memStream.Dispose();

            return returnData;
        }

        public UdpClient LocalUdpClient { get { return m_udpClient; } }
    }

    public enum MessageType
    {
        EntityMessage,
        EntitySystemMessage,
        SystemMessage,
        CustomMessage
    }

    public class NetworkEngine
    {
        //turn this into an interface.
        private static HybridClient             m_hybridclient;
        private static List<NetworkDataPacket>  m_ReceivedPackets;
        private static object                   m_ReceiveListLock;
        private static int                      m_nNumberOfClients;
        private static bool                     m_bInitialised;

        public ReceiveFunction                  MessageReceived;
        public event Action<int>                ClientConnected;
        public event Action<int>                ClientDisconnected;
        private bool                            m_bQueueReceivedMessages;

        public bool IsNetworkActive { get { return m_bInitialised && m_nNumberOfClients > 0; } }

        public void InitializeNetwork(int nUdpPort, bool bQueueMessages)
        {
            m_bQueueReceivedMessages = bQueueMessages;
            m_hybridclient = new HybridClient(nUdpPort);
            m_hybridclient.ReceiveCallback += QueueMessage;
            m_hybridclient.SystemMessageReceivedCallback += ProcessSystemMessage;
            m_hybridclient.ClientConnectedCallback += ProcessClientConnection;
            m_hybridclient.ClientDisconnectedCallback += ProcessClientDisconnection;
            m_ReceivedPackets = new List<NetworkDataPacket>();
            m_ReceiveListLock = new object();
            m_bInitialised = true;
        }

        private void ProcessClientConnection(int nMaskIndex)
        {
            m_nNumberOfClients++;
            if (ClientConnected != null) ClientConnected(nMaskIndex);
        }

        private void ProcessClientDisconnection(int nMaskIndex)
        {
            m_nNumberOfClients--;
            if (ClientDisconnected != null) ClientDisconnected(nMaskIndex);
        }

        public void BroadCast()
        {
            m_hybridclient.FindPeers();
        }

        public void SendMessage(NetworkDataPacket packet, IPAddress address = null)
        {
            m_hybridclient.SendPacket(packet, address);
        }

        public void ProcessSendQueue()
        {
            m_hybridclient.StartSendProcess();
        }

        public void QueueMessage(NetworkDataPacket packet)
        {
            if (m_bQueueReceivedMessages)
            {
                lock (m_ReceiveListLock)
                    m_ReceivedPackets.Add(packet);
            }
            else
            {
                if (MessageReceived != null) MessageReceived(packet);
            }
        }

        //Doing this avoid threading issues.
        //with the entity system...otherwise 
        //we would just pass the the message directly through 
        public void ProcessReceivedMessages()
        {
            if(m_bQueueReceivedMessages)
            {
                lock (m_ReceiveListLock)
                {
                    if (m_ReceivedPackets.Count > 0)
                    {
                        foreach (NetworkDataPacket packet in m_ReceivedPackets)
                        {
                            if (MessageReceived != null) MessageReceived(packet);
                        }
                    }
                }
            }
        }

        public void ProcessSystemMessage(NetworkDataPacket packet)
        {
            Type type = typeof(NetworkEngine);

            MethodInfo method = type.GetMethod(
                packet.MethodInvoke,
                BindingFlags.Instance |
                BindingFlags.NonPublic |
                BindingFlags.Public);

            if (method != null)
            {
                method.Invoke(this, new object[] { packet });
            }
            else
            {
                Debug.Assert(false, "Failed to find a system message for " + packet.MethodInvoke);
            }
        }
    }
}
