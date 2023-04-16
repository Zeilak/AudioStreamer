using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Streamer
{
    public class WavStreamer : IStreamer
    {
        private byte[] _header;
        private WaveFormat _waveFormat;
        private WasapiLoopbackCapture _capture;

        public WavStreamer(WaveFormat waveFormat)
        {
            _waveFormat = waveFormat;
            SetHeader();
        }

        private void SetHeader()
        {
            var memoryStream = new MemoryStream();
            var writer = new WaveFileWriter(memoryStream, _waveFormat);
            writer.Flush();
            _header = memoryStream.ToArray();
        }
        public byte[] GetAudioFileHeader()
        {
            return _header;
        }

        public void Start(Func<byte[], bool> setChunkFunction)
        {
            var _capture = new WasapiLoopbackCapture();
            _capture.WaveFormat = _waveFormat;
            _capture.DataAvailable += (s, a) =>
            {
                byte[] forCopy = new byte[a.BytesRecorded];
                Array.Copy(a.Buffer, forCopy, a.BytesRecorded);
                setChunkFunction(forCopy);
            };
            _capture.RecordingStopped += (s, a) =>
            {
                _capture.Dispose();
            };
            _capture.StartRecording();
        }

        public void Stop()
        {
            _capture.StopRecording();
        }
    }
}
