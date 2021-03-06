using Microsoft.AspNetCore.Mvc;
using opcua_to_prometheus.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace opcua_to_prometheus.Controllers
{
    public class HomeController : Controller
    {
        private readonly ConfigService configService;

        public HomeController(ConfigService configService)
        {
            this.configService = configService;
        }
        [HttpGet("/metrics")]
        public IActionResult Metrics()
        {
            return View(configService.ActiveConfig);
        }
    }
}
