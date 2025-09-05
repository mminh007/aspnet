using Frontend.HttpsClients.Stores;
using Frontend.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace Frontend.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IStoreApiClient _storeApiClient;

        public HomeController(IStoreApiClient storeApiClient, ILogger<HomeController> logger)
        {
            _storeApiClient = storeApiClient;
            _logger = logger;
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpGet("all")]
        [AllowAnonymous]

        public async Task<IActionResult> GetAllStores()
        {
            var results = await _storeApiClient.GetStoresAsync();

            return View(results);
        }
    }
}
