using System;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Pentacorn.Vision.Graphics;

namespace Pentacorn.Vision
{
    class Flow
    {
        public async Task Execute(Window m, Window p, Renderer r)
        {
            Console.WriteLine(Color.HotPink);
            await TaskEx.Delay(TimeSpan.FromSeconds(2));
            Console.WriteLine(Color.Yellow);
            await TaskEx.Delay(TimeSpan.FromSeconds(3));
            Console.WriteLine(Color.Orange);
            await TaskEx.Delay(TimeSpan.FromSeconds(30));
        }
    }
}
