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
using Microsoft.Kinect;

namespace ThreeDeeScene
{
    /// <summary>
    /// This is the main type for your game
    /// </summary>
    public class Game1 : Microsoft.Xna.Framework.Game
    {

        private KinectSensor _KinectDevice;
      //  private Int32Rect _GreenScreenImageRect; This was in the chapter on greenscreen but it needs a reference

        private short[] _DepthPixelData;
        private byte[] _ColorPixelData;
        SpriteBatch spriteBatch;

        Camera camera;
        GraphicsDeviceManager graphics;
        Texture2D colorVideo;
        Texture2D texture;
        BasicModel thisModel;
        int i;
        private ColorImagePoint[] colorCoordinates;
        private Array greenScreenPixelData;
        private Skeleton[] _SkeletonData;
        private int playerIndex;
        ColorImagePoint headColorPoint;
        ColorImagePoint rootColorPoint;
        private DepthImagePoint rootDepthPoint;
        private Texture2D bark;
        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
        }

        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize()
        {
           
            camera = new Camera(this, new Vector3(0, 0, 5), Vector3.Zero, Vector3.Up);
            Components.Add(camera);
            _KinectDevice = KinectSensor.KinectSensors[0];
            _KinectDevice.ColorStream.Enable(ColorImageFormat.RgbResolution1280x960Fps12);
            _KinectDevice.DepthStream.Enable(DepthImageFormat.Resolution640x480Fps30);

            var parameters = new TransformSmoothParameters
            {
                Smoothing = 0.9f,
                Correction = 0.5f,
                Prediction = 0.0f,
                JitterRadius = 1.0f,
                MaxDeviationRadius = 0.5f
            };
            _KinectDevice.SkeletonStream.Enable(parameters);

            _KinectDevice.AllFramesReady += kinect_AllFramesReady;
            byte[] playerImage = new byte[this._KinectDevice.ColorStream.FramePixelDataLength];

            DepthImageStream depthStream = this._KinectDevice.DepthStream;
            this._DepthPixelData = new short[depthStream.FramePixelDataLength];
            this._SkeletonData = new Skeleton[this._KinectDevice.SkeletonStream.FrameSkeletonArrayLength];
            this._ColorPixelData =  new byte[this._KinectDevice.ColorStream.FramePixelDataLength];
            this.greenScreenPixelData = new Array[depthStream.FramePixelDataLength];
            this.colorCoordinates = new ColorImagePoint[depthStream.FramePixelDataLength];
            _KinectDevice.Start();
           colorVideo = new Texture2D(graphics.GraphicsDevice, _KinectDevice.ColorStream.FrameWidth, _KinectDevice.ColorStream.FrameHeight);
            base.Initialize();
        }


   
           void kinect_AllFramesReady(object sender, AllFramesReadyEventArgs allFrame)
        {
            if (null == this._KinectDevice)
            {
                return;
            }

            using (SkeletonFrame skeletonFrame = allFrame.OpenSkeletonFrame())
            {
                if (null != skeletonFrame)
                {
                    skeletonFrame.CopySkeletonDataTo(this._SkeletonData);
                    Skeleton first = (from s in this._SkeletonData where s.TrackingState == SkeletonTrackingState.Tracked select s).FirstOrDefault();
                    if (first == null)
                        return;
                    using (DepthImageFrame depthFrame = allFrame.OpenDepthImageFrame())
                    {
                        if (null != depthFrame)
                        {
                            depthFrame.CopyPixelDataTo(this._DepthPixelData);
                            using (ColorImageFrame colorFrame = allFrame.OpenColorImageFrame())
                            {

                                if (null != colorFrame)
                                {   // Copy the pixel data from the image to a temporary array
                                    colorFrame.CopyPixelDataTo(this._ColorPixelData);

                                    {
                                        int playerImageIndex = 0;
                                        int colorPixelIndex;
                                        ColorImagePoint colorPoint;
                                        int depthPixelIndex;
                                        int colorStride = colorFrame.BytesPerPixel * colorFrame.Width;
                                        // Copy the pixel data from the image to a temporary array

                                        byte[] playerImage = new byte[colorFrame.PixelDataLength];
                                        for (int depthY = 0; depthY < depthFrame.Height; depthY++)
                                        {
                                            for (int depthX = 0; depthX < depthFrame.Width; depthX++, playerImageIndex += 4)
                                            {
                                                depthPixelIndex = depthX + (depthY * depthFrame.Width);
                                                playerIndex = this._DepthPixelData[depthPixelIndex] & DepthImageFrame.PlayerIndexBitmask;

                                                if (playerIndex != 0)
                                                {
                                                    colorPoint = this._KinectDevice.MapDepthToColorImagePoint(depthFrame.Format, depthX, depthY, this._DepthPixelData[depthPixelIndex], colorFrame.Format);
                                                    colorPixelIndex = (colorPoint.X * colorFrame.BytesPerPixel) + (colorPoint.Y * colorStride);
                                                    //red  
                                                    if (this._ColorPixelData[colorPixelIndex + 2] >= 0x80)
                                                        playerImage[playerImageIndex] = (byte)Math.Min(this._ColorPixelData[colorPixelIndex + 2] * 1.25, 0xFF);
                                                    else
                                                        playerImage[playerImageIndex] = 0x80;
                                                    //green
                                                    if (this._ColorPixelData[colorPixelIndex + 1] >= 0x80)
                                                        playerImage[playerImageIndex + 1] = (byte)Math.Min(this._ColorPixelData[colorPixelIndex + 1] * 1.25, 0xFF);
                                                    else
                                                        playerImage[playerImageIndex + 1] = 0x00;
                                                    //blue
                                                    if (this._ColorPixelData[colorPixelIndex] >= 0x83)
                                                        playerImage[playerImageIndex + 2] = (byte)Math.Min(this._ColorPixelData[colorPixelIndex] * 1.25, 0xFF);
                                                    else
                                                        playerImage[playerImageIndex + 2] = 0x00;
                                                    playerImage[playerImageIndex + 3] = 0xFF;                                          //Alpha
                                                }
                                                else
                                                {
                                                    colorPoint = this._KinectDevice.MapDepthToColorImagePoint(depthFrame.Format, depthX, depthY, this._DepthPixelData[depthPixelIndex], colorFrame.Format);
                                                    colorPixelIndex = (colorPoint.X * colorFrame.BytesPerPixel) + (colorPoint.Y * colorStride);
                                                    //red  
                                                    playerImage[playerImageIndex] = 0xaa;
                                                    //green
                                                    playerImage[playerImageIndex + 1] = 0xaa;
                                                    //blue
                                                    playerImage[playerImageIndex + 2] = 0xFF;
                                                    playerImage[playerImageIndex + 3] = 0x80;     // half alpha
                                                }
                                            }
                                        }

                                        colorVideo = new Texture2D(graphics.GraphicsDevice, 1280, 960);
                                        colorVideo.SetData(playerImage);
                                    }

                                }

                                DepthImagePoint headDepthPoint = depthFrame.MapFromSkeletonPoint(first.Joints[JointType.Head].Position);
                                 rootDepthPoint = depthFrame.MapFromSkeletonPoint(first.Position);
                                headColorPoint = depthFrame.MapToColorImagePoint(headDepthPoint.X, headDepthPoint.Y, ColorImageFormat.RgbResolution1280x960Fps12);
                                
                            }
                        }
                    }
                }
            }
        }

           
         

 

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            thisModel = new BasicModel(Content.Load<Model>(@"bark"));
            bark = Content.Load<Texture2D>(@"bark5");
            texture = Content.Load<Texture2D>(@"transparent");

            // TODO: use this.Content to load your game content here
        }

        /// <summary>
        /// UnloadContent will be called once per game and is the place to unload
        /// all content.
        /// </summary>
        protected override void UnloadContent()
        {
            // TODO: Unload any non ContentManager content here
        }

        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {
            // Allows the game to exit
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed)
                this.Exit();

            // TODO: Add your update logic here

            base.Update(gameTime);
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {          
            
            GraphicsDevice.Clear(Color.CornflowerBlue);


            //Texture2D colorVideo2 = new Texture2D(graphics.GraphicsDevice, 640, 480);
            //colorVideo2.SetData(this._ColorPixelData);

            thisModel.Draw(camera, Matrix.CreateWorld(new Vector3(rootDepthPoint.X/4-100, rootDepthPoint.Y/-10-66, -1*(rootDepthPoint.Depth/20)+50), new Vector3(90, 0, 0), Vector3.Up), colorVideo, graphics);

        thisModel.Draw(camera, Matrix.CreateWorld(new Vector3(0,-100, -70), new Vector3(90,0,0), Vector3.Up),texture,graphics);
     

            base.Draw(gameTime);
        }
    }
}
