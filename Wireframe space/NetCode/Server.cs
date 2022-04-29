using BEPUutilities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading;
using System.Threading.Tasks;
using Vector3 = Microsoft.Xna.Framework.Vector3;
using Vector4 = Microsoft.Xna.Framework.Vector4;

namespace Wireframe_space
{
    class Server
    {
        //26386 26387
        public List<Socket> users = new List<Socket>();
        public List<ClientObject> clients = new List<ClientObject>();

        private MultiplayerManager manager;
        private IPAddress localIp;
        private List<IPAddress> ipadr = new List<IPAddress>();
        private int port = 26386;
        private Socket udp, sListener;

        public Server(MultiplayerManager manager)
        {
            //ipadr = new IPAddress(IPAddress.Parse("79.111.188.119").GetAddressBytes());
            //ipadr = new IPAddress(IPAddress.Parse("192.168.1.7").GetAddressBytes());

            sListener = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            udp = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);

            using (Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, 0))
            {
                IPEndPoint endPoint;
                socket.Connect("8.8.8.8", 65530);
                endPoint = socket.LocalEndPoint as IPEndPoint;
                localIp = endPoint.Address;
            }

            this.manager = manager;
        }
        public void Start()
        {
            try
            {
                IPEndPoint ipEndPoint = new IPEndPoint(IPAddress.Any, 26386);
                sListener.Bind(ipEndPoint);
                sListener.Listen(10);

                Task task = new Task(BroadcastMessage);
                task.Start();
                Task task2 = new Task(ListenUdp);
                task2.Start();

                while (true)
                {
                    Socket handler = sListener.Accept();
                    users.Add(handler);

                    byte[] data = new byte[32];
                    handler.Receive(data);
                    var bf = new BinaryFormatter();
                    MemoryStream ms = new MemoryStream(data);
                    int[] msgLenght = bf.Deserialize(ms) as int[];

                    data = new byte[msgLenght[0]];
                    handler.Receive(data);
                    bf = new BinaryFormatter();
                    ms = new MemoryStream(data);
                    IPAddress getData = IPAddress.Parse(bf.Deserialize(ms) as string);
                    ipadr.Add(getData);

                    ClientObject client = new ClientObject(handler, this, manager);
                    clients.Add(client);
                    Task task1 = new Task(client.Process);
                    task1.Start();
                }
            }
            catch { }
        }

        public void BroadcastMessage()   //UDP
        {
            try
            {
                while (true)
                {
                    Thread.Sleep(100);
                    /*//Send environment positions
                    for (int l = 0; l < manager.subjects.Count; l += 10)
                    {
                        float[][] data = new float[10][];

                        for (int i = 0; i < 10; i++)
                        {
                            if (manager.subjects.Count > i + l)
                                if (manager.subjects[i + l] != null && manager.subjects[i + l].type != 1)
                                {
                                    Vector3 p;
                                    if (manager.subjects[i + l].entity != null)
                                    {
                                        BEPUutilities.Vector3 ePos = manager.subjects[i + l].entity.Position;
                                        p = new Vector3(ePos.X, ePos.Y, ePos.Z);
                                    }
                                    else
                                        p = manager.subjects[i + l].pos;

                                    Vector3 q = manager.subjects[i + l].quaternion;

                                    data[i] = new float[7] { p.X, p.Y, p.Z, q.X, q.Y, q.Z, i + l };
                                }
                        }
                        var bf = new BinaryFormatter();
                        MemoryStream ms = new MemoryStream();
                        bf.Serialize(ms, data);
                        byte[] msg = ms.ToArray();

                        foreach (IPAddress ipa in ipadr)
                        {
                            IPEndPoint endPoint = new IPEndPoint(ipa, 26387);
                            endPoint.Port = 26387;
                            udp.SendTo(msg, endPoint);
                        }
                    }
                    //Send players positions
                    for (int m = 0; m < manager.playersId.Count; m++)
                    {
                        for (int l = 0; l < manager.subjects.Count; l += 10)
                        {
                            float[][] data = new float[10][];

                            for (int i = 0; i < 10; i++)
                            {
                                if (manager.subjects.Count > i + l)
                                    if (manager.subjects[i + l] != null && manager.subjects[i + l].type == 1 && manager.subjects[i + l].id != manager.playersId[m])
                                    {
                                        Vector3 p;
                                        if (manager.subjects[i + l].entity != null)
                                        {
                                            BEPUutilities.Vector3 ePos = manager.subjects[i + l].entity.Position;
                                            p = new Vector3(ePos.X, ePos.Y, ePos.Z);
                                        }
                                        else
                                            p = manager.subjects[i + l].pos;

                                        Vector3 q = manager.subjects[i + l].quaternion;

                                        data[i] = new float[7] { p.X, p.Y, p.Z, q.X, q.Y, q.Z, i + l };
                                    }
                            }
                            var bf = new BinaryFormatter();
                            MemoryStream ms = new MemoryStream();
                            bf.Serialize(ms, data);
                            byte[] msg = ms.ToArray();

                            IPEndPoint endPoint = new IPEndPoint(ipadr[m], 26387);
                            endPoint.Port = 26387;
                            udp.SendTo(msg, endPoint);
                        }
                    }*/

                    for (int l = 0; l < manager.subjects.Count; l += 10)
                    {
                        float[][] data = new float[10][];

                        for (int i = 0; i < 10; i++)
                        {
                            if (manager.subjects.Count > i + l)
                                if (manager.subjects[i + l] != null)
                                {
                                    Vector3 p;
                                    if (manager.subjects[i + l].type != 2)  //dont update bullet pos
                                    {
                                        if (manager.subjects[i + l].entity != null)
                                        {
                                            BEPUutilities.Vector3 ePos = manager.subjects[i + l].entity.Position;
                                            p = new Vector3(ePos.X, ePos.Y, ePos.Z);
                                        }
                                        else
                                            p = manager.subjects[i + l].pos;

                                        Vector4 q = manager.subjects[i + l].quaternion;

                                        //0 - serial number in array, 1 - objID, 2-4 - pos, 5-8 - quaternion
                                        data[i] = new float[9] { i + l, manager.subjects[i + l].id, p.X, p.Y, p.Z, q.X, q.Y, q.Z, q.W };
                                    }
                                    else
                                        data[i] = new float[9] { -1, -1, 0, 0, 0, 0, 0, 0, 0 };
                                }
                        }
                        var bf = new BinaryFormatter();
                        MemoryStream ms = new MemoryStream();
                        bf.Serialize(ms, data);
                        byte[] msg = ms.ToArray();

                        foreach (IPAddress ipa in ipadr)
                        {
                            IPEndPoint endPoint = new IPEndPoint(ipa, 26387);
                            for (int i = 26387; i < 26390; i++)
                            {
                                //endPoint.Port = 26387;
                                endPoint.Port = i;
                                udp.SendTo(msg, endPoint);
                            }
                        }
                    }
                }
            }
            catch { }
        }
        public void BroadcastMessageTcp(byte[] command)
        {
            foreach (Socket user in users)
            {
                try
                {
                    user.Send(command);
                }
                catch { };
            }
        }

        public void ListenUdp()
        {
            try
            {
                IPEndPoint localIP = new IPEndPoint(localIp, 26388);
                udp.Bind(localIP);

                while (true)
                {
                    byte[] data = new byte[512];
                    EndPoint remoteIp = new IPEndPoint(IPAddress.Any, 0);

                    do
                    {
                        try
                        {
                            udp.ReceiveFrom(data, ref remoteIp);
                            var bf = new BinaryFormatter();
                            MemoryStream ms = new MemoryStream(data);
                            float[] getData = bf.Deserialize(ms) as float[];

                            //0 - serial number in array, 1 - objID, 2-4 - pos, 5-8 - quaternion
                            if (manager.subjects[(int)getData[0]].id == getData[1])
                            {
                                //manager.subjects[(int)getData[0]].pos = new Vector3(getData[2], getData[3], getData[4]);
                                //manager.subjects[(int)getData[0]].quaternion = new Vector3(getData[5], getData[6], getData[7]);
                                manager.subjects[(int)getData[0]].SetPosition(new Vector3(getData[2], getData[3], getData[4]));
                            }
                            else
                            {
                                for (int i = 0; i < manager.subjects.Count; i++)
                                    if (manager.subjects[i].id == (int)getData[1])
                                    {
                                        //manager.subjects[(int)getData[0]].pos = new Vector3(getData[2], getData[3], getData[4]);
                                        //manager.subjects[(int)getData[0]].quaternion = new Vector3(getData[5], getData[6], getData[7]);
                                        manager.subjects[(int)getData[0]].SetPosition(new Vector3(getData[2], getData[3], getData[4]));
                                    }
                            }
                        }
                        catch { }
                    }
                    while (udp.Available > 0);

                }
            }
            catch { }
        }

        public void DisconnectUser(Socket s, ClientObject c)
        {
            users.Remove(s);
            clients.Remove(c);
        }

        public void CreateObj(int id, Vector3 pos, Quaternion oreintation, Vector3 speed, int type)
        {
            try
            {
                ////0 - commandID, 1 - objID, 2-4 - pos, 5-8 - quaternion, 9-11 - speed, 12 - obj type
                float[] data = new float[13] { 0, id, pos.X, pos.Y, pos.Z, oreintation.X, oreintation.Y, oreintation.Z, oreintation.W, speed.X, speed.Y, speed.Z, type };
                var bf = new BinaryFormatter();
                MemoryStream ms = new MemoryStream();
                bf.Serialize(ms, data);
                byte[] msg = ms.ToArray();

                foreach (ClientObject client in clients)
                    client.SendCommand(msg);
                //serverTcp.Send(msg);
            }
            catch { }
        }
        public void DestroyObj(int serialNumber, int id)
        {
            try
            {
                ///0 - commandID, 1 - serial number in array, 2 - id
                float[] data = new float[3] { 1, serialNumber, id };
                var bf = new BinaryFormatter();
                MemoryStream ms = new MemoryStream();
                bf.Serialize(ms, data);
                byte[] msg = ms.ToArray();

                foreach (ClientObject client in clients)
                    client.SendCommand(msg);
                //serverTcp.Send(msg);
            }
            catch { }
        }
        public void DealDamage(int id, int damage)
        {
            try
            {
                ///0 - commandID, 1 - id, 2 - damage
                float[] data = new float[3] { 2, id, damage };
                var bf = new BinaryFormatter();
                MemoryStream ms = new MemoryStream();
                bf.Serialize(ms, data);
                byte[] msg = ms.ToArray();

                foreach (ClientObject client in clients)
                    client.SendCommand(msg);
                //serverTcp.Send(msg);
            }
            catch { }
        }

    }
    [Serializable]
    public class NetData
    {
        public float x = 0, y = 0, z = 0;
        public float q1 = 0, q2 = 0, q3 = 0;
        public NetData(Vector3 pos, Vector3 q)
        {
            x = pos.X;
            y = pos.Y;
            z = pos.Z;
            q1 = q.X;
            q2 = q.Y;
            q3 = q.Z;
        }
        public NetData() { }
    }
}
