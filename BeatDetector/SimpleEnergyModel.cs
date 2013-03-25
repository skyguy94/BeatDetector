using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace BeatDetector
{
    public class SimpleEnergyModel
    {
        private const int BufferSize = 1024;
        private const int AudioChannels = 2;

        private readonly Queue<double> _energyBuffer = new Queue<double>(43);

        public void ComputeEnergy(FileInfo rawFile, FileInfo outputFile)
        {
            var totalBytesRead = 0;
            var totalBeats = 0;
            Debug.WriteLine("Attempting to parse raw file: '{0}' with size {1}.", rawFile.Name, rawFile.Length);
            var sw = Stopwatch.StartNew();
            using (var reader = new StreamReader(rawFile.FullName))
            using (var writer = new StreamWriter(outputFile.FullName, false))
            using (var binner = new StreamWriter(Path.ChangeExtension(outputFile.FullName, "bin")))
            {
                const int bufferLength = BufferSize*AudioChannels;
                var tmpBuffer = new char[bufferLength];
                int bytesRead;
                while ((bytesRead = reader.ReadBlock(tmpBuffer, 0, bufferLength)) != 0)
                {
                    Debug.WriteLineIf(bytesRead < bufferLength,
                                      string.Format("Attempted to read {0} bytes and found {1} bytes.", bufferLength,
                                                    bytesRead));

                    var data = new InstantaneousData
                        {
                            Time = totalBytesRead / (44100d * AudioChannels),
                            Value = ComputeInstantEnergy(tmpBuffer),
                        };
                    data.CurrentAverage = ComputeUpdatedAverageEnergyValue(data.Value);
                    totalBeats += data.BeatFound ? 1 : 0;
                    if (data.BeatFound)
                    {
                        binner.WriteLine("{0:F2},{1}", data.Time, data.BeatFound ? 1 : 0);
                    }
                    WriteOutCurrentValuesAsCsv(writer, data);
                    totalBytesRead += bytesRead;
                }
            }

            Debug.WriteLine("File Processed in {0} seconds. {1} beats found in {2} bytes", sw.Elapsed.TotalSeconds,
                            totalBeats, totalBytesRead);
        }

        private static double ComputeInstantEnergy(IList<char> data)
        {
            var instantEnergy = 0d;
            for (var i = 0; i < BufferSize; i += 2)
            {
                var leftAudio = (short) data[i];
                var rightAudio = (short) data[i + 1];

                //Equation (R1)
                var currentValue = Math.Pow(leftAudio, 2) + Math.Pow(rightAudio, 2);
                instantEnergy += currentValue;
            }

            return instantEnergy;
        }

        private double ComputeUpdatedAverageEnergyValue(double instant)
        {
            double average = 0;
            if (_energyBuffer.Count == 43)
            {
                //Equation (R3)
                average = _energyBuffer.Average();
            }

            _energyBuffer.Enqueue(instant);
            if (_energyBuffer.Count > 43)
            {
                _energyBuffer.Dequeue();
            }
            return average;
        }

        public static bool CheckForBeat(double average, double instant)
        {
            if (Math.Abs(instant - 0) < .1 || Math.Abs(average) < .1) return false;

            const double cWeight = 1.2;
            bool isBeat = instant > (cWeight * average);
            Debug.WriteLineIf(isBeat,
                              string.Format("Found beat with instantaneous value of {0} and local average of {1}",
                                            instant, average));
            return isBeat;
        }

        public List<ComplexNumber> DFT(IList<ComplexNumber> samples)
        {
            var results = Enumerable.Repeat(ComplexNumber.Zero, 0).ToList();
            var N = samples.Count;
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

        private static void WriteOutCurrentValuesAsCsv(TextWriter writer, InstantaneousData data)
        {
            writer.WriteLine("{0:F2},{1:F2},{2:F2}, {3}", data.Time, data.Value, data.CurrentAverage, data.BeatFound ? 1 : 0);
        }
    }
}
