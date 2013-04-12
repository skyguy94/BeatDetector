using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace BeatDetector
{
    public class BeatAnalysis
    {
        private readonly IEnumerable<ComputedData> _data;
        private readonly double _factor;

        public BeatAnalysis(IEnumerable<ComputedData> data, double factor)
        {
            _data = data;
            _factor = factor;
        }

        public BeatAnalysis(IEnumerable<ComputedData> data)
            : this(data, 1.8)
        { }

        public IEnumerable<ComputedData> Beats
        {
            get
            {

                var beats = _data.Where(CheckForBeat);
                Debug.WriteLine("{0} beats found", beats.Count());
                return beats;
            }
        }

        private bool CheckForBeat(ComputedData data)
        {
            if (Math.Abs(data.InstantaneousEnergy) < .1 || Math.Abs(data.AverageEnergy) < .1) return false;

            var isBeat = data.InstantaneousEnergy > (_factor * data.AverageEnergy);
            return isBeat;
        }
    }
}
