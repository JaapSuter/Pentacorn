using System.Collections.Generic;

namespace Pentacorn.Vision.Graphics
{
    class Frame
    {
        public void Add<Type>(object obj)
        {
            commands.Add(new Command(typeof(Type), obj));
        }

        public IEnumerable<Command> Commands { get { return commands; } }        
        private IList<Command> commands = new List<Command>();        
    }
}
