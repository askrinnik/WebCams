using System.Net;
using System.Threading.Tasks;

namespace WebCams.Services
{
  public class CamService
  {
    private readonly string[] _camUrls =
    {
      "http://192.168.0.189/cgi-bin/api.cgi?cmd=Snap&channel=0&user=MirrorBoy&password=123454321"
    };
    public async Task<byte[]> GetCamImage(int camNum)
    {
      if (camNum < 1 || camNum > _camUrls.Length)
        return null;
      var response = await new WebClient().DownloadDataTaskAsync(_camUrls[camNum-1]);
      return response;
    }
  }
}
