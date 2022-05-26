using BEPUphysics.Entities;
using Microsoft.Xna.Framework;
using System;
using System.IO;
using System.Net.Sockets;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading;

namespace Wireframe_space
{
    class ClientObject
    {

        public int clientId = -1;
        private MultiplayerManager manager;
        private Socket client;
        private Server server;

        public ClientObject(Socket cl, Server serv, MultiplayerManager manager)
        {
            client = cl;
            server = serv;
            this.manager = manager;
        }

        public void Process()
        {
            try
            {
                for (int i = 0; i < manager.subjects.Count; i++)
                {
                    Thread.Sleep(1);
                    float[] data;
                    if (manager.subjects[i].entity != null)
                    {
                        Entity obj = manager.subjects[i].entity;
                        Vector3 p = new Vector3(obj.Position.X, obj.Position.Y, obj.Position.Z);
                        Quaternion q = new Quaternion(obj.Orientation.X, obj.Orientation.Y, obj.Orientation.Z, obj.Orientation.W);

                        //NetData data = new NetData(p, q);
                        data = new float[13] { 0, manager.subjects[i].id, p.X, p.Y, p.Z, q.X, q.Y, q.Z, q.W, 0, 0, 0, manager.subjects[i].type };
                    }
                    else
                    {
                        Vector3 p = new Vector3(manager.subjects[i].pos.X, manager.subjects[i].pos.Y, manager.subjects[i].pos.Z);
                        Quaternion q = Quaternion.Identity;

                        //NetData data = new NetData(p, q);
                        data = new float[13] { 0, manager.subjects[i].id, p.X, p.Y, p.Z, q.X, q.Y, q.Z, q.W, 0, 0, 0, manager.subjects[i].type };
                    }

                    var bf = new BinaryFormatter();
                    MemoryStream ms = new MemoryStream();
                    bf.Serialize(ms, data);
                    byte[] msg = ms.ToArray();

                    ms = new MemoryStream();
                    bf.Serialize(ms, new int[] { msg.Length });
                    byte[] msgLenght = ms.ToArray();

                    client.Send(msgLenght);
                    client.Send(msg);
                }
            }
            catch { }

            try
            {
                byte[] bytes = new byte[512];

                while (true)
                {
                    bytes = new byte[32];
                    client.Receive(bytes);
                    var bf = new BinaryFormatter();
                    MemoryStream ms = new MemoryStream(bytes);
                    int[] msgLenght = bf.Deserialize(ms) as int[];

                    bytes = new byte[msgLenght[0]];
                    client.Receive(bytes);
                    bf = new BinaryFormatter();
                    ms = new MemoryStream(bytes);
                    float[] data = bf.Deserialize(ms) as float[];

                    //command 0, 1, 2...
                    if (data[0] != 0.5f)          
                        manager.commands.Add(data);
                    //send free id
                    else
                    {
                        int id = clientId = manager.FindFreeId();
                        //add
                        //manager.players.Add(id, this);
                        data = new float[2] { 0.5f, id };
                        bf = new BinaryFormatter();
                        ms = new MemoryStream();
                        bf.Serialize(ms, data);
                        byte[] msg = ms.ToArray();

                        bf = new BinaryFormatter();
                        ms = new MemoryStream();
                        bf.Serialize(ms, new int[] { msg.Length });
                        byte[] lenght = ms.ToArray();

                        client.Send(lenght);
                        client.Send(msg);
                    }
                }
            }
            catch (Exception ex)
            {
                client.Shutdown(SocketShutdown.Both);
                client.Close();
                server.DisconnectUser(client, this);
            }
        }
        public void SendCommand(byte[] cmd)
        {
            var bf = new BinaryFormatter();
            MemoryStream ms = new MemoryStream();
            bf.Serialize(ms, new int[] { cmd.Length });
            byte[] msgLenght = ms.ToArray();

            client.Send(msgLenght);
            client.Send(cmd);
        }
    }
}