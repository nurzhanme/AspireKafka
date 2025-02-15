using MediatR;

namespace AspireKafka.Domain;

public class User
{
    private User()
    {
    }

    private User(string username, Guid id)
    {
        Username = username;
        Id = id;
    }

    public static User Create(string username)
    {
        return new User(username, Guid.CreateVersion7());
    }

    public Guid Id { get; private set; }
    public string Username { get; private set; }
}

public record UserCreatedEvent(Guid id, string Username, DateTime CreatedAt) : INotification;