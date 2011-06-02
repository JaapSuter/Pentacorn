using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Pentacorn;

namespace Pentacorn.Graphics
{
    class GrayCodeSweep : IVisible
    {
        public enum Direction
        {
            Horizontal,
            Vertical
        };

        public GrayCodeSweep(SafeGrayCode sgc, int sweepLength, Direction dir)
        {
            Sgc = sgc;
            SweepLength = sweepLength;
            Dir = dir;
        }

        public SafeGrayCode Sgc;
        public int Bit;
        public Direction Dir;
        public int SweepLength;
        public Color BlackColor = Color.Black;
        public Color WhiteColor = Color.White;

        public void Invert()
        {
            Util.Swap(ref BlackColor, ref WhiteColor);
        }

        public void Render(Renderer renderer, IViewProject viewProject)
        {
            throw new System.NotImplementedException();
        }
    };
}
