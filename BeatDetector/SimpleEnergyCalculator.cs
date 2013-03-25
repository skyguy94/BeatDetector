using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace BeatDetector
{
    public class SimpleEnergyCalculator
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
                            Time = totalBytesRead / bufferLength,
                            Value = ComputeInstantEnergy(tmpBuffer),
                        };
                    data.CurrentAverage = ComputeUpdatedAverageEnergyValue(data.Value);
                    totalBeats += data.BeatFound ? 1 : 0;
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

            const double cWeight = 1.8;
            bool isBeat = instant > (cWeight * average);
            Debug.WriteLineIf(isBeat,
                              string.Format("Found beat with instantaneous value of {0} and local average of {1}",
                                            instant, average));
            return isBeat;
        }

        private static void WriteOutCurrentValuesAsCsv(TextWriter writer, InstantaneousData data)
        {
            writer.WriteLine("{0},{1},{2}, {3}", data.Time, data.Value, data.CurrentAverage, data.BeatFound);
        }
    }

    public class InstantaneousData
    {
        public double Time { get; set; }
        public double Value { get; set; }
        public double CurrentAverage { get; set; }

        public bool BeatFound
        {
            get
            {
                var beatFound = SimpleEnergyCalculator.CheckForBeat(CurrentAverage, Value);
                return beatFound;
            }
        }
    }
}
