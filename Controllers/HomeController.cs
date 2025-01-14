using System.Diagnostics;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using PokerTest.Models;

namespace PokerTest.Controllers;

public class HomeController : Controller
{
    private IPokerService _pokerService;
    private readonly ILogger<HomeController> _logger;

    public HomeController(ILogger<HomeController> logger, IPokerService pokerService)
    {
        _logger = logger;
        _pokerService = pokerService;
    }

    public IActionResult Index()
    {
        return View();
    }


    public IActionResult Game()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
