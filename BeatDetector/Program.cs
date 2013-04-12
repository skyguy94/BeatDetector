using System.IO;
using System.Linq;

namespace BeatDetector
{


    public class Program
    {
        public static void Main(string[] args)
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
