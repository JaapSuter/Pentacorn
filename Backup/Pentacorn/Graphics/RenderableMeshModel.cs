using System;
using System.Linq;
using Size = System.Drawing.Size;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Pentacorn;
using System.Collections.Generic;

namespace Pentacorn.Graphics
{
    class RenderableMeshModel : Renderable<MeshModel>
    {
        public RenderableMeshModel(RendererImpl rendererImpl)
        {
            BasicEffect = new BasicEffect(rendererImpl.Device)
            {
                LightingEnabled = true,
                FogEnabled = false,
                TextureEnabled = false,
                VertexColorEnabled = false,
                PreferPerPixelLighting = true,
            };

            VirtualProjectorEffect = rendererImpl.Loader.Load<Effect>("Content/Shaders/VirtualProjector");
            Texture2D = rendererImpl.HorizontalStripes;
        }

        public override void Render(RendererImpl rendererImpl, IViewProject viewProject, MeshModel t)
        {
            BasicEffect.World = t.World;
            BasicEffect.View = viewProject.View;
            BasicEffect.Projection = viewProject.Projection;
            BasicEffect.DiffuseColor = Color.White.ToVector3();
            BasicEffect.AmbientLightColor = Color.White.Alpha(0.1f).ToVector3();
            BasicEffect.DirectionalLight0.DiffuseColor = Color.Orange.ToVector3();
            BasicEffect.DirectionalLight1.DiffuseColor = Color.CornflowerBlue.ToVector3();
            BasicEffect.DirectionalLight2.DiffuseColor = Color.HotPink.ToVector3();

            VirtualProjectorEffect.Parameters["WorldViewProj"].SetValue(t.World * viewProject.View * viewProject.Projection);
            VirtualProjectorEffect.Parameters["WorldProjectorViewProj"].SetValue(t.World * t.ProjectorViewProject.View * t.ProjectorViewProject.Projection);
            VirtualProjectorEffect.Parameters["Color"].SetValue(Color.White.ToVector4());
            VirtualProjectorEffect.Parameters["Texture"].SetValue(Texture2D);
            
            rendererImpl.Device.BlendState = BlendState.AlphaBlend;

            foreach (var mesh in t.Model.Meshes)
            {
                foreach (var meshPart in mesh.MeshParts)
                    meshPart.Effect = BasicEffect;
                mesh.Draw();

                foreach (var meshPart in mesh.MeshParts)
                    meshPart.Effect = VirtualProjectorEffect;                    
                mesh.Draw();
            }
        }
        
        private BasicEffect BasicEffect;
        private Effect VirtualProjectorEffect;
        private Texture2D Texture2D;
    }
}
