using System.IO;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Simple.Controllers
{
    public class ImagesController : Controller
    {
        [Authorize]
        public IActionResult BannerImage()
        {
            var file = Path.Combine(Directory.GetCurrentDirectory(), "MyStaticFiles", "images", "banner1.svg");
            return PhysicalFile(file, "image/svg+xml");
        }
    }
}
