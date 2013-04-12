using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Numerics;
using MathNet.Numerics.IntegralTransforms;

namespace BeatDetector
{
    public class DFTComputer
    {
        private readonly double _instantanteousPeriod;
        private int _samplesInInstantPeriod;

        public DFTComputer(double instantaneousPeriod)
        {
            if (instantaneousPeriod <= 0) throw new ArgumentException("The instantaneous period must be greater than zero.");
            _instantanteousPeriod = instantaneousPeriod;
        }

        public DFTComputer()
            : this(1/43d)
        {}

        private readonly double[] squared = new double[1024];
        private readonly List<Queue<double>> average = new List<Queue<double>>(32);

        public IList<ComputedData> ComputeEnergyFromWAVFile(FileInfo rawFile)
        {
            average.AddRange(Enumerable.Repeat(new Queue<double>(43), 32));
            var data = new List<ComputedData>();
            using (var reader = new WAVFile(rawFile))
            {
                var header = reader.ReadHeader();

                _samplesInInstantPeriod = (int) Math.Floor((header.SampleRate*_instantanteousPeriod)*header.NumberOfChannels);
                var tmpBuffer = new char[1024];
                int bytesRead, totalBytesRead = 0;
                //Read the first chunk. IDK what to do with disconnected chunks.
                using (var chunkStream = reader.SeekChunk(0))
                while ((bytesRead = chunkStream.ReadBlock(tmpBuffer, 0, tmpBuffer.Length)) != 0)
                { 
                    var complex = tmpBuffer.Select(s => new Complex((short)s, 0)).ToArray();
                    Transform.FourierForward(complex);

                    for (int i = 0; i < 1024; i++)
                    {
                        squared[i] = Math.Pow(complex[i].Magnitude, 2);
                    }

                    var time = totalBytesRead/((double) 1024*43*2);
                    totalBytesRead += bytesRead;

                    for (int i = 0; i < 32; i++)
                    {
                        var cd = new ComputedData
                            {
                                Time = time,
                                InstantaneousEnergy = squared.Skip(i * 32).Take(32).Aggregate((c, n) => c + n)
                            };
                        cd.AverageEnergy = ComputeUpdatedAverageEnergy(average[i], cd.InstantaneousEnergy);
                        data.Add(cd);
                    }
                }
            }
            return data;
        }

        private static double ComputeUpdatedAverageEnergy(Queue<double> values, double instant)
        {
            double averageEnergy = 0;
            if (values.Count == 43)
            {
                //Equation (R3)
                averageEnergy = values.Average();
            }

            values.Enqueue(instant);
            if (values.Count > 43)
            {
                values.Dequeue();
            }
            return averageEnergy;
        }

        public List<ComplexNumber> DFT(IList<ComplexNumber> samples)
        {
            var sw = Stopwatch.StartNew();
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
            Debug.WriteLine("Processed DFT chunk in {0} ms.", sw.ElapsedMilliseconds);
            return results;
        }

        private static ComplexNumber CreateComplexNumberFromPolar(int r, double theta)
        {
            var result = new ComplexNumber(r * Math.Cos(theta), r * Math.Sin(theta));
            return result;
        }
    }
}