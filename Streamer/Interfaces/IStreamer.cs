using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;

namespace Streamer
{
    public interface IStreamer
    {
        void Start(Func<byte[], bool> setChunkFunction);
        byte[] GetAudioFileHeader();
        void Stop();
    }
}
