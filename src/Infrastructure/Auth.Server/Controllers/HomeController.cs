using Microsoft.AspNetCore.Mvc;

namespace Auth.Server.Controllers;

public class HomeController : Controller
{
    public IActionResult Index() => View();
    public IActionResult Privacy() => View();
}
