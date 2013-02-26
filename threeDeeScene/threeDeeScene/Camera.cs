using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using Microsoft.Xna.Framework.Net;
using Microsoft.Xna.Framework.Storage;



namespace ThreeDeeScene
{

    public class Camera : Microsoft.Xna.Framework.GameComponent
    {

        public Matrix view { get; protected set; }
        public Matrix projection { get; protected set; }
        // camera vectors
        public Vector3 cameraPosition { get; protected set; }
        Vector3 cameraDirection;
        Vector3 cameraUp;
        MouseState prevMouseState;
        public Camera(Game game, Vector3 pos, Vector3 target, Vector3 up)
            : base(game)
        {
           // build camera view matrix
            cameraPosition = pos;
            cameraDirection = target - pos;
            cameraDirection.Normalize();
            cameraUp = up;
            CreateLookAt();
            projection = Matrix.CreatePerspectiveFieldOfView(MathHelper.PiOver4, (float)Game.Window.ClientBounds.Width / (float)Game.Window.ClientBounds.Height, .1f, 30000);
        }

   
        public override void Initialize()
        {
           // set mouse position and do initial get state
            Mouse.SetPosition(Game.Window.ClientBounds.Width / 2, Game.Window.ClientBounds.Height / 2);
            prevMouseState = Mouse.GetState();



            base.Initialize();
        }

        public override void Update(GameTime gameTime)
        {

          //  cameraPosition += cameraDirection;
            //Yaw Rotation
            //cameraDirection = Vector3.Transform(cameraDirection, Matrix.CreateFromAxisAngle(cameraUp, (MathHelper.PiOver4 / 150) * (Mouse.GetState().X - prevMouseState.X)));

       //    cameraDirection = new Vector3(30, 30,100);
                
                CreateLookAt();
          
            //Roll rotation
            //if (Mouse.GetState().LeftButton == ButtonState.Pressed)
            //{
            //    cameraUp = Vector3.Transform(cameraUp, Matrix.CreateFromAxisAngle(cameraDirection, MathHelper.PiOver4 / 45));

            //}
            // //reset preMouseState
            prevMouseState = Mouse.GetState();

            base.Update(gameTime);
        }
        private void CreateLookAt()
        {
        view = Matrix.CreateLookAt(cameraPosition,cameraPosition +cameraDirection, cameraUp);}
    }
}
