using System;
using Pentacorn.Tasks;
using System.Threading.Tasks;

namespace Pentacorn
{
    static class EntryPoint
    {
        [STAThread]
        private static int Main(string[] args)
        {
            Winterop.MoveBroadCastEventWindowToThisThread();

            return Program.Run("Pentacorn", async () => { await TaskEx.Delay(10); } );
        }
    }
}
