using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace BeatDetector
{
    public class SimpleEnergyModel
    {
        private readonly Queue<double> _energyBuffer = new Queue<double>();
        private readonly Queue<double> _varianceBuffer = new Queue<double>();
        private readonly double _instantanteousPeriod;
        private readonly double _averagePeriod;
        private int _samplesInInstantPeriod;
        private int _samplesInAveragePeriod;

        public SimpleEnergyModel(double instantaneousPeriod, double averagePeriod)
        {
            if (instantaneousPeriod <= 0) throw new ArgumentException("The instantaneous period must be greater than zero.");
            if (averagePeriod <= 0) throw new ArgumentException("The average period must be greater than zero.");

            _instantanteousPeriod = instantaneousPeriod;
            _averagePeriod = averagePeriod;
        }

        public SimpleEnergyModel()
            : this(1/43d, 1d)
        {}

        public IList<ComputedData> ComputeEnergyFromWAVFile(FileInfo rawFile)
        {
            var data = new List<ComputedData>();
            using (var reader = new WAVFile(rawFile))
            {
                var header = reader.ReadHeader();

                _samplesInInstantPeriod = (int)Math.Floor((header.SampleRate*_instantanteousPeriod)*header.NumberOfChannels);
                _samplesInAveragePeriod = (int)Math.Floor((header.SampleRate*_averagePeriod)/_samplesInInstantPeriod);
                var tmpBuffer = new char[_samplesInInstantPeriod];
                int bytesRead, totalBytesRead = 0;

                //Read the first chunk. IDK what to do with disconnected chunks.
                using (var chunkStream = reader.SeekChunk(0))
                while ((bytesRead = chunkStream.ReadBlock(tmpBuffer, 0, tmpBuffer.Length)) != 0)
                {
                    var id = new ComputedData
                        {
                            Time = (totalBytesRead/(tmpBuffer.Length)/_samplesInAveragePeriod),
                            InstantaneousEnergy = ComputeInstantEnergy(tmpBuffer.Select(s => (short)s)),
                        };

                    id.AverageEnergy = ComputeUpdatedAverageEnergy(id.InstantaneousEnergy);
                    id.Factor = ComputeUpdatedVarianceFactor(id.InstantaneousEnergy, id.AverageEnergy);
                    data.Add(id);

                    totalBytesRead += bytesRead;
                }
            }

            return data;
        }

        private static double ComputeInstantEnergy(IEnumerable<short> data)
        {
            //Equation (R1)
            double instantEnergy = data.Sum(s => Math.Pow(s, 2));
            return instantEnergy;
        }

        private double ComputeUpdatedAverageEnergy(double instant)
        {
            double averageEnergy = 0;
            if (_energyBuffer.Count == _samplesInAveragePeriod)
            {
                //Equation (R3)
                averageEnergy = _energyBuffer.Average();
            }

            _energyBuffer.Enqueue(instant);
            if (_energyBuffer.Count > _samplesInAveragePeriod)
            {
                _energyBuffer.Dequeue();
            }
            return averageEnergy;
        }

        private double ComputeUpdatedVarianceFactor(double instant, double average)
        {
            double factor = 0;
            if (_varianceBuffer.Count == _samplesInAveragePeriod)
            {
                //Equation (R6)
                var averageVariance = _varianceBuffer.Average();
                factor = (-0.0025714 * averageVariance) + 1.5142857;
                factor = (Math.Abs(factor) > 0.1) ? factor : 0;
            }

            var currentVariance = Math.Pow(instant - average, 2);
            _varianceBuffer.Enqueue(currentVariance);
            if (_varianceBuffer.Count > _samplesInAveragePeriod)
            {
                _varianceBuffer.Dequeue();
            }

            return factor;
        }
    }
}
