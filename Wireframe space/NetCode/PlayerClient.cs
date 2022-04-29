using BEPUphysics.BroadPhaseEntries;
using BEPUphysics.BroadPhaseEntries.MobileCollidables;
using BEPUphysics.Entities.Prefabs;
using BEPUphysics.NarrowPhaseSystems.Pairs;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Wireframe_space.NetCode
{
    class PlayerClient : IPlayer
    {
        public int playerId = -1;
        public static Subject player;

        private bool flag = false, flag2 = false;
        private List<int> bulletsId = new List<int>();
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
            if (bulletsId.IndexOf((int)cmd[1]) == -1)
            {
                BEPUutilities.Vector3 pos = new BEPUutilities.Vector3(cmd[2], cmd[3], cmd[4]);
                BEPUutilities.Quaternion quaternion = new BEPUutilities.Quaternion(cmd[5], cmd[6], cmd[7], cmd[8]);
                BEPUutilities.Vector3 speed = new BEPUutilities.Vector3(cmd[9], cmd[10], cmd[11]);

                Subject s;
                Box box = new Box(BEPUutilities.Vector3.Zero, 1, 1, 1, 1);
                manager.space.Add(box);
                box.Position = pos;
                box.Orientation = quaternion;
                box.LinearVelocity = speed;

                if ((int)cmd[1] == playerId)
                {
                    s = player = new Subject((int)cmd[1], 1, "player", manager.game);
                    player.SetModel(points, lines, box);
                    player.hp = 3;
                    /*Box box = new Box(BEPUutilities.Vector3.Zero, 1, 1, 1, 1);
                    manager.space.Add(box);
                    s = player = new Subject((int)cmd[1], 1, "player", manager.game);
                    player.SetModel(points, lines, box);

                    box.Position = new BEPUutilities.Vector3(-5, -3, -10);
                    box.Orientation = quaternion;
                    box.LinearVelocity = speed;*/
                }
                else if ((int)cmd[12] == 2)
                {
                    if ((int)cmd[1] / 10000 == playerId)
                        bulletsId.Add((int)cmd[1]);
                    s = new Subject((int)cmd[1], 2, "bullet", manager.game);
                    s.SetModel(points, lines, box);
                    box.Mass = 0.01f;
                    box.CollisionInformation.Events.InitialCollisionDetected += BulletHit;
                    s.Destroy(3000);
                }
                else
                {
                    s = new Subject((int)cmd[1], (int)cmd[12], "asteroid", manager.game);
                    if (s.type == 1)
                        s.hp = 3;
                    s.SetModel(points, lines, box);
                    /*Box box = new Box(BEPUutilities.Vector3.Zero, 1, 1, 1, 1);
                    manager.space.Add(box);
                    s = new Subject((int)cmd[1], 0, "asteroid", manager.game);
                    s.SetModel(points, lines, box);

                    box.Position = pos;
                    box.Orientation = quaternion;
                    box.LinearVelocity = speed;*/
                }
                /*else
                {
                    s = new Subject((int)cmd[1], 3, "error", manager.game);
                    s.SetModel(points, lines, box);
                }*/

                manager.subjects.Add(s);
            }

            manager.commands.Remove(cmd);
        }

        public void DestroyObj(float[] cmd)
        {
            //0 - command number, 1 - serial number in array, 2 - id (check)
            try
            {
                if (manager.subjects[(int)cmd[1]].id == (int)cmd[2])
                    manager.subjects[(int)cmd[1]].Destroy();
                else
                {
                    for (int i = 0; i < manager.subjects.Count; i++)
                        if (manager.subjects[i].id == (int)cmd[2])
                        {
                            manager.subjects[i].Destroy();
                            i = manager.subjects.Count;
                        }
                }

                manager.commands.Remove(cmd);
            }
            catch { };
        }

        public void Update(GameTime gameTime)
        {
            if (manager.client.connected && !flag)  //request a unique id
            {
                Thread.Sleep(100);
                manager.client.SendTcp(new float[] { 0.5f });
                flag = true;
            }
            else if (playerId != -1 && !flag2)  //create player
            {
                Vector3 pos = new Vector3(-5, -5, 0);
                Quaternion quat = Quaternion.Identity;
                Vector3 speed = Vector3.Zero;
                manager.client.CreateObj(playerId, pos, quat, speed, 1);
                flag2 = true;
            }

            if (player != null)
            {
                BEPUutilities.Vector3 compensation;
                Vector3 p = control.Update(gameTime, player.entity.LinearVelocity, out compensation);
                if (manager.game.IsActive)
                {
                    player.entity.LinearVelocity += compensation;
                    player.entity.LinearVelocity += new BEPUutilities.Vector3(p.X, p.Y, p.Z);
                }
                BEPUutilities.Vector3 q = player.entity.Position;
                Subject.cameraPos = new Vector3(q.X, q.Y, q.Z);
            }
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
                    Quaternion quat = Quaternion.Identity;
                    float[] data = new float[9] { serialNumber, playerId, pos.X, pos.Y, pos.Z, quat.X, quat.Y, quat.Z, quat.W };
                    manager.client.SendUdp(data);
                }
            }
        }

        public void Draw()
        {
            /*SpriteBatch sb = manager.game.Services.GetService<SpriteBatch>();

            sb.Begin();
            sb.DrawString(manager.font, player.entity.LinearVelocity.ToString(), new Vector2(0, 0), Color.White);
            sb.End();*/
        }

        public void DealDamage(float[] cmd)
        {

            for (int i = 0; i < manager.subjects.Count; i++)
            {
                if (manager.subjects[i].id == cmd[1])
                {
                    manager.subjects[i].hp -= (int)cmd[2];
                }
            }

            manager.commands.Remove(cmd);
            //manager.subjects[cmd[]]
        }
        private void BulletHit(EntityCollidable sender, Collidable other, CollidablePairHandler pair)
        {
            int serialNum1 = -1;    //bullet
            for (int i = 0; i < manager.subjects.Count; i++)
            {
                if (manager.subjects[i].entity == pair.EntityA)
                    serialNum1 = i;
            }
            int serialNum2 = -1;    //collide
            for (int i = 0; i < manager.subjects.Count; i++)
            {
                if (manager.subjects[i].entity == pair.EntityB)
                    serialNum2 = i;
            }
            int id1 = -1, id2 = -1;
            if (serialNum1 != -1)
                id1 = manager.subjects[serialNum1].id;
            if (serialNum2 != -1)
                id2 = manager.subjects[serialNum2].id;

            if (id2 >= 10000)
            {
                int t = serialNum1;
                serialNum1 = serialNum2;
                serialNum2 = t;

                t = id1;
                id1 = id2;
                id2 = t;
            }

            int senderId = id1, otherId = id2;
            if (serialNum1 != -1)
            {
                manager.subjects[serialNum1].Destroy();
                bulletsId.Remove(senderId);

                if (serialNum2 != -1)
                {
                    if (senderId / 10000 == playerId)
                    {
                        //float[] cmd = new float[] { 2, otherId, 1 }; 
                        //0 - cmd type, 1 - id, 2 - damage
                        manager.client.DealDamage(otherId, 1);
                    }
                }
            }
        }
    }
}
