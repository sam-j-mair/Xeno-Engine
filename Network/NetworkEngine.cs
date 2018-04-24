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
    //----------------------------------------------------------------------------------
    // public delegates that allow the client to hook into and override send and receive functionality.
    //----------------------------------------------------------------------------------
    public delegate void SendFunction(NetworkDataPacket packet);
    public delegate void ReceiveFunction(NetworkDataPacket packet);
    //----------------------------------------------------------------------------------
    /// <summary>
    /// Enum for describe the different categories of messages.
    /// </summary>
    //----------------------------------------------------------------------------------
    public enum MessageType
    {
        EntityMessage,
        EntitySystemMessage,
        SystemMessage,
        CustomMessage
    }
    //----------------------------------------------------------------------------------
    // enum for setting whether a packet is sent in order with ACK(TCP) response or UDP style fire and forget.
    //----------------------------------------------------------------------------------
    [Serializable]
    public enum NetworkSendMethod
    {
        InOrder,
        FireForget
    }
    //----------------------------------------------------------------------------------
    //  enum for defining local and remote connected clients.
    //----------------------------------------------------------------------------------
    public enum NetworkIdentity
    {
        LocalClient,
        RemoteClient
    }
    
    //----------------------------------------------------------------------------------
    // A serializable packet header for storing sending and receiving IDs
    //----------------------------------------------------------------------------------
    [Serializable]
    public class EntityIDInfo
    {
        public int SendingEntityID { get; set; }
        public int ReceiveEntityID { get; set; }
    }
    //----------------------------------------------------------------------------------
    /// <summary>
    /// NetworkDataPacket for sending messages across network.
    /// </summary>
    //----------------------------------------------------------------------------------
    [Serializable]
    public class NetworkDataPacket
    {
        //private members
        private dynamic m_UserData;
        
        //contructor.
        public NetworkDataPacket()
        {
            EntityIDs = new EntityIDInfo();
        }

        public dynamic UserData
        {
            get { return m_UserData; }
            set { m_UserData = value; }
        }

        //public properties
        public int                  PacketID { get; set; }
        public int                  NetworkID { get; set; }
        public EntityIDInfo         EntityIDs { get; set; }
        public IPAddress            IPAddress { get; set; }
        public MessageType          MessageType { get; set; }
        public NetworkSendMethod    SendMethod { get; set; }
        public string               MethodInvoke { get; set; }
    }
    //----------------------------------------------------------------------------------
    /// <summary>
    /// SendItem for putting in the network queue.
    /// </summary>
    //----------------------------------------------------------------------------------
    public class NetworkSendItem
    {
        public NetworkDataPacket Packet { get; set; }
        public IPAddress IPAdress { get; set; }
    }
    //----------------------------------------------------------------------------------
    /// <summary>
    /// Stores info for each client connected to the network.
    /// </summary>
    //----------------------------------------------------------------------------------
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
    //----------------------------------------------------------------------------------
    /// <summary>
    /// HybridClient implements the network interface and has implemented both a TCP and UDP connection
    /// whilst avoiding the overhead of using TCP.
    /// </summary>
    //----------------------------------------------------------------------------------
    public class HybridClient
    {
        //private members
        private UdpClient                                   m_udpClient;
        //store for all connected clients.
        private Dictionary<IPAddress, ClientConnectedInfo>  m_UdpClients;
        //network send queue.
        private Queue<NetworkSendItem>                      m_OrderedSendQueue;
        //allow and block new connections.
        private bool                                        m_bAllowConnections;
        //an invalid index for connected clients(default)
        private const int                                   InvalidIndex = -1;

        //for packet serialization.
        private BinaryFormatter                             m_formatter;
        //for writting packets.
        private MemoryStream                                m_SendMemoryBuffer, 
                                                            m_ReceiveMemoryBuffer;

        //locking objects for controlling access to data over multiple threads.
        private object                                      m_receiveQueueLock, 
                                                            m_SendQueueLock, 
                                                            m_connectionLock, 
                                                            m_AckLock;

        //masks for check ACKs as a well as connected clients.
        private int                                         m_AckClientsMask, 
                                                            m_ConnectedClientsMask, 
                                                            m_nUdpPort;

        
        //events for when a message is received and also for SystemMessage for the entity system.
        public event ReceiveFunction                        ReceiveCallback, 
                                                            SystemMessageReceivedCallback;

        //client callbacks to inform the client when it is connected
        //or when is get disconnected.
        public event Action<int>                            ClientConnectedCallback;
        public event Action<int>                            ClientDisconnectedCallback;
        //----------------------------------------------------------------------------------
        /// <summary>
        /// contructor.
        /// </summary>
        /// <param name="nUdpPort">the port the network will use to connect on.</param>
        //----------------------------------------------------------------------------------
        public HybridClient(int nUdpPort)
        {
            //initialise members.
            m_udpClient = new UdpClient(nUdpPort, AddressFamily.InterNetwork);
            m_UdpClients = new Dictionary<IPAddress, ClientConnectedInfo>();
            m_OrderedSendQueue = new Queue<NetworkSendItem>();

            //Begin receive straight away to kick of the system.
            m_udpClient.BeginReceive(ReceivePacket, null);

            //Initilize buffers
            m_SendMemoryBuffer = new MemoryStream();
            m_ReceiveMemoryBuffer = new MemoryStream();
            m_formatter = new BinaryFormatter();

            m_receiveQueueLock = new object();
            m_SendQueueLock = new object();
            m_connectionLock = new object();

            m_nUdpPort = nUdpPort;
        }
        //----------------------------------------------------------------------------------
        /// <summary>
        /// locate and return broadcasting peers for the network.
        /// </summary>
        //----------------------------------------------------------------------------------
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
            m_udpClient.BeginSend(data,
                                  data.Length,
                                  new IPEndPoint(IPAddress.Loopback, m_nUdpPort),
                                  SendCompleteCallback,
                                  m_udpClient);
        }
        //----------------------------------------------------------------------------------
        /// <summary>
        /// sends network data packet message across the network.
        /// </summary>
        /// <param name="packet">the packet to be sent.</param>
        /// <param name="address">the address of the client that the packet is to be sent to.</param>
        //----------------------------------------------------------------------------------
        public void SendPacket(NetworkDataPacket packet, IPAddress address = null)
        {
            IPEndPoint endPoint = null;
            ClientConnectedInfo clientInfo;
            byte[] data;

            if (address != null)
                endPoint = new IPEndPoint(address, m_nUdpPort);

            data = SerializePacket(packet);

            clientInfo = m_UdpClients[endPoint.Address];
            clientInfo.Client.BeginSend(data,
                                        data.Length,
                                        SendCompleteCallback,
                                        null);
        }
        //----------------------------------------------------------------------------------
        /// <summary>
        /// Kicks off network send task on another thread.
        /// </summary>
        //----------------------------------------------------------------------------------
        public void StartSendProcess()
        {
            Task task = Task.Factory.StartNew(ProcessSendQueue, m_OrderedSendQueue);
        }
        //----------------------------------------------------------------------------------
        /// <summary>
        /// Processes the send queue and send all queued packets for the network. NOTE: This is done on a seperate thread.
        /// </summary>
        /// <param name="queue"></param>
        //----------------------------------------------------------------------------------
        private void ProcessSendQueue(object queue)
        {
            lock (m_SendQueueLock)
            {
                if (m_OrderedSendQueue.Count > 0)
                {
                    while (m_OrderedSendQueue.Count > 0)
                    {
                        NetworkSendItem item = m_OrderedSendQueue.Dequeue();
                        NetworkDataPacket packet = item.Packet;
                        IPEndPoint endpoint = null;

                        //serialise the packet to be sent.
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
                                clientInfo.Client.BeginSend(data,
                                                            data.Length, 
                                                            SendCompleteCallback, 
                                                            clientInfo.Client);
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

                            clientInfo.Client.BeginSend(data,
                                                        data.Length, 
                                                        SendCompleteCallback, 
                                                        clientInfo.Client);

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
        //----------------------------------------------------------------------------------
        /// <summary>
        /// For the receiving client to send an ACK when a packet is sent in order.
        /// </summary>
        /// <param name="address">Address to send the ACK back to.</param>
        //----------------------------------------------------------------------------------
        private void SendAck(IPAddress address)
        {
            NetworkDataPacket packet = new NetworkDataPacket();

            packet.MessageType = MessageType.SystemMessage;
            packet.MethodInvoke = "ReceiveAck";
            packet.SendMethod = NetworkSendMethod.FireForget;

            SendPacket(packet, address);
        }
        //----------------------------------------------------------------------------------
        /// <summary>
        /// Update ACK mask when an ACK is received.
        /// </summary>
        /// <param name="packet"></param>
        //----------------------------------------------------------------------------------
        private void ReceiveAck(NetworkDataPacket packet)
        {
            int nMaskIndex = m_UdpClients[packet.IPAddress].NetworkID;

            //OR the flags together.
            m_AckClientsMask |= nMaskIndex;
        }
        //----------------------------------------------------------------------------------
        /// <summary>
        /// Receive message processing.
        /// </summary>
        /// <param name="msg"></param>
        //----------------------------------------------------------------------------------
        public void ReceivePacket(IAsyncResult msg)
        {
            NetworkDataPacket packet = null;
            IPEndPoint endPoint = new IPEndPoint(0, 0);

            byte[] data = m_udpClient.EndReceive(msg, ref endPoint);

            packet = Deserialize(data);

            packet.IPAddress = endPoint.Address;
            packet.NetworkID = m_UdpClients[endPoint.Address].NetworkID;

            //if packet is sent InOrder then an ACK needs to be sent.
            if (packet.SendMethod == NetworkSendMethod.InOrder)
            {
                SendAck(endPoint.Address);
            }

            //we divert it at this point based on if its a system for the EntityController ie. Delete or Create or an entity message
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
        //----------------------------------------------------------------------------------
        /// <summary>
        /// Finds the next available index in the connection mask for the next client to connect.
        /// </summary>
        /// <returns>index</returns>
        //----------------------------------------------------------------------------------
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
        //----------------------------------------------------------------------------------
        /// <summary>
        /// Broadcast to any available clients looking to connect.
        /// </summary>
        /// <param name="packet">packet data.</param>
        //----------------------------------------------------------------------------------
        private void LookingForPeers(NetworkDataPacket packet)
        {
            //check if connections are allowed
            if (m_bAllowConnections)
            {
                byte[] data;
                //find first available connection index.
                int nMaskIndex = FindAvailableIndex();

                //ensure the index return is a valid index entry.
                if (nMaskIndex != InvalidIndex)
                {
                    NetworkDataPacket replyPacket = new NetworkDataPacket();
                    UdpClient newClient = new UdpClient();
                    newClient.Connect(new IPEndPoint(packet.IPAddress, m_nUdpPort));

                    m_UdpClients.Add(packet.IPAddress,
                                     new ClientConnectedInfo(nMaskIndex,
                                                             newClient,
                                                             packet.IPAddress)
                                     );

                    //we lock here to avoid race conditions with the packet proc.
                    lock (m_connectionLock)
                        m_ConnectedClientsMask |= nMaskIndex;

                    if (ClientConnectedCallback != null) ClientConnectedCallback(nMaskIndex);

                    //set up the reply packet
                    replyPacket.MessageType = MessageType.SystemMessage;
                    replyPacket.SendMethod = NetworkSendMethod.FireForget;
                    replyPacket.MethodInvoke = "PeerConnectionReply";

                    data = SerializePacket(replyPacket);

                    newClient.BeginSend(data, data.Length, SendCompleteCallback, newClient);
                }
            }
        }
        //----------------------------------------------------------------------------------
        /// <summary>
        /// This is the reply to a client looking for peers.
        /// </summary>
        /// <param name="packet">Received packet</param>
        //----------------------------------------------------------------------------------
        private void PeerConnectionReply(NetworkDataPacket packet)
        {
            //get connection mask index
            int nMaskIndex = FindAvailableIndex();

            //ensure its a valid index.
            if (nMaskIndex != InvalidIndex)
            {
                UdpClient newClient = new UdpClient(new IPEndPoint(packet.IPAddress, m_nUdpPort));
                m_UdpClients.Add(packet.IPAddress, new ClientConnectedInfo(nMaskIndex, newClient, packet.IPAddress));

                //lock wil updating mask to avoid race conditions.
                lock (m_connectionLock)
                    m_ConnectedClientsMask |= nMaskIndex;

                if (ClientConnectedCallback != null) ClientConnectedCallback(nMaskIndex);
            }
        }
        //----------------------------------------------------------------------------------
        /// <summary>
        /// Kick a peer of the network.
        /// </summary>
        /// <param name="address">address of peer to be kicked.</param>
        //----------------------------------------------------------------------------------
        public void DisconnectPeer(IPAddress address)
        {
            ClientConnectedInfo clientInfo = m_UdpClients[address];
            int nMaskIndex = clientInfo.NetworkID;

            //remove address from connections.
            m_UdpClients.Remove(address);
            clientInfo.Client.Close();

            //update connected mask.
            lock (m_connectionLock)
                m_ConnectedClientsMask &= ~nMaskIndex;

            //invoke callback to tell the local machine of event.
            if (ClientDisconnectedCallback != null) ClientDisconnectedCallback(nMaskIndex);
        }
        //----------------------------------------------------------------------------------
        /// <summary>
        /// Overridden function to disconnect by mask index instead.
        /// </summary>
        /// <param name="nMaskIndex">index of peer to be kicked.</param>
        //----------------------------------------------------------------------------------
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
        //----------------------------------------------------------------------------------
        /// <summary>
        /// Process system message for the EntityController.
        /// </summary>
        /// <param name="packet">received packet data.</param>
        /// <returns></returns>
        //----------------------------------------------------------------------------------
        private bool ProcessSystemMessage(NetworkDataPacket packet)
        {
            bool bResult = false;
            Type systemType = typeof(HybridClient);

            //using reflection.
            MethodInfo method = systemType.GetMethod(
                packet.MethodInvoke,
                BindingFlags.Instance |
                BindingFlags.NonPublic |
                BindingFlags.Public);

            //if we find the message call...then we handle it. it not we pass it on to the local device for further handling.
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
        //----------------------------------------------------------------------------------
        /// <summary>
        /// Stop sending callback
        /// </summary>
        /// <param name="client"></param>
        //----------------------------------------------------------------------------------
        private void SendCompleteCallback(IAsyncResult client) { ((UdpClient)client.AsyncState).EndSend(client); }
        //----------------------------------------------------------------------------------
        /// <summary>
        /// Serialize packet in binary format.
        /// </summary>
        /// <param name="packet">packet to be serialized</param>
        /// <returns></returns>
        //----------------------------------------------------------------------------------
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
        //----------------------------------------------------------------------------------
        /// <summary>
        /// Deserilize a reveived packet.
        /// </summary>
        /// <param name="data">data to be deserialized.</param>
        /// <returns>NetworkDataPacket</returns>
        //----------------------------------------------------------------------------------
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
        //----------------------------------------------------------------------------------
        /// <summary>
        /// Compress data packet for sending over network.
        /// </summary>
        /// <param name="data">data to be compressed.</param>
        /// <returns></returns>
        //----------------------------------------------------------------------------------
        private byte[] Compress(byte[] data)
        {
            byte[] returnData;
            using (var memStream = new MemoryStream())
            {
                using (var compressStream = new DeflateStream(memStream, CompressionMode.Compress, true))
                {
                    compressStream.Write(data, 0, data.Length);
                }

                memStream.Seek(0, SeekOrigin.Begin);
                returnData = memStream.ToArray();
            }

            return returnData;
        }
        //----------------------------------------------------------------------------------
        /// <summary>
        /// Decompress data packet for sending over network.
        /// </summary>
        /// <param name="data">data to be decompressed.</param>
        /// <returns></returns>
        //----------------------------------------------------------------------------------
        private byte[] Decompress(byte[] data)
        {
            byte[] writeData = new byte[4096];
            byte[] returnData;
            using (var memStream = new MemoryStream())
            {
                using (var decompressStream = new DeflateStream(new MemoryStream(data), CompressionMode.Decompress, true))
                {
                    int nSize;

                    while ((nSize = decompressStream.Read(writeData, 0, writeData.Length)) > 0)
                    {
                        memStream.Write(writeData, 0, nSize);
                    }
                }

                memStream.Flush();
                returnData = memStream.ToArray();
            }

            return returnData;
        }
        //----------------------------------------------------------------------------------
        /// <summary>
        /// returns the local udpclient.
        /// </summary>
        //----------------------------------------------------------------------------------
        public UdpClient LocalUdpClient { get { return m_udpClient; } }
        //----------------------------------------------------------------------------------
        //----------------------------------------------------------------------------------
    }

    //----------------------------------------------------------------------------------
    /// <summary>
    /// Main class for public interface with the network from the client game?
    /// </summary>
    //----------------------------------------------------------------------------------
    public class NetworkEngine
    {
        //turn this into an interface.
        //private static members
        private static HybridClient             m_hybridclient;
        //received packet list
        private static List<NetworkDataPacket>  m_ReceivedPackets;
        //lock object for threading.
        private static object                   m_ReceiveListLock;
        //defines the max number of connected clients.
        private static int                      m_nNumberOfClients;
        //is everything initalized.
        private static bool                     m_bInitialised;

        //client callbacks
        public ReceiveFunction                  MessageReceived;
        public event Action<int>                ClientConnected,
                                                ClientDisconnected;

        private bool                            m_bQueueReceivedMessages;

        public bool                             IsNetworkActive { get { return m_bInitialised && m_nNumberOfClients > 0; } }

        //----------------------------------------------------------------------------------
        /// <summary>
        /// Initialize all network systems including the Hybrid Client.
        /// </summary>
        /// <param name="nUdpPort">the port that will be used for comminications on the network</param>
        /// <param name="bQueueMessages">Whether received messages should be queued.</param>
        //----------------------------------------------------------------------------------
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
        //----------------------------------------------------------------------------------
        /// <summary>
        /// Update client connected mask when a new client connects
        /// </summary>
        /// <param name="nMaskIndex">The mask index of the new client.</param>
        //----------------------------------------------------------------------------------
        private void ProcessClientConnection(int nMaskIndex)
        {
            m_nNumberOfClients++;
            if (ClientConnected != null) ClientConnected(nMaskIndex);
        }
        //----------------------------------------------------------------------------------
        /// <summary>
        /// Update client connection mask when client disconnects.
        /// </summary>
        /// <param name="nMaskIndex">mask index of client that has disconnected.</param>
        //----------------------------------------------------------------------------------
        private void ProcessClientDisconnection(int nMaskIndex)
        {
            m_nNumberOfClients--;
            if (ClientDisconnected != null) ClientDisconnected(nMaskIndex);
        }
        //----------------------------------------------------------------------------------
        /// <summary>
        /// Broadcast the peer is looking for other peers to connect to.
        /// </summary>
        //----------------------------------------------------------------------------------
        public void BroadCast()
        {
            m_hybridclient.FindPeers();
        }
        //----------------------------------------------------------------------------------
        /// <summary>
        /// Send network message. Wraps hybrid client send packet.
        /// </summary>
        /// <param name="packet"></param>
        /// <param name="address"></param>
        //----------------------------------------------------------------------------------
        public void SendMessage(NetworkDataPacket packet, IPAddress address = null)
        {
            m_hybridclient.SendPacket(packet, address);
        }
        //----------------------------------------------------------------------------------
        /// <summary>
        /// Start processing the send queue.
        /// </summary>
        //----------------------------------------------------------------------------------
        public void ProcessSendQueue()
        {
            m_hybridclient.StartSendProcess();
        }
        //----------------------------------------------------------------------------------
        /// <summary>
        /// Put received queue ready for processing.
        /// </summary>
        /// <param name="packet">received packet</param>
        //----------------------------------------------------------------------------------
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
        //----------------------------------------------------------------------------------
        // Doing this avoid threading issues.
        // with the entity system...otherwise 
        // we would just pass the the message directly through 
        /// <summary>
        /// Process all received messages
        /// </summary>
        //----------------------------------------------------------------------------------
        public void ProcessReceivedMessages()
        {
            if (m_bQueueReceivedMessages)
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
        //----------------------------------------------------------------------------------
        /// <summary>
        /// Process messages that are meant for the EntityController.
        /// </summary>
        /// <param name="packet"></param>
        //----------------------------------------------------------------------------------
        public void ProcessSystemMessage(NetworkDataPacket packet)
        {
            Type type = typeof(NetworkEngine);

            //use reflection to find method.
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