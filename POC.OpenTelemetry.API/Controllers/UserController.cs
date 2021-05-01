using EventBusRabbitMQ;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using POC.OpenTelemetry.API.Data;
using POC.OpenTelemetry.API.Domain;
using POC.OpenTelemetry.API.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace POC.OpenTelemetry.API.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class UserController : ControllerBase
    {
        private static IEventBusRabbitMQService _eventBus;
        private readonly Datacontext _context;

        public UserController(ILogger<UserController> logger,
            IEventBusRabbitMQService eventBus,
            Datacontext context)
        {
            _eventBus = eventBus ?? throw new ArgumentNullException(nameof(eventBus));
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        [HttpGet]
        public IEnumerable<User> GetUsers()
        {
            return _context.Users.ToList();
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

            await Publish(user);

            return Ok(Activity.Current.TraceId);
        }

        private async Task Publish(User user)
        {
            var userAdded = new UserAddedEvent { Username = user.Username };

            await _eventBus.Publish(userAdded);
        }
    }
}
