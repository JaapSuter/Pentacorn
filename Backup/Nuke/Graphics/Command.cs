using System;

namespace Pentacorn.Vision.Graphics
{
    class Command
    {
        public Command(Type renderable, object args)
        {
            this.Renderable = renderable;
            this.Args = args;
        }

        public Type Renderable { get; private set; }
        public object Args { get; private set; }
    }
}