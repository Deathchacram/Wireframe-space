using BEPUphysics;
using BEPUphysics.Entities;
using BEPUphysics.Entities.Prefabs;
using BEPUutilities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Myra;
using Myra.Graphics2D.UI;
using System;
using System.Net;
using System.Net.Sockets;
using Matrix = Microsoft.Xna.Framework.Matrix;
using Vector2 = Microsoft.Xna.Framework.Vector2;
using Vector3 = Microsoft.Xna.Framework.Vector3;
using Vector4 = Microsoft.Xna.Framework.Vector4;

namespace Wireframe_space
{
    class MainMenu : DrawableGameComponent
    {
        Game game;
        Desktop desktop;
        Menu menu;
        Effect basicEffect;

        public MainMenu(Game game) : base(game)
        {
            this.game = game;
        }
        protected override void LoadContent()
        {

            basicEffect = game.Content.Load<Effect>("BaseEffect");
            basicEffect.Parameters[0].SetValue(Matrix.CreatePerspectiveFieldOfView(3.14f / 3, 1.6667f, 0.1f, 300));

            MyraEnvironment.Game = game;
            Model.basicEffect = basicEffect;
            Model.game = game;

            desktop = Menu.MainMenu(800, 480);
            desktop.Widgets[0].TouchDown += (s, a) => ConnectToServer();
            desktop.Widgets[1].TouchDown += (s, a) => CreateServer();

            menu = new Menu();
            menu.CreateMenuScene();
        }
        public override void Update(GameTime gameTime)
        {
            menu.MenuSceneUpdate();
            // TODO: Add your update logic here

            /*space.Entities[0].AngularMomentum = new BEPUutilities.Vector3(0.01f, 0, 0);
            space.Update();*/

            base.Update(gameTime);
        }
        public override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.Black);

            menu.MenuSceneDraw();
            desktop.Render();

            base.Draw(gameTime);
        }

        private void ConnectToServer()
        {
            TextBox tb = (TextBox)desktop.Widgets[4];
            MultiplayerManager mManager = new MultiplayerManager(game, tb.Text);
            mManager.isServer = false;
            game.Components.Add(mManager);
            game.Components.Remove(this);
        }
        private void CreateServer()
        {
            MultiplayerManager mManager = new MultiplayerManager(game);
            mManager.isServer = true;
            game.Components.Add(mManager);
            game.Components.Remove(this);

        }
    }
}
