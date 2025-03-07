using System.Text.Json;
using AspireKafka.Domain;
using AspireKafka.Infrastructure;
using DotNetCore.CAP;
using Microsoft.AspNetCore.Mvc;


namespace AspireKafka.ApiService.Controllers;

[ApiController]
[Route("[controller]")]
public class UserController : ControllerBase
{
    private readonly ILogger<UserController> _logger;
    private readonly DataContext _context;
    private readonly ICapPublisher _capPublisher;

    public UserController(ILogger<UserController> logger, DataContext context, ICapPublisher capPublisher)
    {
        _logger = logger;
        _context = context;
        _capPublisher = capPublisher;
    }

    [HttpPost(Name = "Create")] 
    public async Task<IActionResult> Create([FromBody] UserCreate user)
    {
        using var transaction = _context.Database.BeginTransaction();
        try
        {
            var newUser = Domain.User.Create(user.Username);

            await _context.Users.AddAsync(newUser);
            
            var evnt = new UserCreatedEvent(newUser.Id, newUser.Username, DateTime.UtcNow);
            
            var content = JsonSerializer.Serialize(evnt);
            await _capPublisher.PublishAsync<string>("UserAdded", content);

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();
            
            return Ok(newUser);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating user:");
            await transaction.RollbackAsync();
            return BadRequest("An error occurred while creating the user.");
        }
    }
}
