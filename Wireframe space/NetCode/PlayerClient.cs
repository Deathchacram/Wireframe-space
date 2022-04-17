using BEPUphysics.Entities.Prefabs;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System.Threading;
using System.Threading.Tasks;

namespace Wireframe_space.NetCode
{
    class PlayerClient : IPlayer
    {
        public int playerId = -1;

        private Subject player;
        private bool flag = false, flag2 = false;
        private MultiplayerManager manager;
        private VertexPositionColor[] points = new VertexPositionColor[8]
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
        private short[] lines = new short[24]
        {
            0, 1, 1, 2, 2, 3, 3, 0,         //bottom plane
            4, 5, 5, 6, 6, 7, 7, 4,         //top plane
            0, 4, 1, 5, 2, 6, 3, 7,         //side edges
        };
        private PlayerControl control;

        public PlayerClient(MultiplayerManager manager)
        {
            this.manager = manager;
            control = new PlayerControl(manager);

            Task task = new Task(PositionUpdate);
            task.Start();
        }

        public void CreateObj(float[] cmd)
        {
            //0 - commandID, 1 - objID, 2-4 - pos, 5-8 - quaternion, 9-11 - speed, 12 - obj type
            BEPUutilities.Vector3 pos = new BEPUutilities.Vector3(cmd[2], cmd[3], cmd[4]);
            BEPUutilities.Quaternion quaternion = new BEPUutilities.Quaternion(cmd[5], cmd[6], cmd[7], cmd[8]);
            BEPUutilities.Vector3 speed = new BEPUutilities.Vector3(cmd[9], cmd[10], cmd[11]);

            Subject s;
            if ((int)cmd[1] != playerId)
            {
                Box box = new Box(BEPUutilities.Vector3.Zero, 1, 1, 1, 1);
                manager.space.Add(box);
                s = new Subject((int)cmd[1], 0, "asteroid", manager.game);
                s.SetModel(points, lines, box);

                box.Position = pos;
                box.Orientation = quaternion;
                box.LinearVelocity = speed;
            }
            else
            {
                Box box = new Box(BEPUutilities.Vector3.Zero, 1, 1, 1, 1);
                manager.space.Add(box);
                s = player  = new Subject((int)cmd[1], 1, "player", manager.game);
                player.SetModel(points, lines, box);

                box.Position = new BEPUutilities.Vector3(-5, -3, -10);
                box.Orientation = quaternion;
                box.LinearVelocity = speed;
            }

            manager.subjects.Add(s);

            manager.commands.Remove(cmd);
        }
        public void DestroyObj(float[] cmd)
        {
            try
            {
                manager.space.Remove(manager.space.Entities[(int)cmd[1]]);
                if (manager.subjects[(int)cmd[1]].id == (int)cmd[2])
                    manager.subjects.RemoveAt((int)cmd[1]);
                else
                {
                    for (int i = 0; i < manager.subjects.Count; i++)
                        if (manager.subjects[i].id == (int)cmd[2])
                        {
                            manager.subjects.RemoveAt(i);
                            i = manager.subjects.Count;
                        }
                }

                manager.commands.Remove(cmd);
            }
            catch { };
        }
        public void Update(GameTime gameTime)
        {
            if (manager.client.connected && !flag)
            {
                Thread.Sleep(100);
                manager.client.SendTcp(new float[] { 0.5f });
                flag = true;
            }
            else if (playerId != -1 && !flag2)
            {
                Vector3 pos = new Vector3(1, 1, 1);
                Quaternion quat = Quaternion.Identity;
                Vector3 speed = Vector3.Zero;
                manager.client.CreateObj(playerId, pos, quat, speed, 1);
                flag2 = true;
            }

            if (player != null)
            {
                Vector3 p = control.Update(gameTime);
                //player.entity.Position += new BEPUutilities.Vector3(p.X, p.Y, p.Z);
                //player.entity.LinearVelocity = BEPUutilities.Vector3.Zero;
                player.entity.LinearVelocity += new BEPUutilities.Vector3(p.X, p.Y, p.Z);
            }
            /*if (player != null)
            {
                if (Keyboard.GetState().IsKeyDown(Keys.A))
                {
                    //player.pos.X += 1f / gameTime.ElapsedGameTime.Milliseconds;
                    player.entity.LinearVelocity += new BEPUutilities.Vector3(1f / gameTime.ElapsedGameTime.Milliseconds, 0, 0);
                }
                if (Keyboard.GetState().IsKeyDown(Keys.D))
                {
                    //player.pos.X += 1f / gameTime.ElapsedGameTime.Milliseconds;
                    player.entity.LinearVelocity += new BEPUutilities.Vector3(-1f / gameTime.ElapsedGameTime.Milliseconds, 0, 0);
                }
                if (Keyboard.GetState().IsKeyDown(Keys.S))
                {
                    //player.pos.X += 1f / gameTime.ElapsedGameTime.Milliseconds;
                    player.entity.LinearVelocity += new BEPUutilities.Vector3(0, 1f / gameTime.ElapsedGameTime.Milliseconds, 0);
                }
                if (Keyboard.GetState().IsKeyDown(Keys.W))
                {
                    //player.pos.X += 1f / gameTime.ElapsedGameTime.Milliseconds;
                    player.entity.LinearVelocity += new BEPUutilities.Vector3(0, -1f / gameTime.ElapsedGameTime.Milliseconds, 0);
                }
            }*/
        }
        private void PositionUpdate()
        {
            while (true)
            {
                if (player != null)
                {
                    Thread.Sleep(100);
                    int serialNumber = manager.subjects.IndexOf(player);
                    BEPUutilities.Vector3 pos = player.entity.Position;
                    Vector3 quat = Vector3.Zero;
                    float[] data = new float[8] { serialNumber, playerId, pos.X, pos.Y, pos.Z, quat.X, quat.Y, quat.Z };
                    manager.client.SendUdp(data);
                }
            }
        }
        public void Draw()
        {

        }
        public void DealDamage(float[] command)
        {

        }
    }
}
