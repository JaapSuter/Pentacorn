using System;
using System.Linq;
using Size = System.Drawing.Size;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Pentacorn;

namespace Pentacorn.Graphics
{
    class RenderableCloud : RenderablePrimitiveBase<VertexPositionNormalTexture, Cloud>
    {
        public override void Render(RendererImpl rendererImpl, IViewProject viewProject, Cloud cloud)
        {
            if (!Initialized)
                Initialize(rendererImpl, cloud);

            BasicEffect.World = Matrix.Identity;
            BasicEffect.View = viewProject.View;            
            BasicEffect.Projection = viewProject.Projection;
            BasicEffect.DiffuseColor = cloud.Color.ToVector3();
            BasicEffect.Alpha = cloud.Color.A / 255.0f;

            rendererImpl.Device.DepthStencilState = new DepthStencilState() { DepthBufferFunction = CompareFunction.LessEqual }; //DepthStencilState.Default;
            
            base.Render(rendererImpl, BasicEffect);
        }

        public void Initialize(RendererImpl rendererImpl, Cloud cloud)
        {
            Initialized = true;

            var device = rendererImpl.Device;
            var width = cloud.Size.Width;
            var height = cloud.Size.Height;

            var minimax = cloud.Points.Aggregate(Tuple.Create(float.MaxValue, float.MinValue),
                    (a, pt) => Tuple.Create((float)Math.Min(pt.Position.Z, a.Item1), (float)Math.Max(pt.Position.Z, a.Item2)));

            for (int i = 0; i < cloud.Points.Length; ++i)
                AddVertex(cloud.Points[i]);

            for (int y = 0; y < (height - 1); ++y)
            {
                for (int x = 0; x < (width - 1); ++x)
                {
                    var v0 = y * width + x;
                    var v1 = v0 + 1;
                    var v2 = v0 + width;
                    var v3 = v0 + width + 1;

                    var discon = Math.Max(width / 100, 0.1f * minimax.Item2 - minimax.Item1); 

                    if (cloud.Points[v0].Position == Vector3.Zero) continue;
                    if (cloud.Points[v1].Position == Vector3.Zero) continue;
                    if (cloud.Points[v2].Position == Vector3.Zero) continue;
                    if (cloud.Points[v3].Position == Vector3.Zero) continue;

                    if (Math.Abs(cloud.Points[v0].Position.Z - cloud.Points[v1].Position.Z) > discon) continue;
                    if (Math.Abs(cloud.Points[v0].Position.Z - cloud.Points[v2].Position.Z) > discon) continue;
                    if (Math.Abs(cloud.Points[v0].Position.Z - cloud.Points[v3].Position.Z) > discon) continue;
                    if (Math.Abs(cloud.Points[v1].Position.Z - cloud.Points[v2].Position.Z) > discon) continue;
                    if (Math.Abs(cloud.Points[v1].Position.Z - cloud.Points[v3].Position.Z) > discon) continue;
                    if (Math.Abs(cloud.Points[v2].Position.Z - cloud.Points[v3].Position.Z) > discon) continue;

                    AddIndex(v0);
                    AddIndex(v1);
                    AddIndex(v3);

                    AddIndex(v0);
                    AddIndex(v3);
                    AddIndex(v2);
                }
            }

            BasicEffect = new BasicEffect(device)
            {
                LightingEnabled = true,
                FogEnabled = false,
                TextureEnabled = true,
                VertexColorEnabled = false,
                PreferPerPixelLighting = true,
                Texture = cloud.Texture2D,
            };

            BasicEffect.EnableDefaultLighting();

            FinishConstruction(device);
        }

        private bool Initialized;
        private BasicEffect BasicEffect;
    }
}
