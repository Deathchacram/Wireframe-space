using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace Wireframe_space
{
    class PlayerControl
    {
        MultiplayerManager manager;
        Game game;
        Subject player;
        public static int bulletId = 0;

        /*Point mouseRot = new Point(0, 0);
        Vector3 rot = Vector3.Zero;
        Vector3 pos = new Vector3(0, 0, 0),
            cameraPos = new Vector3(0, 0, -2),
            direction = new Vector3(0, 0, 0),
            up = Vector3.Up;
        private float horizontalAngle = 0, verticalAngle = 0, milisec = 0;*/
        private bool flag = true, flag2 = false, inertiaCompensation = true;

        public PlayerControl(MultiplayerManager manager)
        {
            this.manager = manager;
            game = manager.game;
        }

        public Vector3 Update(GameTime gameTime, BEPUutilities.Vector3 velocity, out BEPUutilities.Vector3 compensation)
        {
            compensation = BEPUutilities.Vector3.Zero;
            if (Keyboard.GetState().IsKeyDown(Keys.I))
                inertiaCompensation = !inertiaCompensation;

            Vector3 delta = Vector3.Zero;
            if (!inertiaCompensation)
            {
                if (game.IsActive)
                {
                    if (Keyboard.GetState().IsKeyDown(Keys.LeftShift))
                        Subject.orientation *= Matrix.CreateRotationZ((float)(1f / gameTime.ElapsedGameTime.TotalMilliseconds) * 10 * 0.017f);
                    if (Keyboard.GetState().IsKeyDown(Keys.Space))
                        Subject.orientation *= Matrix.CreateRotationZ((float)(-1f / gameTime.ElapsedGameTime.TotalMilliseconds) * 10 * 0.017f);
                    Subject.orientation *= Matrix.CreateRotationX((Mouse.GetState().Y - game.GraphicsDevice.Viewport.Height / 2f) / 25f * 0.017f);
                    Subject.orientation *= Matrix.CreateRotationY((Mouse.GetState().X - game.GraphicsDevice.Viewport.Width / 2f) / 25f * 0.017f);
                    //mouseRot = Mouse.GetState().Position;

                    Mouse.SetPosition(game.GraphicsDevice.Viewport.Width / 2, game.GraphicsDevice.Viewport.Height / 2);
                }

                //horizontalAngle = rot.X / 25f;
                //verticalAngle = rot.Y / 25f;

                Vector3 deltaPos = Vector3.Zero;
                if (Keyboard.GetState().IsKeyDown(Keys.W))
                    deltaPos.Z += (float)(1f / gameTime.ElapsedGameTime.TotalMilliseconds);
                if (Keyboard.GetState().IsKeyDown(Keys.S))
                    deltaPos.Z -= (float)(1f / gameTime.ElapsedGameTime.TotalMilliseconds);
                if (Keyboard.GetState().IsKeyDown(Keys.E))
                    deltaPos.Y += (float)(1f / gameTime.ElapsedGameTime.TotalMilliseconds);
                if (Keyboard.GetState().IsKeyDown(Keys.Q))
                    deltaPos.Y -= (float)(1f / gameTime.ElapsedGameTime.TotalMilliseconds);
                if (Keyboard.GetState().IsKeyDown(Keys.D))
                    deltaPos.X -= (float)(1f / gameTime.ElapsedGameTime.TotalMilliseconds);
                if (Keyboard.GetState().IsKeyDown(Keys.A))
                    deltaPos.X += (float)(1f / gameTime.ElapsedGameTime.TotalMilliseconds);

                Matrix m = Matrix.Invert(Subject.orientation);

                //Rotation
                delta.X = deltaPos.X * m.M11 + deltaPos.Y * m.M21 + deltaPos.Z * m.M31 + m.M41;
                delta.Y = deltaPos.X * m.M12 + deltaPos.Y * m.M22 + deltaPos.Z * m.M32 + m.M42;
                delta.Z = deltaPos.X * m.M13 + deltaPos.Y * m.M23 + deltaPos.Z * m.M33 + m.M43;

                //cameraPos += delta * 3;
                //direction += delta;

                //Subject.cameraPos += delta * 1;
                //direction += delta;
            }   //camera and move control
            else
            {
                if (game.IsActive)
                {
                    if (Mouse.GetState().Position.ToVector2().Length() > 10)
                    {
                        Subject.orientation *= Matrix.CreateRotationZ((Mouse.GetState().X - game.GraphicsDevice.Viewport.Width / 2f) / 150f * 0.017f);
                        Subject.orientation *= Matrix.CreateRotationX((Mouse.GetState().Y - game.GraphicsDevice.Viewport.Height / 2f) / 150f * 0.017f);
                        if (Keyboard.GetState().IsKeyDown(Keys.D))
                            Subject.orientation *= Matrix.CreateRotationY((float)(1f / gameTime.ElapsedGameTime.TotalMilliseconds) * 5 * 0.017f);
                        if (Keyboard.GetState().IsKeyDown(Keys.A))
                            Subject.orientation *= Matrix.CreateRotationY((float)(-1f / gameTime.ElapsedGameTime.TotalMilliseconds) * 5 * 0.017f);
                    }

                    //Mouse.SetPosition(game.GraphicsDevice.Viewport.Width / 2, game.GraphicsDevice.Viewport.Height / 2);
                }

                Vector3 deltaPos = Vector3.Zero;
                if (Keyboard.GetState().IsKeyDown(Keys.W))
                    deltaPos.Z += (float)(1f / gameTime.ElapsedGameTime.TotalMilliseconds);
                if (Keyboard.GetState().IsKeyDown(Keys.S))
                    deltaPos.Z -= (float)(1f / gameTime.ElapsedGameTime.TotalMilliseconds);
                /*if (Keyboard.GetState().IsKeyDown(Keys.E))
                    deltaPos.Y += (float)(1f / gameTime.ElapsedGameTime.TotalMilliseconds);
                if (Keyboard.GetState().IsKeyDown(Keys.Q))
                    deltaPos.Y -= (float)(1f / gameTime.ElapsedGameTime.TotalMilliseconds);
                if (Keyboard.GetState().IsKeyDown(Keys.D))
                    deltaPos.X -= (float)(1f / gameTime.ElapsedGameTime.TotalMilliseconds);
                if (Keyboard.GetState().IsKeyDown(Keys.A))
                    deltaPos.X += (float)(1f / gameTime.ElapsedGameTime.TotalMilliseconds);*/

                Matrix m = Matrix.Invert(Subject.orientation);

                delta.X = deltaPos.X * m.M11 + deltaPos.Y * m.M21 + deltaPos.Z * m.M31 + m.M41;
                delta.Y = deltaPos.X * m.M12 + deltaPos.Y * m.M22 + deltaPos.Z * m.M32 + m.M42;
                delta.Z = deltaPos.X * m.M13 + deltaPos.Y * m.M23 + deltaPos.Z * m.M33 + m.M43;

                float deltaTimeSpeed = (float)(1f / gameTime.ElapsedGameTime.TotalMilliseconds);
                //X
                if (velocity.X > deltaTimeSpeed)
                    compensation.X += (delta.X - deltaTimeSpeed);
                else if (velocity.X < -deltaTimeSpeed)
                    compensation.X += (delta.X + deltaTimeSpeed);
                //else
                //    compensation.X = -velocity.X;

                //Y
                if (velocity.Y > deltaTimeSpeed)    //Y
                    compensation.Y += (delta.Y - deltaTimeSpeed);
                else if (velocity.Y < -deltaTimeSpeed)
                    compensation.Y += (delta.Y + deltaTimeSpeed);
                //else
                //    compensation.Y = -velocity.Y;

                //Z
                if (velocity.Z > deltaTimeSpeed)
                    compensation.Z += (delta.Z - deltaTimeSpeed);
                else if (velocity.Z < -deltaTimeSpeed)
                    compensation.Z += (delta.Z + deltaTimeSpeed);
                //else
                //    compensation.Z = velocity.Z;

                //cameraPos += delta * 3;
                //direction += delta;

                //Subject.cameraPos += delta * 1;
            }

            if (Mouse.GetState().LeftButton == ButtonState.Pressed && !flag2)   //create bullet
            {
                flag2 = true;
                Vector3 start = Subject.cameraPos;
                Vector3 end = ScreenToWorld();
                Vector3 speed = Vector3.Normalize(end - start) * 25;
                end = (end - start) * 5 + start;
                Quaternion quat = Quaternion.CreateFromRotationMatrix(Subject.orientation);
                //0 - commandID, 1 - objID, 2-4 - pos, 5-8 - quaternion, 9-11 - speed, 12 - obj type, 13
                float[] command = new float[] { 0, manager.id * 10000 + bulletId, end.X, end.Y, end.Z, quat.X, quat.Y, quat.Z, quat.W, speed.X, speed.Y, speed.Z, 2 };
                if (!manager.isServer)
                {
                    MultiplayerManager.player.CreateObj(command);
                    manager.client.CreateObj(manager.id * 10000 + bulletId, end, quat, speed, 2);
                    bulletId++;
                    bulletId %= 10000;
                }
                else
                    manager.commands.Add(command);
            }
            else if (Mouse.GetState().LeftButton != ButtonState.Pressed && flag2)
                flag2 = false;

            return delta;
        }
        private Vector3 ScreenToWorld()
        {
            Matrix m = Matrix.CreatePerspectiveFieldOfView(3.14f / 3, 1.6667f, 0.1f, 300);
            m = Matrix.Invert(m);
            Point mousePos = Mouse.GetState().Position;
            Vector4 vec = Vector4.Zero;
            vec.X = 1.0f - (2.0f * ((float)mousePos.X / 800));
            vec.Y = (2.0f * ((float)mousePos.Y / 480)) - 1.0f;
            vec.Z = 2.0f * 1 - 1.0f;
            vec.Z = 1;

            Vector4 v;
            v.X = vec.X * m.M11 + vec.Y * m.M21 + vec.Z * m.M31 + vec.W * m.M41;
            v.Y = vec.X * m.M12 + vec.Y * m.M22 + vec.Z * m.M32 + vec.W * m.M42;
            v.Z = vec.X * m.M13 + vec.Y * m.M23 + vec.Z * m.M33 + vec.W * m.M43;
            v.W = vec.X * m.M14 + vec.Y * m.M24 + vec.Z * m.M34 + vec.W * m.M44;
            v.W = 1f / v.W;
            v.X *= v.W;
            v.Y *= v.W;
            v.Z *= v.W;

            m = Matrix.Invert(Subject.orientation);
            v.X = vec.X * m.M11 + vec.Y * m.M21 + vec.Z * m.M31 + vec.W * m.M41;
            v.Y = vec.X * m.M12 + vec.Y * m.M22 + vec.Z * m.M32 + vec.W * m.M42;
            v.Z = vec.X * m.M13 + vec.Y * m.M23 + vec.Z * m.M33 + vec.W * m.M43;
            v.W = vec.X * m.M14 + vec.Y * m.M24 + vec.Z * m.M34 + vec.W * m.M44;

            return new Vector3(v.X, v.Y, v.Z) + Subject.cameraPos;
        }
    }
}
