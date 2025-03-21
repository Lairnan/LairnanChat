using LairnanChat.Plugins.Layer.Implements;
using LairnanChat.Plugins.Layer.Implements.Models;

namespace LairnanChat.Plugins.Layer.Interfaces.Models;

public interface IMessage
{
    /// <summary>
    /// Gets the unique identifier for the message.
    /// </summary>
    Guid MessageId { get; }

    /// <summary>
    /// Gets the sender of the message.
    /// </summary>
    User Sender { get; }

    /// <summary>
    /// Gets the receiver of the message (null for public chat).
    /// </summary>
    User? Receiver { get; }
    
    Guid? ChatRoomId { get; }

    /// <summary>
    /// Gets the timestamp when the message was sent.
    /// </summary>
    DateTime Timestamp { get; }

    /// <summary>
    /// Gets the original content of the message.
    /// </summary>
    string? OriginalContent { get; }

    /// <summary>
    /// Gets the language of the original content.
    /// </summary>
    string OriginalLanguage { get; }

    /// <summary>
    /// Gets the translated content, if translation is requested.
    /// </summary>
    string? TranslatedContent { get; }
    
    /// <summary>
    /// Gets object like image or files
    /// </summary>
    object? MessageContent { get; }

    /// <summary>
    /// Sets the translated content of the message.
    /// </summary>
    /// <param name="translatedContent">The translated content.</param>
    void SetTranslatedContent(string translatedContent);

    void ChangeOriginalLanguage(string newLanguage);
    void ChangeOriginalContent(string newContent, string? newLanguage = null);
    void ChangeMessageContent(object? newMessageContent);

    void ChangeReceiver(User newReceiver);
    void ChangeReceiver(Guid newChatRoomId);
}