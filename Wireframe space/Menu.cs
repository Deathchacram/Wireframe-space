using BEPUphysics;
using BEPUphysics.Entities;
using BEPUphysics.UpdateableSystems.ForceFields;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Myra.Graphics2D.UI;
using System;
using System.Collections.Generic;
using System.IO;
using System.Xml.Serialization;

namespace Wireframe_space
{
    class Menu
    {
        Point mouseRot = new Point(0, 0);
        Vector2 rot = Vector2.Zero;
        public float horizontalAngle = 0, verticalAngle = 0;
        private Space space;
        private List<Model> models = new List<Model>();
        private List<float> modelPos = new List<float>();
        private List<float> modelPosOriginal = new List<float>();
        private float delta = 0.00001f;

        public void CreateMenuScene()
        {
            space = new Space();
            space.ForceUpdater.Gravity = BEPUutilities.Vector3.Zero;

            var exePath = AppDomain.CurrentDomain.BaseDirectory;//path to exe file
            var path = Path.Combine(exePath, "Content\\models\\environment\\big_asteroid_1.dat");

            //FileStream file = File.Open(@"C://Folder/big_asteroid_1.dat", FileMode.Open);
            FileStream file = File.Open(path, FileMode.Open);
            var bf = new XmlSerializer(typeof(CustomMesh));
            CustomMesh model = bf.Deserialize(file) as CustomMesh;
            models.Add(new Model(model, new Vector3(0, 0, 100)));
            //models[0].SetEntity(space, 5000);
            //space.Entities[0].Position = new BEPUutilities.Vector3(0, 0, 100);

            //GravitationField gf = new GravitationField(new InfiniteForceFieldShape(), new Vector3(0, 0, 100), 50);
            //space.Add(gf);
            file.Close();

            path = Path.Combine(exePath, "Content\\models\\environment\\asteroid_1.dat");
            file = File.Open(path, FileMode.Open);
            bf = new XmlSerializer(typeof(CustomMesh));
            model = bf.Deserialize(file) as CustomMesh;
            Random rnd = new Random();
            for (int i = 0; i < 150; i++)
            {
                float rand = (float)rnd.NextDouble();
                float x = 65 * (float)Math.Sin(rand * 62.831f) + (rand - 0.5f) * 15;
                float y = rand * 10;
                float z = 55 * (float)Math.Cos(rand * 62.831f) + (rand - 0.5f) * 15 + 100;
                Model mdl = new Model(model, new Vector3(x, y, z));
                models.Add(mdl);
                modelPos.Add(rand);
                rand = (float)rnd.NextDouble();
                modelPos.Add(rand);
                //models[i + 1].SetEntity(space, 1);
                //space.Entities[i].Position = new BEPUutilities.Vector3(50 * (float)Math.Sin(rand * 61.8f) + rand * 10, rand * 10, 50 * (float)Math.Cos(rand * 61.8f) + rand * 10 + 100);
                //space.Entities[i].LinearVelocity = new BEPUutilities.Vector3(-5 * (float)Math.Cos(rand * 61.8f), 0, 5 * (float)Math.Sin(rand * 61.8f));
            }
            modelPosOriginal = new List<float>(modelPos);
            file.Close();

            /*models.Add(new Model(model));
            models[2].SetEntity(space, 1);
            space.Entities[2].Position = new BEPUutilities.Vector3(0, -50, 150);
            space.Entities[2].LinearVelocity = new BEPUutilities.Vector3(3, 0, 0);*/

            //file = File.Open(@"C://Folder/asteroid.dat", FileMode.Open);
            //bf = new XmlSerializer(typeof(CustomMesh));
            //model = bf.Deserialize(file) as CustomMesh;
            /*for (int i = 0; i < 4; i++)
            {
                models.Add(new Model(model));
                models[i + 1].SetEntity(space);
                models[i + 1].entity.Position = new BEPUutilities.Vector3(i * 3, 0, 0);
                models[i + 1].entity.LinearVelocity = new BEPUutilities.Vector3(-i * 3, 0, 0);
            }*/

        }
        public void MenuSceneUpdate()
        {
            /*rot.X += game.GraphicsDevice.Viewport.Width / 2 - Mouse.GetState().X;
            rot.Y -= game.GraphicsDevice.Viewport.Height / 2 - Mouse.GetState().Y;
            mouseRot = Mouse.GetState().Position;
            Mouse.SetPosition(game.GraphicsDevice.Viewport.Width / 2, game.GraphicsDevice.Viewport.Height / 2);

            horizontalAngle = rot.X / 4;
            verticalAngle = rot.Y / 4;
*/
            for (int i = 0; i < models.Count - 1; i++)
            {
                modelPos[i] = (modelPos[i] + delta) % 1;
                float rand = modelPos[i];
                float x = 65 * (float)Math.Sin(rand * 62.831f) + (modelPosOriginal[i * 2] - 0.5f) * 15;
                float y = (float)Math.Sin(rand * 62.831f) * 10;
                float z = 55 * (float)Math.Cos(rand * 62.831f) + (modelPosOriginal[i * 2 + 1] - 0.5f) * 15 + 100;
                Vector3 newPos = new Vector3(x, y, z);
                models[i + 1].pos = newPos;
            }
            //space.Update();
        }
        public void MenuSceneDraw()
        {
            for (int i = 0; i < models.Count; i++)
            {
                models[i].Draw(new Vector3(0, -50, 0), horizontalAngle, verticalAngle + 30);
            }
        }
        public static Desktop MainMenu(int screenWidth, int screenHeight)
        {
            Desktop desktop = new Desktop();
            TextButton button1 = new TextButton
            {
                Left = (int)(screenWidth * 0.025f),
                Top = (int)(screenHeight * 0.635f - screenHeight * 0.12f),
                Width = (int)(screenWidth * 0.2f),
                Height = (int)(screenHeight * 0.1f),
                Text = "Подключение"
            };
            desktop.Widgets.Add(button1);
            TextButton button2 = new TextButton
            {
                Left = (int)(screenWidth * 0.025f),
                Top = (int)(screenHeight * 0.635f),
                Width = (int)(screenWidth * 0.2f + screenWidth * 0.02f),
                Height = (int)(screenHeight * 0.1f),
                Text = "Создать игру"
            };
            desktop.Widgets.Add(button2);
            TextButton button3 = new TextButton
            {
                Left = (int)(screenWidth * 0.025f),
                Top = (int)(screenHeight * 0.635f + screenHeight * 0.12f),
                Width = (int)(screenWidth * 0.2f + screenWidth * 0.04f),
                Height = (int)(screenHeight * 0.1f),
                Text = "Не работает"
            };
            desktop.Widgets.Add(button3);
            TextButton button4 = new TextButton
            {
                Left = (int)(screenWidth * 0.025f),
                Top = (int)(screenHeight * 0.635f + screenHeight * 0.24f),
                Width = (int)(screenWidth * 0.2f + screenWidth * 0.06f),
                Height = (int)(screenHeight * 0.1f),
                Text = "Не работает"
            };
            desktop.Widgets.Add(button4);
            //button.Visible = false;
            return desktop;
        }
        public static Desktop SettingsMenu(int screenWidth, int screenHeight)
        {
            Desktop desktop = new Desktop();
            return desktop;
        }
        class GravitationField : ForceField
        {
            Vector3 pos;
            float force;
            public GravitationField(ForceFieldShape shape, Vector3 pos, float force) : base(shape)
            {
                this.force = force;
                this.pos = pos;
            }

            protected override void CalculateImpulse(Entity e, float dt, out BEPUutilities.Vector3 impulse)
            {
                BEPUutilities.Vector3 newVec = new BEPUutilities.Vector3(pos.X, pos.Y, pos.Z) - e.Position;
                impulse = BEPUutilities.Vector3.Normalize(newVec) * force * dt / (1 + newVec.Length() * newVec.Length());
                //impulse = new BEPUutilities.Vector3(pos.X, pos.Y, pos.Z) * dt;
            }
        }
    }
}
