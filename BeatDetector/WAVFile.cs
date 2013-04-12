using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace BeatDetector
{
    public class WAVFile : IDisposable
    {
        private readonly FileInfo _fi;

        private readonly List<WeakReference<StreamReader>> _readers = new List<WeakReference<StreamReader>>();

        public WAVFile(FileInfo file)
        {
            if (file == null) throw new ArgumentNullException("file");

            _fi = file;
        }

        public WAVHeader ReadHeader()
        {
            var header = new WAVHeader();

            using (var reader = new BinaryReader(File.OpenRead(_fi.FullName), Encoding.ASCII))
            {
                var riffMarker = new string(reader.ReadChars(4));
                if (String.CompareOrdinal(riffMarker, "RIFF") != 0) throw new InvalidWAVFileException("RIFF header truncated.");

                var chunkSize = reader.ReadInt32();
                if (chunkSize > _fi.Length) throw new InvalidWAVFileException("RIFF header truncated.");

                var waveMarker = new string(reader.ReadChars(4));
                if (String.CompareOrdinal(waveMarker, "WAVE") != 0) throw new InvalidWAVFileException("WAVE format identifier not found.");

                var fmtMarker = new string(reader.ReadChars(4));
                if (String.CompareOrdinal(fmtMarker, "fmt ") != 0) throw new InvalidWAVFileException("WAV header corrupted.");

                var bytesRemaining = reader.ReadInt32();
                if (bytesRemaining != 16) throw new InvalidWAVFileException("Cannot read non-PCM WAV files."); //LazyDeveloperException();

                header.AudioFormat = reader.ReadInt16();
                header.NumberOfChannels = reader.ReadInt16();
                header.SampleRate = reader.ReadInt32();
                header.ByteRate = reader.ReadInt32();
                header.BlockAlign = reader.ReadInt16();
                header.BitsPerSample = reader.ReadInt16();

                //TODO: Handle extra stuff. LazyDeveloperException();
                
                var chunk = new WAVSubChunk
                    {
                        Id = reader.ReadInt32(),
                        Size = reader.ReadInt32()
                    };

                header.Chunks = new List<WAVSubChunk> { chunk };
            }

            return header;
        }

        public StreamReader SeekChunk(int chunkId)
        {
            //Only handle the first chunkz for the moment.
            var reader = new StreamReader(File.OpenRead(Path.ChangeExtension(_fi.FullName, "raw")));

            _readers.Add(new WeakReference<StreamReader>(reader));
            return reader;
        }

        public void Dispose()
        {
            _readers.ForEach(CloseReader);
            _readers.Clear();
        }

        private static void CloseReader(WeakReference<StreamReader> weak)
        {
            StreamReader reader;
            if (!weak.TryGetTarget(out reader)) return;

            reader.Dispose();
            weak.SetTarget(null);
        }
    }
}
