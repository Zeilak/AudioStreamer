using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using Nito.AsyncEx;

namespace Streamer
{
    /// <summary>
    /// redirects data from distributor to outStream
    /// </summary>
    public class MusictStreamerReader
    {
        Stream _outStream;
        CancellationToken _cancellationToken;
        AsyncManualResetEvent _manualResetEvent = new AsyncManualResetEvent(false);
        ConcurrentQueue<byte[]> _buffers = new ConcurrentQueue<byte[]>();
        public MusictStreamerReader(Stream stream, CancellationToken cancellationToken)
        {
            _outStream = stream;
            _cancellationToken = cancellationToken;
        }

        public bool SetChunk(byte[] chunk)
        {
            if (_cancellationToken.IsCancellationRequested) return false;

            _buffers.Enqueue(chunk);
            _manualResetEvent.Set();
            return true;
        }

        public async Task Start()
        {
            while (true)
            {
                if (_buffers.IsEmpty == false)
                {
                    if (_buffers.TryDequeue(out var buffer))
                    {
                        await _outStream.WriteAsync(buffer, 0, buffer.Length);
                        await _outStream.FlushAsync();
                    }
                }
                else
                {
                    if (_cancellationToken.IsCancellationRequested) 
                        break;
                    await _manualResetEvent.WaitAsync(_cancellationToken);
                }
            }
        }
    }
}
