using System.Diagnostics;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using PokerTest.Models;
using PokerTest.Services.Interface;

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

}
