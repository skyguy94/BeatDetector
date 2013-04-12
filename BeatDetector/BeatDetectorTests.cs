using System.IO;
using System.Linq;
using NUnit.Framework;

namespace BeatDetector
{
    [TestFixture]
    public class BeatDetectorTests
    {
        [Test]
        public void SimpleEnergyModelWithPopcorn()
        {
            var ec = new SimpleEnergyModel();
            var data = ec.ComputeEnergyFromWAVFile(new FileInfo("D:\\popcorn.wav"));

            using (var writer = new StreamWriter("D:\\popcorn.csv"))
            {
                writer.WriteLine("time, e[k], <E>, c");
                foreach (var value in data)
                {
                    writer.WriteLine("{0:F2},{1},{2:F2},{3}", value.Time, value.InstantaneousEnergy, value.AverageEnergy, value.Factor);
                }
            }
        }

        [Test]
        public void SubBandFourierWithPopcorn()
        {
            var ec = new DFTComputer();
            var data = ec.ComputeEnergyFromWAVFile(new FileInfo("D:\\popcorn.wav"));

            var bd = new BeatAnalysis(data, 10);
            using (var writer = new StreamWriter("D:\\popcorn.dft.csv"))
            {
                writer.WriteLine("time, e[k], <E>");
                foreach (var value in bd.Beats)
                {
                    writer.WriteLine("{0:F2},{1:F2}, {2:F2}", value.Time, value.InstantaneousEnergy, value.AverageEnergy);
                }
            }
        }

        [Test]
        public void SimpleEnergyModelWithDrums()
        {
            var ec = new SimpleEnergyModel();
            var data = ec.ComputeEnergyFromWAVFile(new FileInfo("D:\\drums.wav"));

            using (var writer = new StreamWriter("D:\\drums.csv"))
            {
                writer.WriteLine("time, e[k], <E>, c");
                foreach (var value in data)
                {
                    writer.WriteLine("{0:F2},{1},{2:F2},{3}", value.Time, value.InstantaneousEnergy, value.AverageEnergy, value.Factor);
                }
            }
        }

        [Test]
        public void SubBandFourierWithDrums()
        {
            var ec = new DFTComputer();
            var data = ec.ComputeEnergyFromWAVFile(new FileInfo("D:\\drums.wav"));

            var bd = new BeatAnalysis(data, 10);
            using (var writer = new StreamWriter("D:\\drums.dft.csv"))
            {
                writer.WriteLine("time, e[k], <E>");
                foreach (var value in bd.Beats.Distinct())
                {
                    writer.WriteLine("{0:F2},{1:F2}, {2:F2}", value.Time, value.InstantaneousEnergy, value.AverageEnergy);
                }
            }
        }
    }
}
