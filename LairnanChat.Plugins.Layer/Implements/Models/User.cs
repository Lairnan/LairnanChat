using LairnanChat.Plugins.Layer.Interfaces.Models;

namespace LairnanChat.Plugins.Layer.Implements.Models;

public class User : IUser
{
    /// <summary>
    /// Gets the unique identifier for the user.
    /// </summary>
    public Guid Id { get; init; }

    /// <summary>
    /// Gets the username(login). Unique
    /// </summary>
    public string UserName { get; init; }

    /// <summary>
    /// Gets the preferred language (e.g., "en-US", "ru-RU").
    /// </summary>
    public string Language { get; init; }

    /// <summary>
    /// Initializes a new instance of the <see cref="User"/> class.
    /// </summary>
    /// <param name="userName">The username.</param>
    /// <param name="language">The preferred language.</param>
    public User(string userName, string language)
    {
        Id = Guid.NewGuid();
        UserName = userName;
        Language = language;
    }
}