using BEPUphysics;
using BEPUphysics.Entities;
using BEPUphysics.Entities.Prefabs;
using BEPUutilities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Myra.Graphics2D.UI;
using System;
using Matrix = Microsoft.Xna.Framework.Matrix;
using Vector2 = Microsoft.Xna.Framework.Vector2;
using Vector3 = Microsoft.Xna.Framework.Vector3;
using Vector4 = Microsoft.Xna.Framework.Vector4;

namespace Wireframe_space
{
    public class Game1 : Game
    {
        private GraphicsDeviceManager graphics;
        private SpriteBatch spriteBatch;
        Space space;
        Menu menu;

        Effect basicEffect;
        bool pressed = false;
        Point mouseRot = new Point(0, 0);
        Vector2 rot = Vector2.Zero;
        Vector3 pos = new Vector3(0, 0, 0),
            cameraPos = new Vector3(0, 0, -40),
            direction = new Vector3(0, 0, 0),
            up = Vector3.Up;
        private float horizontalAngle = 0, verticalAngle = 0;

        VertexPositionColor[] pts;
        short[] lines;
        Desktop desktop;

        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = true;
        }

        protected override void Initialize()
        {
            //Mouse.SetPosition(GraphicsDevice.Viewport.Width / 2, GraphicsDevice.Viewport.Height / 2);
            Components.Add(new MainMenu(this));

            base.Initialize();
        }

        protected override void OnExiting(object sender, EventArgs args)
        {
            foreach (GameComponent m in Components)
                if (m == MultiplayerManager.manager)
                {
                    MultiplayerManager man = (MultiplayerManager)m;
                    man.DestroyNet();
                }
        }

        protected override void LoadContent()
        {
            spriteBatch = new SpriteBatch(GraphicsDevice);
            Services.AddService(spriteBatch);

            /*FileStream file = File.Open(@"C://Folder/Big_asteroid2.dat", FileMode.Open);
            var bf = new XmlSerializer(typeof(CustomMesh));
            CustomMesh model = bf.Deserialize(file) as CustomMesh;
            pts = model.GetPoints();
            lines = model.lines;

            space = new Space();
            space.ForceUpdater.Gravity = BEPUutilities.Vector3.Zero;

            Box box = new Box(BEPUutilities.Vector3.Zero, 1, 1, 1, 1);
            box.AngularVelocity = new BEPUutilities.Vector3(1, 1, 0);
            space.Add(box);*/
        }

        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            // TODO: Add your update logic here

            /*space.Entities[0].AngularMomentum = new BEPUutilities.Vector3(0.01f, 0, 0);
            space.Update();*/

            base.Update(gameTime);
        }
        protected override void Draw(GameTime gameTime)
        {

            base.Draw(gameTime);
        }
    }

    [Serializable]
    public struct Vec3
    {
        public float x;
        public float y;
        public float z;
        public Vec3(float x, float y, float z)
        {
            this.x = x;
            this.y = y;
            this.z = z;
        }
        public Vec3(Vector3 v)
        { x = v.X; y = v.Y; z = v.Z; }
    }
    [Serializable]
    public class CustomMesh
    {
        public Vec3 color;
        public Vec3[] points;
        public short[] polygons;
        public short[] lines;
        public CustomMesh()
        {
            color = new Vec3(1, 1, 1);
            points = new Vec3[0];
            lines = new short[0];
            polygons = new short[0];
        }
        public CustomMesh(VertexPositionColor[] vecs, short[] poly, short[] line, Color color)
        {
            this.color = new Vec3(color.R, color.G, color.B);
            Vec3[] pnts = new Vec3[vecs.Length];
            for (int i = 0; i < pnts.Length; i++)
            {
                pnts[i] = new Vec3(vecs[i].Position);
            }
            points = pnts;
            polygons = poly;
            lines = line;
        }
        public VertexPositionColor[] GetPoints()
        {
            VertexPositionColor[] pts = new VertexPositionColor[points.Length];
            for (int i = 0; i < pts.Length; i++)
                pts[i] = new VertexPositionColor(new Vector3(points[i].x, points[i].y, points[i].z), new Color(color.x, color.y, color.z));
            return pts;
        }
    }
    public class Model
    {
        public static Game game;
        public static Effect basicEffect;

        public Vector3 pos = new Vector3(0, 0, 0);
        public Entity entity;
        public float scale = 1;

        private VertexPositionColor[] points;
        private short[] polygons;
        private short[] lines;

        public Model(CustomMesh mesh)
        {
            points = mesh.GetPoints();
            polygons = mesh.polygons;
            lines = mesh.lines;
        }
        public Model(CustomMesh mesh, Vector3 pos)
        {
            points = mesh.GetPoints();
            polygons = mesh.polygons;
            lines = mesh.lines;
            this.pos = pos;
        }
        public void SetEntity(Space space, float mass)
        {
            BEPUutilities.Vector3[] vecs = new BEPUutilities.Vector3[points.Length];

            for (int i = 0; i < vecs.Length; i++)
                vecs[i] = new BEPUutilities.Vector3(points[i].Position.X, points[i].Position.Y, points[i].Position.Z);

            int[] indices = new int[polygons.Length];
            for (int i = 0; i < indices.Length; i++)
                indices[i] = (int)polygons[i];

            entity = new MobileMesh(vecs, indices, new AffineTransform(new BEPUutilities.Vector3(0, 0, 0)), BEPUphysics.CollisionShapes.MobileMeshSolidity.DoubleSided);
            entity.BecomeDynamic(mass);
            space.Add(entity);
        }
        public void Draw(Vector3 cameraPos, float horizontalAngle, float verticalAngle)
        {
            Matrix matrix = Matrix.Identity;
            if (entity != null)
            {
                BEPUutilities.Vector3 p = entity.Position;
                pos = new Vector3(p.X, p.Y, p.Z);

                BEPUutilities.Matrix3x3 m = entity.OrientationMatrix;
                matrix = new Matrix(m.M11, m.M12, m.M13, 0,
                    m.M21, m.M22, m.M23, 0,
                    m.M31, m.M32, m.M33, 0,
                    0, 0, 0, 1);
            }

            SpriteBatch sb = game.Services.GetService<SpriteBatch>();
            //sb.Begin();
            basicEffect.Parameters[1].SetValue(matrix);
            basicEffect.Parameters[2].SetValue(Matrix.CreateRotationY(-horizontalAngle * 0.017f) * Matrix.CreateRotationX(verticalAngle * 0.017f));
            basicEffect.Parameters[3].SetValue(cameraPos - pos);
            basicEffect.Parameters[4].SetValue(new Vector4(1, 1, 1, 1));
            basicEffect.CurrentTechnique.Passes[0].Apply();

            game.GraphicsDevice.DrawUserIndexedPrimitives(PrimitiveType.LineList, points, 0, points.Length, lines, 0, lines.Length / 2);
            //sb.End();
        }
    }
}
