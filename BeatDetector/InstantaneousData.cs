namespace BeatDetector
{
    public class InstantaneousData
    {
        public double Time { get; set; }
        public double Value { get; set; }
        public double CurrentAverage { get; set; }

        public bool BeatFound
        {
            get
            {
                var beatFound = SimpleEnergyModel.CheckForBeat(CurrentAverage, Value);
                return beatFound;
            }
        }
    }
}