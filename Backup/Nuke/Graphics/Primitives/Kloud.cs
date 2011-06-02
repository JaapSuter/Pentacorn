using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace Pentacorn.Vision.Graphics.Primitives
{
    class Kloud : PrimitiveBase<VertexPositionColor>
    {
        public Kloud(GraphicsDevice device, ContentManager content, Vector3[] cloud, Picture picture)
        {
            var width = picture.Width;
            var height = picture.Height;

            for (int i = 0; i < cloud.Length; ++i)
            {
                float s = MathHelper.Clamp(cloud[i].Z, 0.0f, 1.0f);
                var b = picture.Bytes;
                AddVertex(new VertexPositionColor(cloud[i], new Color(b[i * 4 + 2], b[i * 4 + 1], b[i * 4 + 0], 255)));
            }

            for (int y = 0; y < (height - 1); ++y)
            {
                for (int x = 0; x < (width - 1); ++x)
                {
                    var v0 = y * width + x;
                    var v1 = v0 + 1;
                    var v2 = v0 + width + 1;
                    var v3 = v0 + width;

                    float min = 0.02f;
                    float max = 0.8f;
                    float discon = 0.02f;

                    if (cloud[v0].Z < min || cloud[v0].Z > max) continue;
                    if (cloud[v1].Z < min || cloud[v1].Z > max) continue;
                    if (cloud[v2].Z < min || cloud[v2].Z > max) continue;
                    if (cloud[v3].Z < min || cloud[v3].Z > max) continue;

                    if (Math.Abs(cloud[v0].Z - Math.Abs(cloud[v1].Z)) > discon) continue;
                    if (Math.Abs(cloud[v0].Z - Math.Abs(cloud[v2].Z)) > discon) continue;
                    if (Math.Abs(cloud[v0].Z - Math.Abs(cloud[v3].Z)) > discon) continue;
                    if (Math.Abs(cloud[v1].Z - Math.Abs(cloud[v2].Z)) > discon) continue;
                    if (Math.Abs(cloud[v1].Z - Math.Abs(cloud[v3].Z)) > discon) continue;
                    if (Math.Abs(cloud[v2].Z - Math.Abs(cloud[v3].Z)) > discon) continue;

                    AddIndex(v0);
                    AddIndex(v1);
                    AddIndex(v2);

                    AddIndex(v0);
                    AddIndex(v2);
                    AddIndex(v3);
                }
            }


            FinishConstruction(device, new BasicEffect(device)
            {
                LightingEnabled = false,
                FogEnabled = false,
                TextureEnabled = false,
                VertexColorEnabled = true,
            });
        }

        public override void Render(object obj, Matrix view, Matrix projection)
        {
            base.Render(obj, view, projection);
        }
    }
}
