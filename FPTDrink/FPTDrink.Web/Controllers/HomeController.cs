using FPTDrink.Web.Models;
using FPTDrink.Core.Interfaces.Services;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace FPTDrink.Web.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
		private readonly IVisitorStatsService _visitorStatsService;

		public HomeController(ILogger<HomeController> logger, IVisitorStatsService visitorStatsService)
        {
            _logger = logger;
			_visitorStatsService = visitorStatsService;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

		public async Task<IActionResult> Refresh()
		{
			var stats = await _visitorStatsService.GetVisitorStatsAsync();
			return PartialView("_VisitorStatsPartial", stats);
		}

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
