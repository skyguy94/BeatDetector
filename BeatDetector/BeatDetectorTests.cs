using System.IO;
using NUnit.Framework;

namespace BeatDetector
{
    [TestFixture]
    public class BeatDetectorTests
    {
        [Test]
        public void TestWithFullSample()
        {
            var ec = new SimpleEnergyCalculator();
            var energy = ec.ComputeEnergy(new FileInfo("D:\\sample.raw"));
        }
    }
}
