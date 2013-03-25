using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace BeatDetector
{
    public class SimpleEnergyCalculator
    {
        private const int BufferSize = 1024;
        private const int AudioChannels = 1;

        private readonly Queue<double> _energyBuffer = new Queue<double>(43);
        public IList<InstantaneousData> ComputeEnergy(FileInfo rawFile)
        {
            var data = new List<InstantaneousData>();
            var totalBytesRead = 0d;
            using (var reader = new StreamReader(rawFile.FullName))
            {
                const int bufferLength = BufferSize*AudioChannels;
                var tmpBuffer = new char[bufferLength];
                int bytesRead;
                while ((bytesRead = reader.ReadBlock(tmpBuffer, 0, bufferLength)) != 0)
                {
                    var id = new InstantaneousData
                        {
                            Time = (totalBytesRead / bufferLength) / 43d,
                            Value = ComputeInstantEnergy(tmpBuffer),
                        };

                    id.CurrentAverage = ComputeUpdatedAverageEnergyValue(id.Value);
                    id.BeatFound = CheckForBeat(id.CurrentAverage, id.Value);
                    data.Add(id);

                    totalBytesRead += bytesRead;
                }
            }

            return data;
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
            var isBeat = instant > (cWeight * average);
            return isBeat;
        }
    }
}
