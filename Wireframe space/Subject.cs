using BEPUphysics.Entities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Wireframe_space
{
    class Subject
    {
        //player settings
        public string name;
        public int id;
        public int type;    // 0 - environment, 1 - player, 2 - bullet
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

        //model settinggs
        public Entity entity;
        public static Effect basicEffect;
        public Vector3 pos;
        public Vector4 color;
        public static Matrix orientation = Matrix.Identity;
        public Vector3 quaternion;

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
        public void Draw()
        {
            Vector3 pos = this.pos;
            if (entity != null)
            {
                BEPUutilities.Vector3 p = entity.Position;
                pos = new Vector3(p.X, p.Y, p.Z);
            }

            basicEffect.Parameters[1].SetValue(Matrix.Identity);
            basicEffect.Parameters[2].SetValue(orientation);
            basicEffect.Parameters[3].SetValue(cameraPos - pos);
            basicEffect.Parameters[4].SetValue(new Vector4(1, 1, 1, 1));
            basicEffect.CurrentTechnique.Passes[0].Apply();

            game.GraphicsDevice.DrawUserIndexedPrimitives(PrimitiveType.LineList, points, 0, points.Length, lines, 0, lines.Length / 2);
        }
        public void SetPosition(Vector3 newPos)
        {
            BEPUutilities.Vector3 speed = new BEPUutilities.Vector3(newPos.X - pos.X, newPos.Y - pos.Y, newPos.Z - pos.Z);
            entity.Position = new BEPUutilities.Vector3(pos.X, pos.Y, pos.Z);
            pos = newPos;
            entity.LinearVelocity = speed * 10;
            //entity.LinearVelocity = BEPUutilities.Vector3.Zero;
        }
    }
}
