using System.IO;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Simple.Controllers
{
    public class ImagesController : Controller
    {
        [Authorize]
        public IActionResult MyImage()
        {
            var file = Path.Combine(Directory.GetCurrentDirectory(), "StaticFiles", "imgs", "2.png");
            return PhysicalFile(file, "image/svg+xml");
        }
    }
}
