using System.Diagnostics;
using ClientTopicMVC.Models;
using Microsoft.AspNetCore.Mvc;
using RabbitMQTopicHWReceiverSection.Services;

namespace ClientTopicMVC.Controllers
{
    public class HomeController : Controller
    {
        private readonly RabbitMqListenerService _rabbitMqListenerService;
        private readonly IRedisService _redisService;


        public HomeController(RabbitMqListenerService rabbitMqListenerService, IRedisService redisService)
        {
            _rabbitMqListenerService = rabbitMqListenerService;
            _redisService = redisService;
        }

        public IActionResult Index()
        {
            string routingKey = TempData["routingKey"] as string;



            Console.WriteLine(routingKey);

            Thread.Sleep(500);

            HomeViewModel model = new HomeViewModel()
            {
                RabbitMQDivisions = _redisService.GetRabbitMQDivisionList(),
                RoutingKey = routingKey == null ? "" : routingKey,
                Messages = _rabbitMqListenerService.GetMessages()
            };



            return View(model);
        }



       
    }
}
