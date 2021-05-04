using Microsoft.AspNetCore.Mvc;
using POC.OpenTelemetry.API.Data;
using POC.OpenTelemetry.API.Domain;
using POC.OpenTelemetry.Worker.Models;
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
        private readonly Datacontext _context;
        private readonly MessageSender _messageSender;

        public UserController(Datacontext context, MessageSender messageSender)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _messageSender = messageSender ?? throw new ArgumentNullException(nameof(messageSender));
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

            return Ok(Activity.Current.TraceId.ToString());
        }

        private async Task Publish(User user)
        {
            var userAdded = new UserAddedEvent { Username = user.Username };

            _messageSender.SendMessage(userAdded);
        }
    }
}
