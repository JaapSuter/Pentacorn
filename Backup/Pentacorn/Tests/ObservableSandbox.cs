using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Pentacorn.Tasks
{
    class ObservableSandbox
    {
        public static async Task Run()
        {
            var s = new Subject<TestDisposable>();

            var obs = s.AsObservable().AddRef();

            // obs.Where(d => d.Id % 2 == 0).Subscribe(d => d.Write("Where Even"));
            // obs.Where(d => d.Id % 2 == 1).Subscribe(d => d.Write("Where Odd"));
            // obs.Skip(1).Take(2).Subscribe(d => d.Write("Skip Take"));

            await Program.SwitchToCompute();

            obs.Using().Skip(2).Take(2).ObserveOn(Program.Monitor).Subscribe(d =>
            {
                // using (d)
                {
                    d.Write("Challenging Pre...");
                    // await Program.SwitchToCompute();
                    d.Write("Challenging Middle...");
                    // await Program.SwitchToRender();
                    d.Write("Challenging Post...");
                }
            }, () => Console.WriteLine("Challenging Completed"));

            for (int i = 0; i < 8; ++i)
                using (var d = new TestDisposable())
                    s.OnNext(d);

            s.OnCompleted();

            await TaskEx.Delay(30000);
            if (Global.Yes)
                return;
        }
    }

   

}
