using System;

namespace BeatDetector
{
    public class InvalidWAVFileException : Exception
    {
        public InvalidWAVFileException(string message) :
            base(message)
        {}
    }
}
