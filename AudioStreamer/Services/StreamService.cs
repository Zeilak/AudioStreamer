using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using NAudio.Wave;
using Streamer;

namespace AudioStreamer.Services
{
    public class StreamService
    {
        IStreamer _streamer;
        MusicStreamerDistributor _distributor;
        object _locker = new object();
        public StreamService()
        {
            _distributor = new MusicStreamerDistributor();
        }

        public void UpdateStreamer(IStreamer streamer)
        {
            lock(_locker)
            {
                if(_streamer != null)
                {
                    _streamer.Stop();
                }
                _streamer = streamer;
                _streamer.Start(_distributor.SetChunkToReaders);
            }
        }

        public void SetDefaultStreamerIfNull()
        {
            lock (_locker)
            {
                if (_streamer == null)
                {
                    _streamer = new WavStreamer(new WaveFormat(44100, 32, 2));
                    _streamer.Start(_distributor.SetChunkToReaders);
                }
            }
            
        }

        public async Task ServeClientAndStartStream(Stream outStreaem, CancellationToken cancellationToken)
        {
            SetDefaultStreamerIfNull();

            var reader = _distributor.Attach(outStreaem, _streamer, cancellationToken);
            await reader.Start();
        }
    }
}
