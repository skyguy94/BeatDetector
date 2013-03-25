using System.IO;
using NUnit.Framework;

namespace BeatDetector
{
    [TestFixture]
    public class SoundernatorTests
    {
        [Test]
        public void TestWithFullSample()
        {
            var ec = new SimpleEnergyModel();
            ec.ComputeEnergy(new FileInfo("D:\\sample.raw"), new FileInfo("D:\\sample.csv"));
        }
    }
}
