using BEPUphysics;
using BEPUphysics.Entities.Prefabs;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Mono.Nat;
using System.Collections.Generic;
using System.Threading.Tasks;
using Wireframe_space.NetCode;
using Matrix = Microsoft.Xna.Framework.Matrix;
using Vector3 = Microsoft.Xna.Framework.Vector3;

namespace Wireframe_space
{
    class MultiplayerManager : DrawableGameComponent
    {
        public static MultiplayerManager manager;

        public List<float[]> commands = new List<float[]>();
        public List<Subject> subjects = new List<Subject>();
        public List<int> playersId = new List<int>();
        public int id = -1;
        public bool isServer;
        public Space space;
        public Client client;
        public Server server;
        public Subject playerSubject;

        private INatDevice device;
        //debug
        public string s = "";
        int counter = 0;

        public Game game;
        Effect basicEffect;
        IPlayer player;
        public SpriteFont font;

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
        protected override void LoadContent()
        {
            NatUtility.DeviceFound += DeviceFound;
            NatUtility.DeviceLost += DeviceLost;
            NatUtility.StartDiscovery();

            Subject.basicEffect = game.Content.Load<Effect>("BaseEffect");
            Subject.basicEffect.Parameters[0].SetValue(Matrix.CreatePerspectiveFieldOfView(3.14f / 3, 1.6667f, 0.1f, 300));

            font = game.Content.Load<SpriteFont>("text");
            space = new Space();

            if (isServer)
            {
                for (int i = 0; i < 25; i++)
                {
                    Box box = new Box(BEPUutilities.Vector3.Zero, 1, 1, 1, 1);
                    box.Position = new BEPUutilities.Vector3(3, 0, i);
                    space.Add(box);

                    Subject s = new Subject(i, 0, "asteroid", game);
                    s.SetModel(points, lines, box);
                    subjects.Add(s);
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
                    case 0.5f:
                        PlayerClient pl = player as PlayerClient;
                        pl.playerId = (int)commands[0][1];
                        id = (int)commands[0][1];
                        commands.Remove(commands[0]);
                        break;
                }
            }
            player.Update(gameTime);
            space.Update();
        }
        public override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.Black);

            SpriteBatch sb = game.Services.GetService<SpriteBatch>();
            if (isServer)
            {
                sb.Begin();
                sb.DrawString(font, server.users.Count.ToString(), new Vector2(0, 0), Color.White);
                sb.DrawString(font, space.Entities.Count.ToString(), new Vector2(0, 20), Color.White);
                sb.DrawString(font, s, new Vector2(0, 40), Color.White);
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
                sb.DrawString(font, commands.Count.ToString() + s, new Vector2(0, 40), Color.White);
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
            for (int i = 0; i < subjects.Count; i++)
                if (subjects[i].id != i)
                    return i;

            return subjects.Count;
        }

        private void DeviceFound(object sender, DeviceEventArgs args)
        {
            device = args.Device;

            //device.DeletePortMap(new Mapping(Protocol.Tcp, 26386, 26386));
            //device.DeletePortMap(new Mapping(Protocol.Udp, 26387, 26387));

            if (isServer)
            {
                if (device.GetSpecificMapping(Protocol.Tcp, 26386).PublicPort == -1)
                    device.CreatePortMap(new Mapping(Protocol.Tcp, 26386, 26386));

                if (device.GetSpecificMapping(Protocol.Udp, 26388).PublicPort == -1)
                    device.CreatePortMap(new Mapping(Protocol.Udp, 26388, 26388));
            }
            else if (device.GetSpecificMapping(Protocol.Udp, 26387).PublicPort == -1)
                device.CreatePortMap(new Mapping(Protocol.Udp, 26387, 26387));

            /*foreach (Mapping portMap in device.GetAllMappings())
                s += '\n' + portMap.ToString();*/

            //ipadr = device.GetExternalIP();
        }

        private void DeviceLost(object sender, DeviceEventArgs args)
        {
            device = args.Device;

            //if (device.GetSpecificMapping(Protocol.Tcp, 26386).PublicPort > -1)
            //    device.DeletePortMap(new Mapping(Protocol.Tcp, 26386, 26386));
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
