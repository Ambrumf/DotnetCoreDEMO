using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

public class HomeController : Controller
{
    private readonly IOptionsMonitor<ThemeOptions> _themeoptions;
    public HomeController(IOptionsMonitor<ThemeOptions> themeoptions)
    {
        _themeoptions = themeoptions;
    }

    public IActionResult Index()
    {
        var theme = _themeoptions.Get("theme-1");
        ViewBag.Content = theme.ContentColor;
        ViewBag.Back = theme.BackColor;
        return View();
    }

    public IActionResult SetTheme(string theme)
    {
        var tho = _themeoptions.Get(theme);
        ViewBag.Content = tho.ContentColor;
        ViewBag.Back = tho.BackColor;
        ViewBag.currentSelect = theme;
        return View("Index");
    }
}