using AspireKafka.Domain;
using AspireKafka.Infrastructure;
using MassTransit;
using Microsoft.AspNetCore.Mvc;


namespace AspireKafka.ApiService.Controllers;

[ApiController]
[Route("[controller]")]
public class UserController : ControllerBase
{
    private readonly ILogger<UserController> _logger;
    private readonly ITopicProducer<string, UserCreatedEvent> _producer;
    private readonly DataContext _context;

    public UserController(ILogger<UserController> logger, ITopicProducer<string, UserCreatedEvent> producer, DataContext context)
    {
        _logger = logger;
        _producer = producer;
        _context = context;
    }

    [HttpPost(Name = "Create")] 
    public async Task<IActionResult> Create([FromBody] UserCreate user)
    {
        try
        {
            var newUser = Domain.User.Create(user.Username);

            await _context.Users.AddAsync(newUser);
            await _context.SaveChangesAsync();

            await _producer.Produce(Const.UserTopicName, new UserCreatedEvent(newUser.Id, newUser.Username, DateTime.UtcNow));

            return Ok(newUser);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating user:");
            return BadRequest("An error occurred while creating the user.");
        }
     }
}
