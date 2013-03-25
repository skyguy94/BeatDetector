using System;
using System.Diagnostics;
using System.IO;

namespace BeatDetector
{
    public class WAVtoRAW
    {
        public void StripHeaderAndWriteRAW(FileInfo file)
        {
            using (var reader = new StreamReader(file.FullName))
            {
                var dataLength = ProcessHeader(reader);

                var bytesRemaining = dataLength;
                var chunkId = 1;
                while (bytesRemaining != 0)
                {
                    using (var writer = new StreamWriter(Path.ChangeExtension(file.FullName, "chunk" + chunkId)))
                    {
                        bytesRemaining = ProcessAndWriteChunk(reader, writer, dataLength);
                    }

                    chunkId++;
                }
            }
        }

        private static int ProcessHeader(TextReader reader)
        {
            const int headerSize = 6;
            var headerBuffer = new char[headerSize];
            var bytesRead = reader.ReadBlock(headerBuffer, 0, headerSize);
            if (bytesRead != headerSize) throw new InvalidWAVFileException("RIFF header truncated.");

            var header = new string(headerBuffer);
            var riffHeader = header.Substring(0, 4);
            var dataLength = Convert.ToInt32(header.Substring(4, 4));
            var waveHeader = header.Substring(8, 4);

            if (!String.Equals(riffHeader, "RIFF")) throw new InvalidWAVFileException("RIFF token not found.");
            if (!String.Equals(waveHeader, "WAVE")) throw new InvalidWAVFileException("WAVE format identifier not found.");

            Debug.WriteLine("Found WAV data with length of {0} bytes .", dataLength);

            return dataLength;
        }

        private static int ProcessAndWriteChunk(TextReader reader, TextWriter writer, int dataLength)
        {
            const int headerSize = 4;
            var headerbuffer = new char[headerSize];
            var bytesRead = reader.ReadBlock(headerbuffer, 0, headerSize);
            if (bytesRead != headerSize) throw new InvalidWAVFileException("Chunk header truncated.");

            const int bufferSize = 4096;
            var dataBuffer = new char[bufferSize];
            var totalBytesRead = 0;
            while ((bytesRead = reader.ReadBlock(dataBuffer, 0, bufferSize)) != 0)
            {
                writer.Write(dataBuffer);
                totalBytesRead += bytesRead;
            }

            var bytesRemaining = dataLength - totalBytesRead;
            return bytesRemaining;
        }
    }
}
