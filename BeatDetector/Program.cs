using System;
using System.IO;
using System.Linq;

namespace BeatDetector
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var file = new FileInfo(args[0]);
            var ec = new SimpleEnergyCalculator();
            var values = ec.ComputeEnergy(file);

            using (var writer = new StreamWriter(Path.ChangeExtension(file.FullName, ".csv")))
            {
                var writeable = values.Select(v => string.Format("{0:2F},{1:2F},{2:2F},{3}", v.Time, v.Value, v.CurrentAverage, v.BeatFound));
                var output = writeable.Aggregate((a, b) => a + Environment.NewLine + b);
                writer.Write(output);
            }
        }
    }
}
