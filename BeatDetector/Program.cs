using System.IO;

namespace BeatDetector
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var ec = new SimpleEnergyCalculator();
            ec.ComputeEnergy(new FileInfo("D:\\sample.raw"), new FileInfo("D:\\sample.csv"));
        }
    }
}
