using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using AudioStreamer.Services;
using System.Net;
using Microsoft.VisualBasic;
using Microsoft.AspNetCore.Http.Features;
using System.Text;
using Microsoft.AspNetCore.Http;
using System.Reflection.PortableExecutable;

namespace AudioStreamer.Controllers
{
    [ApiController]
    public class MainController : ControllerBase
    {

        private readonly StreamService _stremService;

        public MainController(StreamService streamService)
        {
            _stremService = streamService;
        }

        [HttpGet("/stream")]
        public async Task<IActionResult> GetStream(CancellationToken cancellationToken)
        {
            Response.ContentType = "audio/x-wav";
            Response.Headers.Connection = "close";
            await _stremService.ServeClientAndStartStream(Response.Body, cancellationToken);
            await Task.Delay(1000000);
            return Ok();
        }


        [HttpGet]
        [Route("/record")]
        public async Task<IActionResult> RecordFile(string outputFilePath, CancellationToken cancellationToken)
        {
            await using (Response.Body.ConfigureAwait(false))
            {
                var waveFormat = new WaveFormat(44100, 32, 2);
                var capture = new WasapiLoopbackCapture();
                capture.WaveFormat = waveFormat;
                var writer = new WaveFileWriter(outputFilePath, waveFormat);
                capture.DataAvailable += (s, a) =>
                {
                    writer.Write(a.Buffer, 0, a.BytesRecorded);
                    Console.WriteLine(a.BytesRecorded);
                    if (writer.Position > capture.WaveFormat.AverageBytesPerSecond * 20)
                    {
                        writer.Flush();
                    }
                };
                capture.RecordingStopped += (s, a) =>
                {
                    writer.Flush();
                    writer.Dispose();
                    writer = null;
                    capture.Dispose();
                };
                capture.StartRecording();
                while (capture.CaptureState != NAudio.CoreAudioApi.CaptureState.Stopped)
                {
                    Thread.Sleep(500);
                    if (cancellationToken.IsCancellationRequested)
                    {
                        capture.StopRecording();
                        break;
                    }
                }
            }
            return Ok();
        }

    }
}
