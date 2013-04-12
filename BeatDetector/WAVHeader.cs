using System.Collections.Generic;

namespace BeatDetector
{
    public class WAVHeader
    {
        public short AudioFormat { get; set; }
        public short NumberOfChannels { get; set; }
        public int SampleRate { get; set; }
        public int ByteRate { get; set; }
        public short BlockAlign { get; set; }
        public short BitsPerSample { get; set; }
        public List<WAVSubChunk> Chunks { get; set; }
    }

    public class WAVSubChunk
    {
        public int Id { get; set; }
        public int Size { get; set; }
    }
}
