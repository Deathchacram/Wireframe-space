using BEPUphysics.Entities.Prefabs;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Wireframe_space.NetCode
{
    class PlayerServer : IPlayer
    {
        public int playerId = -1;

        private Subject player;
        private MultiplayerManager manager;
        private Server server;
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

        public PlayerServer(MultiplayerManager manager, Server server)
        {
            this.manager = manager;
            this.server = server;
            control = new PlayerControl(manager);

            playerId = manager.id = manager.FindFreeId();
            float[] cmd = new float[13] { 0, playerId, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1 };
            manager.commands.Add(cmd);
        }

        public void CreateObj(float[] cmd)
        {
            BEPUutilities.Vector3 pos = new BEPUutilities.Vector3(cmd[2], cmd[3], cmd[4]);
            Vector3 posv = new Vector3(cmd[2], cmd[3], cmd[4]);
            BEPUutilities.Quaternion quaternion = new BEPUutilities.Quaternion(cmd[5], cmd[6], cmd[7], cmd[8]);
            BEPUutilities.Vector3 speed = new BEPUutilities.Vector3(cmd[9], cmd[10], cmd[11]);
            Vector3 speedv = new Vector3(cmd[9], cmd[10], cmd[11]);

            Subject s;
            if (cmd[1] != playerId)
            {
                s = new Subject((int)cmd[1], (int)cmd[12], "asteroid", manager.game);
                Box box = new Box(BEPUutilities.Vector3.Zero, 1, 1, 1, 1);
                manager.space.Add(box);
                s.SetModel(points, lines, box);

                box.Position = pos;
                box.Orientation = quaternion;
                box.LinearVelocity = speed;
            }
            else
            {
                Box box = new Box(BEPUutilities.Vector3.Zero, 1, 1, 1, 1);
                manager.space.Add(box);
                s = player = new Subject((int)cmd[1], 1, "player", manager.game);
                player.SetModel(points, lines, box);

                box.Position = new BEPUutilities.Vector3(-5, -3, -10);
                //box.Orientation = quaternion;
                //box.LinearVelocity = speed;
            }

            manager.subjects.Add(s);
            server.CreateObj((int)cmd[1], posv, quaternion, speedv, (int)cmd[12]);

            manager.commands.Remove(cmd);
        }
        public void DestroyObj(float[] cmd)
        {
            manager.space.Remove(manager.space.Entities[(int)cmd[1]]);
            manager.subjects.RemoveAt((int)cmd[1]);

            server.DestroyObj((int)cmd[1], (int)cmd[2]);

            manager.commands.Remove(cmd);
        }
        public void Update(GameTime gameTime)
        {
            if (player != null)
            {
                Vector3 p = control.Update(gameTime);
                //player.entity.Position += new BEPUutilities.Vector3(p.X, p.Y, p.Z);
                //player.entity.LinearVelocity = BEPUutilities.Vector3.Zero;
                player.entity.LinearVelocity += new BEPUutilities.Vector3(p.X, p.Y, p.Z);
            }
            for (int i = 0; i < manager.subjects.Count; i++)
            {
                if (manager.subjects[i].type != 1)
                    manager.subjects[i].entity.LinearVelocity = new BEPUutilities.Vector3(-0.1f * i, 0, 0);
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
