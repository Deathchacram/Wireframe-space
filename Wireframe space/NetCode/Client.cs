using Microsoft.Xna.Framework;
using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading.Tasks;

namespace Wireframe_space
{
    class Client
    {
        //public NetData netData = new NetData();
        //public float[][] netData = new float[30][];
        //public List<float[][]> netData = new List<float[][]>();
        public bool connected = false;

        //private IPAddress ipadr = IPAddress.Parse("77.51.211.124");
        //private IPAddress ipadr = IPAddress.Parse("192.168.1.7"); //77.51.211.124
        private IPAddress ipadr;
        private int port = 26386;
        private Socket serverTcp, serverUdp;
        private MultiplayerManager manager;


        public Client(MultiplayerManager manager)
        {
            this.manager = manager;
        }

        public void Connect()
        {
            try
            {
                using (Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, 0))
                {
                    IPEndPoint endPoint;
                    socket.Connect("8.8.8.8", 65530);
                    endPoint = socket.LocalEndPoint as IPEndPoint;
                    ipadr = endPoint.Address;
                }

                Task task1 = new Task(ListenTCP);
                task1.Start();
                //Thread listenTcp = new Thread(ListenTCP);
                //listenTcp.IsBackground = true;
                //listenTcp.Start();

                Task task2 = new Task(ListenUDP);
                task2.Start();
                //Thread listenUdp = new Thread(ListenUDP);
                //listenTcp.IsBackground = true;
                //listenUdp.Start();
            }
            catch { }
        }

        private void ListenTCP()    //TCP recive commands
        {
            IPEndPoint ipEndPoint = new IPEndPoint(IPAddress.Parse("79.111.188.119"), 26386);
            serverTcp = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            serverTcp.Connect(ipEndPoint);

            //send IP to server
            try
            {
                var bf = new BinaryFormatter();
                MemoryStream ms = new MemoryStream();
                bf.Serialize(ms, ipadr.ToString());
                byte[] msg = ms.ToArray();

                ms = new MemoryStream();
                bf.Serialize(ms, new int[] { msg.Length });
                byte[] lenght = ms.ToArray();

                serverTcp.Send(lenght);
                serverTcp.Send(msg);
            }
            catch { }

            connected = true;
            //recive

            while (true)
            {
                try
                {
                    byte[] data = new byte[32];
                    serverTcp.Receive(data);
                    var bf = new BinaryFormatter();
                    MemoryStream ms = new MemoryStream(data);
                    int[] msgLenght = bf.Deserialize(ms) as int[];

                    data = new byte[msgLenght[0]];
                    serverTcp.Receive(data);
                    bf = new BinaryFormatter();
                    ms = new MemoryStream(data);
                    float[] getData = bf.Deserialize(ms) as float[];
                    manager.commands.Add(getData);
                }
                catch { }
            }
        }

        private void ListenUDP()    //UDP update positions
        {
            try
            {
                serverUdp = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
                IPEndPoint localIP = new IPEndPoint(ipadr, 26387);
                serverUdp.Bind(localIP);

                while (true)
                {
                    byte[] data = new byte[1024]; //answer buffer
                    EndPoint remoteIp = new IPEndPoint(IPAddress.Any, 0);

                    do
                    {
                        try
                        {
                            serverUdp.ReceiveFrom(data, ref remoteIp);
                            var bf = new BinaryFormatter();
                            MemoryStream ms = new MemoryStream(data);
                            float[][] getData = bf.Deserialize(ms) as float[][];

                            //0 - serial number in array, 1 - objID, 2-4 - pos, 5-8 - quaternion
                            for (int i = 0; i < 10; i++)
                            {
                                if (getData[i] != null)
                                {
                                    if (manager.subjects.Count > getData[i][0] && getData[i][1] != manager.id && getData[i][1] != -1)
                                    {
                                        Vector3 pos = new Vector3(getData[i][2], getData[i][3], getData[i][4]);

                                        if (manager.subjects[(int)getData[i][0]].id == (int)getData[i][1])
                                            manager.subjects[(int)getData[i][0]].SetPosition(pos);
                                        else
                                            for (int l = 0; l < manager.subjects.Count; l++)
                                                if (manager.subjects[l].id == (int)getData[i][1])
                                                    manager.subjects[l].SetPosition(pos);
                                    }
                                }
                            }
                        }
                        catch(Exception ex) 
                        {
                            string s = ex.Source;
                        }
                    }
                    while (serverUdp.Available > 0);

                }
            }
            catch { }
        }

        public void CreateObj(int id, Vector3 pos, Quaternion oreintation, Vector3 speed, int type)
        {
            try
            {
                //0 - commandID, 1 - objID, 2-4 - pos, 5-8 - quaternion, 9-11 - speed, 12 - obj type, 13 - obj model id
                float[] data = new float[13] { 0, id, pos.X, pos.Y, pos.Z, oreintation.X, oreintation.Y, oreintation.Z, oreintation.W, speed.X, speed.Y, speed.Z, type };
                SendTcp(data);
                /*var bf = new BinaryFormatter();
                MemoryStream ms = new MemoryStream();
                bf.Serialize(ms, data);
                byte[] msg = ms.ToArray();

                bf = new BinaryFormatter();
                ms = new MemoryStream();
                bf.Serialize(ms, new int[] { msg.Length });
                byte[] msgLenght = ms.ToArray();

                serverTcp.Send(msgLenght);
                serverTcp.Send(msg);*/
            }
            catch { }
        }
        public void DealDamage(int id, int damage)
        {
            try
            {
                //0 - cmd type, 1 - id, 2 - damage
                float[] data = new float[3] { 2, id, damage };
                SendTcp(data);
            }
            catch { }
        }

        public void SendTcp(float[] cmd)
        {
            var bf = new BinaryFormatter();
            MemoryStream ms = new MemoryStream();
            bf.Serialize(ms, cmd);
            byte[] msg = ms.ToArray();

            bf = new BinaryFormatter();
            ms = new MemoryStream();
            bf.Serialize(ms, new int[] { msg.Length });
            byte[] msgLenght = ms.ToArray();

            serverTcp.Send(msgLenght);
            serverTcp.Send(msg);
        }
        public void SendUdp(float[] cmd)
        {
            var bf = new BinaryFormatter();
            MemoryStream ms = new MemoryStream();
            bf.Serialize(ms, cmd);
            byte[] msg = ms.ToArray();

            IPEndPoint endPoint = new IPEndPoint(IPAddress.Parse("79.111.188.119"), 26388);
            endPoint.Port = 26388;
            serverUdp.SendTo(msg, endPoint);
        }
    }
}
