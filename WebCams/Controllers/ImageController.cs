using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using WebCams.Services;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace WebCams.Controllers
{
  [Route("api/[controller]")]
  [ApiController]
  public class ImageController : ControllerBase
  {
    private readonly CamService _camService;

    public ImageController(CamService camService)
    {
      _camService = camService;
    }

    // GET: api/<ImageController>
    [HttpGet]
    public async Task<ActionResult> Get()
    {
      return await Get(1);
    }

    // GET api/<ImageController>/5
    [HttpGet("{id}")]
    public async Task<ActionResult> Get(int id)
    {
      var imageData = await _camService.GetCamImage(id);
      if (imageData == null)
        return NotFound();
      return File(imageData, "image/jpeg");
    }
  }
}
