using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace BeatDetector
{
    public class DFTComputer
    {
        private const int BufferSize = 1024;
        private const int AudioChannels = 1;

        private readonly Queue<double> _energyBuffer = new Queue<double>(43);

        public void ComputeEnergy(FileInfo rawFile, FileInfo outputFile)
        {
            Debug.WriteLine("Attempting to parse raw file: '{0}' with size {1}.", rawFile.Name, rawFile.Length);
            var sw = Stopwatch.StartNew();
            using (var reader = new StreamReader(rawFile.FullName))
            using (var writer = new StreamWriter(outputFile.FullName, false))
            {
                const int bufferLength = BufferSize * AudioChannels;
                var tmpBuffer = new char[bufferLength];
                int bytesRead = 0;
                while ((bytesRead = reader.ReadBlock(tmpBuffer, 0, bufferLength)) != 0)
                {
                    var complex = tmpBuffer.Select(s => new ComplexNumber(s, 0)).ToList();
                    var results = DFT(complex);
                    results.ForEach(c => writer.WriteLine("{0:F2}+i{1:F2}", c.Real, c.Imaginary));
                }
            }
        }

        public List<ComplexNumber> DFT(IList<ComplexNumber> samples)
        {
            var N = samples.Count;
            var results = Enumerable.Repeat(ComplexNumber.Zero, N).ToList();
            for (var k = 0; k < N; k++)
            {
                for (var n = 0; n < N; n++)
                {
                    var tmp = CreateComplexNumberFromPolar(1, -2 * Math.PI * n * k / N);
                    results[k].Add(tmp.Multiply(samples[n]));
                }
            }

            return results;
        }

        private static ComplexNumber CreateComplexNumberFromPolar(int r, double theta)
        {
            var result = new ComplexNumber(r * Math.Cos(theta), r * Math.Sin(theta));
            return result;
        }
    }
}
