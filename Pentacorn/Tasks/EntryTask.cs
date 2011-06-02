using System.Threading.Tasks;
using System;
using System.Linq;
using System.Collections.Generic;
using Emgu.CV.Structure;
using Emgu.CV;
using Emgu.CV.CvEnum;
using Microsoft.Xna.Framework;

namespace Pentacorn.Tasks
{
    class EntryTask
    {
        public async Task Run()
        {
            await new ProgramTask().Run();
        }
    }
}
