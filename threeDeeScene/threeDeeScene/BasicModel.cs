using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;


namespace ThreeDeeScene
{
   public class BasicModel
    {
       protected GraphicsDeviceManager graphicsDeviceManager;
        public Model model { get; protected set; }
        protected Matrix world = Matrix.Identity;
        public BasicModel(Model m)
        {
         
            model = m;
            
        }

        public bool CollidesWith(Model otherModel, Matrix otherWorld)
        {
            // Loop through each ModelMesh in both objects and compare
            // all bounding spheres for collisions
            foreach (ModelMesh myModelMeshes in model.Meshes)
            {
                foreach (ModelMesh hisModelMeshes in otherModel.Meshes)
                {
                    if (myModelMeshes.BoundingSphere.Transform(
                    GetWorld()).Intersects(
                    hisModelMeshes.BoundingSphere.Transform(otherWorld)))
                        return true;
                }
            }
            return false;
        }  

        public virtual void Update(GameTime gameTime)
        {
           
        }

        public void Draw(Camera camera, Matrix worldPlacement, Texture2D texture, GraphicsDeviceManager graphics)
        {
            GraphicsDevice device = graphics.GraphicsDevice;
            Vector3 lightDirection = Vector3.Normalize(new Vector3(3, -1, 1));
            Vector3 lightColor = new Vector3(0.3f, 0.4f, 0.2f);
            Matrix[] transforms = new Matrix[model.Bones.Count];
            model.CopyAbsoluteBoneTransformsTo(transforms);
            foreach (ModelMesh mesh in model.Meshes)
            {
                foreach (BasicEffect be in mesh.Effects)
                {
                    be.EnableDefaultLighting();
                    be.Projection = camera.projection;
                    be.AmbientLightColor = new Vector3(1, 1, 1);
                    be.View = camera.view;
                    be.World = GetWorld()* mesh.ParentBone.Transform*worldPlacement;
                    be.Texture = texture;
                    be.Alpha = 1;
                    be.TextureEnabled = true;
        
                } 
                device.BlendState = BlendState.AlphaBlend;
                device.DepthStencilState = DepthStencilState.Default;
                device.RasterizerState = RasterizerState.CullCounterClockwise;

                mesh.Draw();
                // Second pass renders the alpha blended fringe pixels.
                foreach (Effect effect in mesh.Effects)
                {
                   // effect.Parameters["AlphaTestDirection"].SetValue(-1f);
                }

                device.BlendState = BlendState.NonPremultiplied;
                device.DepthStencilState = DepthStencilState.DepthRead;

                mesh.Draw();
            }
        }
        public virtual Matrix GetWorld()
        {
            return world;
        }
    }
}
