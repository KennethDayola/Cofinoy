using AutoMapper;
using Cofinoy.WebApp.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Cofinoy.WebApp.Controllers
{
    /// <summary>
    /// Menu Controller
    /// </summary>
    public class OrderController : ControllerBase<OrderController>
    {
        public OrderController(
            IHttpContextAccessor httpContextAccessor,
            ILoggerFactory loggerFactory,
            IConfiguration configuration,
            IMapper mapper = null
        ) : base(httpContextAccessor, loggerFactory, configuration, mapper)
        {
        }

        public IActionResult Index()
        {

            return View();
        }

    }
}