using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Text;

namespace Wireframe_space
{
    class PlayerControl
    {
        MultiplayerManager manager;
        Game game;
        Subject player;

        Point mouseRot = new Point(0, 0);
        Vector3 rot = Vector3.Zero;
        Vector3 pos = new Vector3(0, 0, 0),
            cameraPos = new Vector3(0, 0, -2),
            direction = new Vector3(0, 0, 0),
            up = Vector3.Up;
        private float horizontalAngle = 0, verticalAngle = 0, milisec = 0;
        private bool flag = true, inertiaCompensation = true;

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

            if(!inertiaCompensation)
            {
                /*if (Keyboard.GetState().IsKeyDown(Keys.N) && !press)
                {
                    flag = !flag;
                    press = true;
                }
                else if (!Keyboard.GetState().IsKeyDown(Keys.N))
                    press = false;*/

                if (flag)
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

                Vector3 delta = Vector3.Zero;
                //Matrix m = Matrix.CreateRotationY(horizontalAngle * 0.017f) * Matrix.CreateRotationX(-verticalAngle * 0.017f) * Matrix.CreateRotationZ(-rot.Z * 0.017f);
                Matrix m = Matrix.Invert(Subject.orientation);

                //Rotation
                /*Matrix m = new Matrix(
                        new Vector4((float)Math.Cos(horizontalAngle * 0.01745f), 0, -(float)Math.Sin(horizontalAngle * 0.01745f), 0),
                        new Vector4(0, 1, 0, 0),
                        new Vector4((float)Math.Sin(horizontalAngle * 0.01745f), 0, (float)Math.Cos(horizontalAngle * 0.01745f), 0),
                        new Vector4(0, 0, 0, 1));*/
                delta.X = deltaPos.X * m.M11 + deltaPos.Y * m.M21 + deltaPos.Z * m.M31 + m.M41;
                delta.Y = deltaPos.X * m.M12 + deltaPos.Y * m.M22 + deltaPos.Z * m.M32 + m.M42;
                delta.Z = deltaPos.X * m.M13 + deltaPos.Y * m.M23 + deltaPos.Z * m.M33 + m.M43;

                cameraPos += delta * 3;
                direction += delta;

                //Subject.cameraPos += delta * 1;
                return delta;
                //direction += delta;
            }   //camera and move control
            else
            {
                if (flag)
                {
                    if (Mouse.GetState().Position.ToVector2().Length() > 10)
                    {
                        Subject.orientation *= Matrix.CreateRotationZ((Mouse.GetState().X - game.GraphicsDevice.Viewport.Width / 2f) / 150f * 0.017f);
                        Subject.orientation *= Matrix.CreateRotationX((Mouse.GetState().Y - game.GraphicsDevice.Viewport.Height / 2f) / 150f * 0.017f);
                        if (Keyboard.GetState().IsKeyDown(Keys.A))
                            Subject.orientation *= Matrix.CreateRotationY((float)(1f / gameTime.ElapsedGameTime.TotalMilliseconds) * 15 * 0.017f);
                        if (Keyboard.GetState().IsKeyDown(Keys.D))
                            Subject.orientation *= Matrix.CreateRotationY((float)(-1f / gameTime.ElapsedGameTime.TotalMilliseconds) * 15 * 0.017f);
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

                Vector3 delta = Vector3.Zero;
                Matrix m = Matrix.Invert(Subject.orientation);

                delta.X = deltaPos.X * m.M11 + deltaPos.Y * m.M21 + deltaPos.Z * m.M31 + m.M41;
                delta.Y = deltaPos.X * m.M12 + deltaPos.Y * m.M22 + deltaPos.Z * m.M32 + m.M42;
                delta.Z = deltaPos.X * m.M13 + deltaPos.Y * m.M23 + deltaPos.Z * m.M33 + m.M43;

                float deltaTimeSpeed = (float)(1f / gameTime.ElapsedGameTime.TotalMilliseconds);
                //X
                if (velocity.X > deltaTimeSpeed)    
                    compensation.X += (delta.X - deltaTimeSpeed);
                else if(velocity.X < -deltaTimeSpeed)
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
                direction += delta;

                //Subject.cameraPos += delta * 1;
                return delta;
            }

        }
    }
}
