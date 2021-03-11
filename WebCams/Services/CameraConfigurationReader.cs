using Microsoft.Extensions.Configuration;

namespace WebCams.Services
{
  public enum Format { Jpg, Rtsp }
  public class Camera
  {
    public int Id { get; set; }
    public string Model { get; set; }
    public string Title { get; set; }
    public Format Format { get; set; }
    public string Address { get; set; }
    public int ImageWidth { get; set; }
    public int ImageHeight { get; set; }
  }

  public static class CameraConfigurationReader
  {
    public static Camera[] ReadCameras(this IConfiguration configuration)
    {
      var cameras = configuration.GetSection("Cameras");
      var result = cameras.Get<Camera[]>();
      return result;
    }
  }
}
