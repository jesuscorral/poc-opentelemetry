using EventBusRabbitMQ;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using POC.OpenTelemetry.API.Data;
using POC.OpenTelemetry.API.Domain;
using POC.OpenTelemetry.API.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace POC.OpenTelemetry.API.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class WeatherForecastController : ControllerBase
    {
        private static IEventBusRabbitMQService _eventBus;
        private readonly Datacontext _context;

        private static readonly string[] Summaries = new[]
        {
            "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
        };

        private readonly ILogger<WeatherForecastController> _logger;

        public WeatherForecastController(ILogger<WeatherForecastController> logger,
            IEventBusRabbitMQService eventBus,
            Datacontext context)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _eventBus = eventBus ?? throw new ArgumentNullException(nameof(eventBus));
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        [HttpGet]
        public IEnumerable<WeatherForecast> Get()
        {
            var rng = new Random();

            var t = new AddUser { Username = "JesusCorral" };

            _eventBus.Publish(t);

            return Enumerable.Range(1, 5).Select(index => new WeatherForecast
            {
                Date = DateTime.Now.AddDays(index),
                TemperatureC = rng.Next(-20, 55),
                Summary = Summaries[rng.Next(Summaries.Length)]
            })
            .ToArray();
        }


        [HttpPost]
        public async Task<ActionResult> AddUser()
        {
            var user = new User
            {
                Id = Guid.NewGuid(),
                Name = "Jesus",
                Surname = "corral",
                Username = "jCorral"
            };

            await _context.Users.AddAsync(user);
            await _context.SaveChangesAsync();

            return Ok();

        }
    }
}
