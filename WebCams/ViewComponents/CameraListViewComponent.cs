using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using WebCams.Services;

namespace WebCams.ViewComponents
{
  public class CameraListViewComponent: ViewComponent
  {
    private readonly IConfiguration _configuration;

    public CameraListViewComponent(IConfiguration configuration)
    {
      _configuration = configuration;
    }

    public IViewComponentResult Invoke()
    {
      return View(_configuration.ReadCameras());
    }


  }
}
