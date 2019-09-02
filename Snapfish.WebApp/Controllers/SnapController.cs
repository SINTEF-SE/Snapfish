using Microsoft.AspNetCore.Mvc;
using Snapfish.WebApp.Pages;

namespace Snapfish.WebApp.Controllers
{
    public class SnapController : Controller
    {
        [Route("/snap/{id}")]
        public IActionResult Snap(int id)
        {
            System.Console.WriteLine(id);
            ViewData["Title"] = "Snapfish Echoviewer";
            ViewData["SnapId"] = id;
            return View("/Pages/Snap.cshtml");
        }
    }
}