using System.Threading.Tasks;
using System;
using System.Linq;
using System.Collections.Generic;
using Emgu.CV.Structure;
using Emgu.CV;
using Microsoft.Xna.Framework;
using Emgu.CV.CvEnum;

namespace Pentacorn.Tasks
{
    class PhaseShiftTask
    {

        private static IEnumerable<byte> FringePattern(int totalLength, double waveLength, double phaseShift)
        {
            const double twoPi = Math.PI * 2;
            for (int i = 0; i < totalLength; ++i)
                yield return (byte)(255 * (0.5 + 0.5 * Math.Cos(twoPi * (i / waveLength) + phaseShift)));
        }

        private static IEnumerable<double> GetPhase(double waveLength, IList<byte> minPhase, IList<byte> zeroPhase, IList<byte> plusPhase)
        {
            var sqrt3 =-Math.Sqrt(3);
            var twoPi = Math.PI * 2;

            for (int i = 0; i < minPhase.Count; ++i)
            {
                double p0 = minPhase[i];
                double p1 = zeroPhase[i];
                double p2 = plusPhase[i];

                var phase = -Math.Atan2(sqrt3 * (p0 - p2), 2 * p1 - p0 - p2);

                var positive = (phase < 0) ? (twoPi + phase) : phase;

                yield return positive / twoPi * waveLength;
            };
        }

        public async Task Run()
        {
            var sqrt3 =-Math.Sqrt(3);
            var twoPi = Math.PI * 2;

            var len = 1280;
            var per = 40;
            var shift = twoPi / 3.0;
            
            var img0 = new Picture<Gray, byte>(len, 1, FringePattern(len, per, -shift));
            var img1 = new Picture<Gray, byte>(len, 1, FringePattern(len, per, 0));
            var img2 = new Picture<Gray, byte>(len, 1, FringePattern(len, per, shift));

            SaveWider(len, img0, "0");
            SaveWider(len, img1, "1");
            SaveWider(len, img2, "2");

            var phases = GetPhase(per, img0.Bytes, img1.Bytes, img2.Bytes);

            Console.WriteLine(phases.Min());
            Console.WriteLine(phases.Max());

            var unwrapped = phases.Select((i, phase) => Math.Floor(i / per) * per + phase);

            var img_ = new Picture<Gray, byte>(len, 1, unwrapped.Select(b => (byte)(b * 255.0 / len)));

            SaveWider(len, img_, "unwrapped");

            await new ProgramTask().Run();
        }

        private static void SaveWider(int len, Picture<Gray, byte> img, string name)
        {
            img.Emgu.Resize(len, 100, INTER.CV_INTER_NN, false).Save(Global.TmpFileName(name, "png"));
        }


    }
}
