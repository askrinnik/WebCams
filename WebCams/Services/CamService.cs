using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using RtspClientSharpCore;
using RtspClientSharpCore.RawFrames;

namespace WebCams.Services
{
  public class CamService
  {
    private readonly ILogger<CamService> _logger;
    private readonly IConfiguration _configuration;

    public CamService(ILogger<CamService> logger, IConfiguration configuration)
    {
      _logger = logger;
      _configuration = configuration;
    }

    public async Task<byte[]> GetCamImage(int camNum)
    {
      _logger.LogInformation($"Image #{camNum} is requested");

      if(camNum == 1)
      {
        // Reolink RLC-410W
        var address = _configuration["CameraAddresses:1"];
        return await new WebClient().DownloadDataTaskAsync(address);
      }

      if (camNum == 2)
      {
        //IP Camera Sricam SP012
        var address = _configuration["CameraAddresses:2"];
        return await GetFromRtspClient(address, 1280, 738);
      }
      if (camNum == 3)
      {
        // Reolink RLC-410W RTSP
        var address = _configuration["CameraAddresses:3"];
        return await GetFromRtspClient(address, 2560, 1440);
      }

      return null;
    }

    private static async Task<byte[]> GetFromRtspClient(string address, int streamWidth, int streamHeight)
    {
      var serverUri = new Uri(address);
      //var credentials = new NetworkCredential("admin", "admin12345678");
      var connectionParameters = new ConnectionParameters(serverUri/*, credentials*/);

      var cancellationTokenSource = new CancellationTokenSource();
      Bitmap bitmap = null;

      var frameDecoder = new FrameDecoder();
      var frameTransformer = new FrameTransformer(streamWidth, streamHeight);
      try
      {
        using var rtspClient = new RtspClient(connectionParameters);
        rtspClient.FrameReceived += delegate (object o, RawFrame rawFrame)
        {
          if (!(rawFrame is RtspClientSharpCore.RawFrames.Video.RawVideoFrame rawVideoFrame))
            return;

          var decodedFrame = frameDecoder.TryDecode(rawVideoFrame);

          if (decodedFrame == null) return;

          bitmap = frameTransformer.TransformToBitmap(decodedFrame);
          cancellationTokenSource.Cancel();
        };

        await rtspClient.ConnectAsync(cancellationTokenSource.Token);
        await rtspClient.ReceiveAsync(cancellationTokenSource.Token);

      }
      catch (OperationCanceledException)
      {
      }

      if (bitmap != null)
      {
        //var tempFileName = Path.GetTempFileName();
        //bitmap?.Save(tempFileName, ImageFormat.Jpeg);

        await using var stream = new MemoryStream();
        bitmap.Save(stream, ImageFormat.Jpeg);
        return stream.ToArray();
      }
      return null;
    }
  }
}
