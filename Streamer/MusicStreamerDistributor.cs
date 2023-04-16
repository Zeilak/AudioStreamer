using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;

namespace Streamer
{
    /// <summary>
    /// distributes data across streams
    /// </summary>
    public class MusicStreamerDistributor
    {
        List<MusictStreamerReader> _readers = new List<MusictStreamerReader>();
        public MusictStreamerReader Attach(Stream outStream, IStreamer streamer, CancellationToken cancellationToken)
        {
            var reader = new MusictStreamerReader(outStream, cancellationToken);
            var audioHeader = streamer.GetAudioFileHeader();
            reader.SetChunk(audioHeader);
            lock (_readers)
            {
                _readers.Add(reader);
            }
            return reader;
        }

        private List<MusictStreamerReader> _readersToRemove = new List<MusictStreamerReader>();
        public bool SetChunkToReaders(byte[] chunk)
        {
            lock (_readers)
            {
                foreach (var reader in _readers)
                {
                    if (!reader.SetChunk(chunk))
                    {
                        _readersToRemove.Add(reader);
                    }
                }
                if (_readersToRemove.Count > 0)
                {
                    foreach (var reader in _readersToRemove)
                    {
                        _readers.Remove(reader);
                    }
                    _readersToRemove = new List<MusictStreamerReader>();
                }

            }
            return false;
        }
    }
}
