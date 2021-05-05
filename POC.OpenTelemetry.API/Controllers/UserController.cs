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

        [HttpPost]
        public async Task<ActionResult> AddUser()
        {
            // Create user and insert into DB
            var user = BuildNewUser();
            await _context.Users.AddAsync(user);
            await _context.SaveChangesAsync();

            //// Checks exceptions in traces
            //throw new ArgumentNullException("Null exception");

            // Publish message to Rabbitmq queue
            var userAdded = new UserAddedEvent { Username = user.Username };
            Publish(userAdded);

            // Return trace Id
            return Ok(Activity.Current.TraceId.ToString());
        }

        [HttpGet]
        public IEnumerable<User> GetUsers()
        {
            return _context.Users.ToList();
        }

        private User BuildNewUser()
        {
            return new User
            {
                Id = Guid.NewGuid(),
                Name = "Jesus",
                Surname = "corral",
                Username = "jCorral"
            };
        }

        private void Publish(UserAddedEvent userAdded)
        {
            _messageSender.SendMessage(userAdded);
        }
    }
}
