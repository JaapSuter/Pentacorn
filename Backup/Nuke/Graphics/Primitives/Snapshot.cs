using System.Collections.Concurrent;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Pentacorn.Vision.Graphics.Primitives
{
    class Snapshot : IRenderable
    {
        public class Context
        {
            public Picture Picture;
            public Rectangle Rect;
        }

        public Snapshot(GraphicsDevice device)
        {
            spriteBatch = new SpriteBatch(device);
        }

        public void Render(object obj, Matrix view, Matrix projection)
        {
            var context = (Context)obj;
            var pic = context.Picture;
            
            var len = pic.Bytes.Length;
            var tex = texture2Ds.GetOrAdd(len, (_) => new Texture2D(spriteBatch.GraphicsDevice, pic.Width, pic.Height, false, SurfaceFormat.Color));
            var channels = 4;
            tex.GraphicsDevice.Textures[0] = null;
            tex.SetData(pic.Bytes, 0, pic.Width * pic.Height * channels);

            spriteBatch.Begin(SpriteSortMode.Texture, BlendState.Opaque);
            spriteBatch.Draw(tex, context.Rect, Color.White);
            spriteBatch.End();
        }

        private ConcurrentDictionary<int, Texture2D> texture2Ds = new ConcurrentDictionary<int, Texture2D>();
        private SpriteBatch spriteBatch;
    }
}
