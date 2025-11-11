using FPTDrink.Web.Models;
using FPTDrink.Core.Interfaces.Services;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using FPTDrink.Web.Services;
using FPTDrink.Web.ViewModels;
using System.Text.Json;

namespace FPTDrink.Web.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
		private readonly IVisitorStatsService _visitorStatsService;
		private readonly ApiClient _apiClient;

		public HomeController(ILogger<HomeController> logger, IVisitorStatsService visitorStatsService, ApiClient apiClient)
        {
            _logger = logger;
			_visitorStatsService = visitorStatsService;
			_apiClient = apiClient;
        }

        public async Task<IActionResult> Index()
        {
			var cartId = Request.Cookies["cart_id"];
			var cartIdParam = !string.IsNullOrWhiteSpace(cartId) ? $"&cartId={System.Net.WebUtility.UrlEncode(cartId)}" : "";
			var blocks = await _apiClient.GetAsync<HomeBlocksViewModel>($"api/public/Home/blocks?limit=8{cartIdParam}") ?? new HomeBlocksViewModel();
            return View(blocks);
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
