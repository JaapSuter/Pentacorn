using Microsoft.Xna.Framework;

namespace Pentacorn.Graphics
{
    class Text : IVisible
    {
        public Text(string s, Vector3 position, Color fillColor)
        {
            String = s;
            Position = position;
            FillColor = fillColor;
            AlreadyInScreenSpace = false;
        }

        public Text(string s, Vector2 position, Color fillColor)
        {
            String = s;
            Position = new Vector3(position, 0);
            FillColor = fillColor;
            AlreadyInScreenSpace = true;
        }

        public void Render(Renderer renderer, IViewProject viewProject)
        {
            throw new System.NotImplementedException();
        }

        public readonly bool AlreadyInScreenSpace;
        public Color FillColor;
        public Vector3 Position;
        public string String;
    }
}
