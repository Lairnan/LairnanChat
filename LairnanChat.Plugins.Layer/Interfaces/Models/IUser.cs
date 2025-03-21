namespace LairnanChat.Plugins.Layer.Interfaces.Models;

public interface IUser
{
    /// <summary>
    /// Gets the unique identifier for the user.
    /// </summary>
    Guid Id { get; }

    /// <summary>
    /// Gets the username(login). Unique
    /// </summary>
    string UserName { get; }

    /// <summary>
    /// Gets the preferred language (e.g., "en-US", "ru-RU").
    /// </summary>
    string Language { get; }
}