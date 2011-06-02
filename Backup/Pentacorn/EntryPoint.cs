using System;
using Pentacorn.Tasks;

namespace Pentacorn
{
    static class EntryPoint
    {
        [STAThread]
        private static void Main(string[] args)
        {
            Program.Run("Pentacorn", new EntryTask().Run);
        }
    }
}
 