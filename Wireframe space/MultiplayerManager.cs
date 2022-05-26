using BEPUphysics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Open.Nat;
using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using Wireframe_space.NetCode;
using Matrix = Microsoft.Xna.Framework.Matrix;
using Vector3 = Microsoft.Xna.Framework.Vector3;

namespace Wireframe_space
{
    class MultiplayerManager : DrawableGameComponent
    {
        //global
        public static MultiplayerManager manager;
        public Space space;

        public List<float[]> commands = new List<float[]>();
        public List<Subject> subjects = new List<Subject>();
        public Dictionary<int, ClientObject> players = new Dictionary<int, ClientObject>();
        public List<int> idCollection = new List<int>();

        //player
        public int id = -1;
        public bool isServer;
        public Client client;
        public Server server;
        public Subject playerSubject;
        public static IPlayer player;
        public List<Subject> toDestroy = new List<Subject>();

        //debug
        public string s = "";
        int counter = 0;

        //other
        private NatDevice device;
        public Game game;
        public SpriteFont font;
        Effect basicEffect;
        IPAddress externalIP, serverIP;

        VertexPositionColor[] points = new VertexPositionColor[8]
        {
            new VertexPositionColor(new Vector3(-0.5f, -0.5f, -0.5f), Color.White),
            new VertexPositionColor(new Vector3(0.5f, -0.5f, -0.5f), Color.White),
            new VertexPositionColor(new Vector3(0.5f, 0.5f, -0.5f), Color.White),
            new VertexPositionColor(new Vector3(-0.5f, 0.5f, -0.5f), Color.White),
            new VertexPositionColor(new Vector3(-0.5f, -0.5f, 0.5f), Color.White),
            new VertexPositionColor(new Vector3(0.5f, -0.5f, 0.5f), Color.White),
            new VertexPositionColor(new Vector3(0.5f, 0.5f, 0.5f), Color.White),
            new VertexPositionColor(new Vector3(-0.5f, 0.5f, 0.5f), Color.White)
        };
        short[] lines = new short[24]
        {
            0, 1, 1, 2, 2, 3, 3, 0,         //bottom plane
            4, 5, 5, 6, 6, 7, 7, 4,         //top plane
            0, 4, 1, 5, 2, 6, 3, 7,         //side edges
        };

        public MultiplayerManager(Game game) : base(game)
        {
            this.game = game;
            manager = this;
        }
        public MultiplayerManager(Game game, string ip) : base(game)
        {
            serverIP = IPAddress.Parse(ip);
            this.game = game;
            manager = this;
        }
        async protected override void LoadContent()
        {
            //NatUtility.DeviceFound += DeviceFound;
            //NatUtility.DeviceLost += DeviceLost;
            //NatUtility.StartDiscovery();

            CreateNet();

            Subject.basicEffect = game.Content.Load<Effect>("BaseEffect");
            Subject.basicEffect.Parameters[0].SetValue(Matrix.CreatePerspectiveFieldOfView(3.14f / 3, 1.6667f, 0.1f, 300));

            font = game.Content.Load<SpriteFont>("text");
            space = new Space();

            if (isServer)
            {
                for (int i = 0; i < 200; i++)
                {
                    int freeId = FindFreeId();
                    Quaternion q = Quaternion.Identity;
                    Random rnd = new Random();
                    //float[] cmd = new float[13] { 0, freeId, 3, 0, i * 2, q.X, q.Y, q.Z, q.W, 0, 0, 0, 0 };
                    //commands.Add(cmd);

                    //freeId = FindFreeId();
                    float[] cmd = new float[13] { 0, freeId, rnd.Next(-100, 100), rnd.Next(-100, 100), rnd.Next(-100, 100), q.X, q.Y, q.Z, q.W, 0, 0, 0, 0 };
                    commands.Add(cmd);

                    /*Box box = new Box(BEPUutilities.Vector3.Zero, 1, 1, 1, 1);
                    box.Position = new BEPUutilities.Vector3(3, 0, i);
                    space.Add(box);

                    Subject s = new Subject(i, 0, "asteroid", game);
                    s.SetModel(points, lines, box);
                    subjects.Add(s);*/
                }

                server = new Server(this);
                player = new PlayerServer(this, server);

                Task task = new Task(server.Start);
                task.Start();
            }
            else
            {
                player = new PlayerClient(this);
                client = new Client(this);
                client.externalIp = externalIP;
                if (serverIP != null)
                {
                    client.serverIp = serverIP;
                }
                Task task = new Task(client.Connect);
                task.Start();
                //connect = new Thread(client.Connect);
                //connect.IsBackground = true;
                //connect.Start();
            }
        }
        public override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
            {
                //Thread.Sleep(1000);
                game.Exit();
            }
            if (isServer)
            {
                counter++;
                if (counter % 60 == 0 && space.Entities.Count != 0)
                {
                    counter %= 60;
                    //0 - command number, 1 - serial number in array, 2 - id (check)
                    //commands.Add(new float[3] { 1, subjects.Count - 1, subjects.Count - 1 });
                }
            }

            for (int i = 0; i < toDestroy.Count; i++)
                toDestroy[0].Destroy();

            while (commands.Count != 0)
            {
                s += commands.Count.ToString();
                switch (commands[0][0])
                {
                    case 0:
                        player.CreateObj(commands[0]);
                        break;
                    case 1:
                        player.DestroyObj(commands[0]);
                        break;
                    case 2:
                        player.DealDamage(commands[0]);
                        break;
                    case 0.5f:
                        PlayerClient pl = player as PlayerClient;
                        pl.playerId = (int)commands[0][1];
                        id = (int)commands[0][1];
                        commands.Remove(commands[0]);
                        break;
                }
            }
            player.Update(gameTime);
            try
            {
                space.Update();
            }
            catch { }
        }
        public override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.Black);

            player.Draw();
            SpriteBatch sb = game.Services.GetService<SpriteBatch>();
            sb.Begin();
            sb.DrawString(font, "O", new Vector2(395, 235), Color.White);
            sb.DrawString(font, subjects.Count + " " + space.Entities.Count, new Vector2(0, 60), Color.White);
            if (externalIP != null)
                sb.DrawString(font, externalIP.ToString(), new Vector2(660, 20), Color.White);
            sb.End();

            if (isServer)
            {
                sb.Begin();
                //sb.DrawString(font, server.users.Count.ToString(), new Vector2(0, 0), Color.White);
                //sb.DrawString(font, space.Entities.Count.ToString(), new Vector2(0, 20), Color.White);
                //sb.DrawString(font, "O", new Vector2(395, 235), Color.White);
                //sb.DrawString(font, v.ToString(), new Vector2(395, 235), Color.White);
                //sb.DrawString(font, s, new Vector2(0, 40), Color.White);
                sb.End();

                for (int i = 0; i < subjects.Count; i++)
                {
                    subjects[i].Draw();
                    /*BEPUutilities.Vector3 p = space.Entities[i].Position;
                    Vector3 pos = new Vector3(p.X, p.Y, p.Z);

                    basicEffect.Parameters[1].SetValue(Matrix.Identity);
                    basicEffect.Parameters[2].SetValue(Matrix.CreateRotationY(0 * 0.017f) * Matrix.CreateRotationX(0 * 0.017f));
                    basicEffect.Parameters[3].SetValue(new Vector3(-5, -3, -10) - pos);
                    basicEffect.Parameters[4].SetValue(new Vector4(1, 1, 1, 1));
                    basicEffect.CurrentTechnique.Passes[0].Apply();

                    game.GraphicsDevice.DrawUserIndexedPrimitives(PrimitiveType.LineList, points, 0, points.Length, lines, 0, lines.Length / 2);*/
                }
            }
            else
            {
                sb.Begin();
                sb.DrawString(font, commands.Count.ToString() + s, new Vector2(0, 20), Color.White);
                /*string a = "";
                for (int i = 0; i < subjects.Count; i += 10)
                {
                    for (int l = 0; l < 7; l++)
                    {
                        if (i + l < subjects.Count)
                            a += "id:" + subjects[i + l].id + "pos:" + subjects[i + l].entity.Position.ToString();
                    }
                    a += '\n';
                }
                sb.DrawString(font, a, new Vector2(0, 40), Color.White);*/
                sb.End();
                for (int i = 0; i < subjects.Count; i++)
                {
                    subjects[i].Draw();
                    /*Vector3 pos = new Vector3(space.Entities[i].Position.X, space.Entities[i].Position.Y, space.Entities[i].Position.Z);

                    basicEffect.Parameters[1].SetValue(Matrix.Identity);
                    basicEffect.Parameters[2].SetValue(Matrix.CreateRotationY(0 * 0.017f) * Matrix.CreateRotationX(0 * 0.017f));
                    basicEffect.Parameters[3].SetValue(new Vector3(-5, -3, -10) - pos);
                    basicEffect.Parameters[4].SetValue(new Vector4(1, 1, 1, 1));
                    basicEffect.CurrentTechnique.Passes[0].Apply();

                    game.GraphicsDevice.DrawUserIndexedPrimitives(PrimitiveType.LineList, points, 0, points.Length, lines, 0, lines.Length / 2);*/
                }
            }
        }
        public int FindFreeId()
        {

            for (int i = 0; i < idCollection.Count; i++)
                if (idCollection.IndexOf(i) == -1)
                {
                    idCollection.Add(i);
                    return i;
                }

            idCollection.Add(idCollection.Count);
            return idCollection.Count - 1;
        }

        private async void CreateNet()
        {
            var discoverer = new NatDiscoverer();
            //var cts = new CancellationTokenSource(10000);
            //var device = await discoverer.DiscoverDeviceAsync(PortMapper.Upnp, cts);
            device = await discoverer.DiscoverDeviceAsync();
            var m = device.GetAllMappingsAsync().Result;
            externalIP = await device.GetExternalIPAsync();
            if (client != null)
                client.externalIp = externalIP;

            if (isServer)
            {
                if (await device.GetSpecificMappingAsync(Protocol.Tcp, 26386) == null)
                    await device.CreatePortMapAsync(new Mapping(Protocol.Tcp, 26386, 26386, "Tcp server"));

                if (await device.GetSpecificMappingAsync(Protocol.Udp, 26387) == null)
                    await device.CreatePortMapAsync(new Mapping(Protocol.Udp, 26387, 26387, "Udp server"));
            }
            else
            {
                if (await device.GetSpecificMappingAsync(Protocol.Udp, 26388) == null)
                    await device.CreatePortMapAsync(new Mapping(Protocol.Udp, 26388, 26388, "Udp client"));
                else if (await device.GetSpecificMappingAsync(Protocol.Udp, 26389) == null)
                    await device.CreatePortMapAsync(new Mapping(Protocol.Udp, 26389, 26389, "Udp client"));
                else if (await device.GetSpecificMappingAsync(Protocol.Udp, 26390) == null)
                    await device.CreatePortMapAsync(new Mapping(Protocol.Udp, 26390, 26390, "Udp client"));
            }

        }
        public async void DestroyNet()
        {
            await device.DeletePortMapAsync(new Mapping(Protocol.Tcp, 26386, 26386, "Tcp server"));
            await device.DeletePortMapAsync(new Mapping(Protocol.Udp, 26388, 26388, "Udp server"));
            await device.DeletePortMapAsync(new Mapping(Protocol.Udp, 26387, 26387, "Udp client"));
        }
    }
    interface IPlayer
    {
        public void CreateObj(float[] command);
        public void DestroyObj(float[] command);
        public void DealDamage(float[] command);
        public void Update(GameTime gameTime);
        public void Draw();
    }
}
