using BEPUphysics.BroadPhaseEntries;
using BEPUphysics.BroadPhaseEntries.MobileCollidables;
using BEPUphysics.Entities.Prefabs;
using BEPUphysics.NarrowPhaseSystems.Pairs;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;

namespace Wireframe_space.NetCode
{
    class PlayerServer : IPlayer
    {
        public int playerId = -1;
        public static Subject player;

        private MultiplayerManager manager;
        private List<int> bulletsId = new List<int>();
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

        BEPUutilities.Vector3 compensation = BEPUutilities.Vector3.Zero;

        public PlayerServer(MultiplayerManager manager, Server server)
        {
            this.manager = manager;
            this.server = server;

            playerId = manager.id = manager.FindFreeId();
            Quaternion q = Quaternion.Identity;
            float[] cmd = new float[13] { 0, playerId, 0, 0, 0, q.X, q.Y, q.Z, q.W, 0, 0, 0, 1 };   //create player
            //manager.commands.Add(cmd);
            CreateObj(cmd);

            control = new PlayerControl(manager, player);
        }

        public void CreateObj(float[] cmd)
        {
            //0 - commandID, 1 - objID, 2-4 - pos, 5-8 - quaternion, 9-11 - speed, 12 - obj type
            BEPUutilities.Vector3 pos = new BEPUutilities.Vector3(cmd[2], cmd[3], cmd[4]);
            Vector3 posv = new Vector3(cmd[2], cmd[3], cmd[4]);
            BEPUutilities.Quaternion quaternion = new BEPUutilities.Quaternion(cmd[5], cmd[6], cmd[7], cmd[8]);
            BEPUutilities.Vector3 speed = new BEPUutilities.Vector3(cmd[9], cmd[10], cmd[11]);
            Vector3 speedv = new Vector3(cmd[9], cmd[10], cmd[11]);

            Subject s;
            Box box = new Box(BEPUutilities.Vector3.Zero, 1, 1, 1, 1);
            manager.space.Add(box);

            box.Position = pos;
            box.Orientation = quaternion;
            box.LinearVelocity = speed;

            if (cmd[1] == playerId)
            {
                s = player = new Subject((int)cmd[1], 1, "player", manager.game);
                player.SetModel(points, lines, box);
                player.hp = 3;

                box.Position = new BEPUutilities.Vector3(-5, -3, -10);
                box.Orientation = BEPUutilities.Quaternion.Identity;
                box.LinearVelocity = speed;
            }
            else if(cmd[12] == 2)
            {
                s = new Subject((int)cmd[1], 2, "bullet", manager.game);
                s.SetModel(points, lines, box);
                box.Mass = 0.01f;
                box.CollisionInformation.Events.InitialCollisionDetected += BulletHit;
                s.Destroy(3000);
            }
            else
            {
                s = new Subject((int)cmd[1], (int)cmd[12], "asteroid", manager.game);
                s.SetModel(points, lines, box);
            }
            if (s.type == 1)
                s.hp = 3;

            manager.subjects.Add(s);
            server.CreateObj((int)cmd[1], posv, quaternion, speedv, (int)cmd[12]);

            manager.commands.Remove(cmd);
        }

        public void DestroyObj(float[] cmd)
        {
            //0 - command number, 1 - serial number in array, 2 - id (check)

            //manager.space.Remove(manager.space.Entities[(int)cmd[1]]);
            //manager.subjects.RemoveAt((int)cmd[1]);

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

            server.DestroyObj((int)cmd[1], (int)cmd[2]);

            manager.commands.Remove(cmd);

            if((int)cmd[2] == playerId)
            {
                playerId = manager.id = manager.FindFreeId();
                Quaternion q = Quaternion.Identity;
                float[] c = new float[13] { 0, playerId, 0, 0, 0, q.X, q.Y, q.Z, q.W, 0, 0, 0, 1 };   //create player
                                                                                                      //manager.commands.Add(cmd);
                CreateObj(c);
                control.player = player;
            }
        }

        public void Update(GameTime gameTime)
        {

            if (player != null)    //update player speed
            {
                Vector3 p = control.Update(gameTime, player.entity.LinearVelocity, out compensation);
                if (manager.game.IsActive)
                {
                    player.entity.LinearVelocity += compensation;
                    player.entity.LinearVelocity += new BEPUutilities.Vector3(p.X, p.Y, p.Z);
                }
                BEPUutilities.Vector3 q = player.entity.Position;
                Subject.cameraPos = new Vector3(q.X, q.Y, q.Z); //update camera pos
            }
            for (int i = 0; i < manager.subjects.Count; i++)    //debug 25 cubes
            {
                //if (manager.subjects[i].type != 1)
                //manager.subjects[i].entity.LinearVelocity = new BEPUutilities.Vector3(-0.1f * i, 0, 0);
                //manager.subjects[i].entity.LinearVelocity = new BEPUutilities.Vector3(0, 0, 0);
            }
        }

        public void Draw()
        {
            SpriteBatch sb = manager.game.Services.GetService<SpriteBatch>();

            string speed = "X: " + player.entity.LinearVelocity.X.ToString("0.000") + " Y: " + player.entity.LinearVelocity.Y.ToString("0.000") + " Z: " + player.entity.LinearVelocity.Z.ToString("0.000");
            string comp = "X: " + compensation.X.ToString("0.000") + " Y: " + compensation.Y.ToString("0.000") + " Z: " + compensation.Z.ToString("0.000");

            sb.Begin();
            ///sb.DrawString(manager.font, speed, new Vector2(0, 0), Color.White);
            //sb.DrawString(manager.font, comp, new Vector2(20, 30), Color.White);
            sb.DrawString(manager.font, player.hp.ToString(), new Vector2(20, 30), Color.White);
            sb.End();
        }

        public void DealDamage(float[] cmd)
        {
            //0 - cmd type, 1 - id, 2 - damage

            for (int i = 0; i < manager.subjects.Count; i++)
            {
                if (manager.subjects[i].id == cmd[1])
                {
                    manager.subjects[i].hp -= (int)cmd[2];

                    //if hp <= 0
                    if (manager.subjects[i].hp <= 0)
                    {
                        //destroy
                        //manager.commands.Add(new float[3] { 1, i, cmd[1] });
                        DestroyObj(new float[3] { 1, i, cmd[1] });
                        //manager.subjects[i].Destroy();
                    }
                    i = manager.subjects.Count;
                }
            }

            /*if (manager.subjects[(int)cmd[1]].hp <= 0)
                for (int i = 0; i < manager.subjects.Count; i++)
                    if (manager.subjects[i].id == cmd[1])
                    {
                        //destroy
                        manager.commands.Add(new float[3] { 1, i, cmd[1] });
                        manager.subjects[i].Destroy();
                    }
            */

            manager.server.DealDamage((int)cmd[1], (int)cmd[2]);
            manager.commands.Remove(cmd);
        }
        private void BulletHit(EntityCollidable sender, Collidable other, CollidablePairHandler pair)
        {
            /*int senderId = manager.space.Entities.IndexOf(sender.Entity);
            int otherId;
            if (sender.Entity != pair.EntityB)
                otherId = manager.space.Entities.IndexOf(pair.EntityB);
            else
                otherId = manager.space.Entities.IndexOf(pair.EntityA);*/

            int serialNum1 = -1;    //bullet
            for (int i = 0; i < manager.subjects.Count; i++)
            {
                if (manager.subjects[i].entity == pair.EntityA)
                    serialNum1 = i;
            }
            //int serialNum2 = manager.space.Entities.IndexOf(pair.EntityB);
            int serialNum2 = -1;    //collide
            for(int i = 0; i < manager.subjects.Count; i++)
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

                if (serialNum2 != -1)
                {
                    if (senderId / 10000 == playerId)
                    {
                        float[] cmd = new float[] { 2, otherId, 1 };
                        //0 - cmd type, 1 - id, 2 - damage
                        //manager.commands.Add(cmd);
                        DealDamage(cmd);
                    }
                }
            }

        }
    }
}
