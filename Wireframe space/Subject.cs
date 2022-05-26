using BEPUphysics;
using BEPUphysics.Entities;
using BEPUphysics.Entities.Prefabs;
using BEPUutilities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Matrix = Microsoft.Xna.Framework.Matrix;
using Vector3 = Microsoft.Xna.Framework.Vector3;
using Vector4 = Microsoft.Xna.Framework.Vector4;

namespace Wireframe_space
{
    class Subject
    {
        //player settings
        public int hp = 1;
        public string name;
        public int id;
        public int type;    // 0 - environment, 1 - player, 2 - bullet
        public bool isActive = true;
        Game game;

        //Default model
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
        //private short[] polygons;

        //world
        //Space space;
        //List<Subject> subjects;
        //List<int> idCollection;
        //List<Subject> toDestroy;

        //model settings
        public Entity entity;
        public static Effect basicEffect;
        public Vector3 pos;
        public Vector4 color;
        public static Matrix orientation = Matrix.Identity;
        public Vector4 quaternion;
        public float scale = 1;

        //camera settings
        public static Vector3 rot = Vector3.Zero;
        public static Vector3 cameraPos = new Vector3(-5, -3, -10);
        public static float horizontalAngle = 0, verticalAngle = 0;

        public Subject(int id, int type, string name, Game game)
        {
            this.id = id;
            this.type = type;
            this.name = name;
            this.game = game;
        }
        public void SetModel(VertexPositionColor[] points, short[] lines, Entity entity)
        {
            this.points = points;
            this.lines = lines;
            this.entity = entity;
        }
        /*public void SetModel(float mass, string name)
        {
            BEPUutilities.Vector3[] vecs = new BEPUutilities.Vector3[points.Length];

            for (int i = 0; i < vecs.Length; i++)
                vecs[i] = new BEPUutilities.Vector3(points[i].Position.X, points[i].Position.Y, points[i].Position.Z);

            int[] indices = new int[polygons.Length];
            for (int i = 0; i < indices.Length; i++)
                indices[i] = (int)polygons[i];

            entity = new MobileMesh(vecs, indices, new AffineTransform(new BEPUutilities.Vector3(0, 0, 0)), BEPUphysics.CollisionShapes.MobileMeshSolidity.DoubleSided);
            entity.BecomeDynamic(mass);
            MultiplayerManager.manager.space.Add(entity);
        }*/
        public void Draw()
        {
            if (isActive)
            {
                Vector3 pos = this.pos;
                if (entity != null)
                {
                    BEPUutilities.Vector3 p = entity.Position;
                    pos = new Vector3(p.X, p.Y, p.Z);
                }

                /*if (quaternion != null)
                {
                    Quaternion q = new Quaternion(quaternion.X, quaternion.Y, quaternion.Z, quaternion.W);
                    basicEffect.Parameters[1].SetValue(Matrix.CreateFromQuaternion(q)); 
                }
                else
                {*/
                var m = entity.OrientationMatrix;
                Matrix matrix = new Matrix(m.M11, m.M12, m.M13, 0,
                    m.M21, m.M22, m.M23, 0,
                    m.M31, m.M32, m.M33, 0,
                    0, 0, 0, 1);
                basicEffect.Parameters[1].SetValue(matrix);
                //}

                basicEffect.Parameters[2].SetValue(orientation);
                basicEffect.Parameters[3].SetValue(cameraPos - pos);
                if (type == 0)
                    basicEffect.Parameters[4].SetValue(new Vector4(1, 1, 1, 1));
                else if (type == 1)
                    basicEffect.Parameters[4].SetValue(new Vector4(0, 1, 0, 1));
                else if (type == 2)
                    basicEffect.Parameters[4].SetValue(new Vector4(1, 0.8f, 0, 1));
                else if (type == 3)
                    basicEffect.Parameters[4].SetValue(new Vector4(1, 0, 0, 1));

                basicEffect.CurrentTechnique.Passes[0].Apply();

                game.GraphicsDevice.DrawUserIndexedPrimitives(PrimitiveType.LineList, points, 0, points.Length, lines, 0, lines.Length / 2);
            }
        }
        public void SetPosition(Vector3 newPos)
        {
            if (isActive)
            {
                BEPUutilities.Vector3 speed = new BEPUutilities.Vector3(newPos.X - pos.X, newPos.Y - pos.Y, newPos.Z - pos.Z);
                entity.Position = new BEPUutilities.Vector3(pos.X, pos.Y, pos.Z);
                pos = newPos;
                entity.LinearVelocity = speed * 10;
                //entity.LinearVelocity = BEPUutilities.Vector3.Zero;
            }
        }
        public void SetPosition(Vector3 newPos, Vector4 quat)
        {
            if (isActive)
            {
                BEPUutilities.Vector3 speed = new BEPUutilities.Vector3(newPos.X - pos.X, newPos.Y - pos.Y, newPos.Z - pos.Z);
                entity.Position = new BEPUutilities.Vector3(pos.X, pos.Y, pos.Z);
                pos = newPos;
                entity.LinearVelocity = speed * 10;

                BEPUutilities.Quaternion q = new BEPUutilities.Quaternion(quat.X, quat.Y, quat.Z, quat.W);
                entity.Orientation = q;
                //entity.LinearVelocity = BEPUutilities.Vector3.Zero;
            }
        }
        public void Destroy()
        {
            MultiplayerManager.manager.toDestroy.Remove(this);
            MultiplayerManager.manager.idCollection.Remove(id);
            MultiplayerManager.manager.subjects.Remove(this);

            isActive = false;

            if (entity != null)
            {
                MultiplayerManager.manager.space.Remove(entity);
            }
            /*MultiplayerManager.manager.toDestroy.Remove(this);
            MultiplayerManager.manager.idCollection.Remove(id);
            subjects.Remove(this);

            isActive = false;

            if (entity != null)
            {
                MultiplayerManager.manager.space.Remove(entity);
            }*/
        }
        public void Destroy(int time)
        {
            Task t = new Task(() =>
            {
                Thread.Sleep(time);
                if (isActive)
                {
                    /*MultiplayerManager.manager.idCollection.Remove(id);
                    MultiplayerManager.manager.subjects.Remove(this);

                    isActive = false;
                    if (entity != null)
                    {
                            MultiplayerManager.manager.space.Remove(entity);
                    }*/
                    MultiplayerManager.manager.toDestroy.Add(this);
                }
            });
            t.Start();
        }
    }
}
