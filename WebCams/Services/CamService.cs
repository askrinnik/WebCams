using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
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
      var cameras = _configuration.ReadCameras();
      
      var selectedCamera = cameras.SingleOrDefault(c => c.Id == camNum);

      _logger.LogInformation($"Getting image from {selectedCamera?.Address}");

      return selectedCamera?.Format switch
      {
        Format.Jpg => await new WebClient().DownloadDataTaskAsync(selectedCamera.Address),
        Format.Rtsp => await GetFromRtspClient(selectedCamera.Address, selectedCamera.ImageWidth, selectedCamera.ImageHeight),
        _ => null
      };
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
