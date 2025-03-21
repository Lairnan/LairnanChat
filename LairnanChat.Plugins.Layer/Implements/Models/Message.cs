using System.Text.Json.Serialization;
using LairnanChat.Plugins.Layer.Interfaces.Models;

namespace LairnanChat.Plugins.Layer.Implements.Models;

public class Message : IMessage
{
    /// <summary>
    /// Gets the unique identifier for the message.
    /// </summary>
    public Guid MessageId { get; init; }

    /// <summary>
    /// Gets the sender of the message.
    /// </summary>
    public User Sender { get; init; }

    /// <summary>
    /// Gets the receiver of the message (null for public chat).
    /// </summary>
    [JsonInclude]
    public User? Receiver { get; private set; }
    
    [JsonInclude]
    public Guid? ChatRoomId { get; private set; }

    /// <summary>
    /// Gets the timestamp when the message was sent.
    /// </summary>
    public DateTime Timestamp { get; init; }

    /// <summary>
    /// Gets the original content of the message.
    /// </summary>
    [JsonInclude]
    public string? OriginalContent { get; private set; }

    /// <summary>
    /// Gets the language of the original content.
    /// </summary>
    [JsonInclude]
    public string OriginalLanguage { get; private set; }

    /// <summary>
    /// Gets the translated content, if translation is requested.
    /// </summary>
    [JsonInclude]
    public string? TranslatedContent { get; private set; }
    
    /// <summary>
    /// Gets object like image or files
    /// </summary>
    [JsonInclude]
    public object? MessageContent { get; private set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="Message"/> class.
    /// </summary>
    /// <param name="sender">The sender of the message.</param>
    /// <param name="receiver">The receiver of the message (null for public chat).</param>
    /// <param name="originalLanguage">The original language of the message.</param>
    /// <param name="originalContent">The original content of the message.</param>
    /// <param name="messageContent">The object like image or file</param>
    public Message(User sender, User? receiver, string originalLanguage, string? originalContent = null, object? messageContent = null)
    {
        MessageId = Guid.NewGuid();
        Sender = sender;
        Receiver = receiver;
        OriginalContent = originalContent;
        OriginalLanguage = originalLanguage;
        Timestamp = DateTime.UtcNow;
        MessageContent = messageContent;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="Message"/> class.
    /// </summary>
    /// <param name="sender">The sender of the message.</param>
    /// <param name="chatRoomId">The chat room of the message (null for public chat).</param>
    /// <param name="originalLanguage">The original language of the message.</param>
    /// <param name="originalContent">The original content of the message.</param>
    /// <param name="messageContent">The object like image or file</param>
    public Message(User sender, Guid? chatRoomId, string originalLanguage, string? originalContent = null, object? messageContent = null)
    {
        MessageId = Guid.NewGuid();
        Sender = sender;
        ChatRoomId = chatRoomId;
        OriginalContent = originalContent;
        OriginalLanguage = originalLanguage;
        Timestamp = DateTime.UtcNow;
        MessageContent = messageContent;
    }

    public Message() : this(null!, chatRoomId: null, null!, null, null)
    {
    }

    public Message Clone()
    {
        return new Message
        {
            MessageId = MessageId,
            Sender = Sender,
            Receiver = Receiver,
            OriginalContent = OriginalContent,
            OriginalLanguage = OriginalLanguage,
            Timestamp = Timestamp,
            ChatRoomId = ChatRoomId,
            TranslatedContent = TranslatedContent,
            MessageContent = MessageContent
        };
    }

    public void ChangeOriginalLanguage(string newLanguage)
    {
        OriginalLanguage = newLanguage;
    }

    public void ChangeOriginalContent(string newContent, string? newLanguage = null)
    {
        OriginalContent = newContent;
        OriginalLanguage = newLanguage ?? OriginalLanguage;
    }

    public void ChangeMessageContent(object? newMessageContent)
    {
        MessageContent = newMessageContent;
    }

    public void ChangeReceiver(User newReceiver)
    {
        Receiver = newReceiver;
        ChatRoomId = null;
    }

    public void ChangeReceiver(Guid newChatRoomId)
    {
        Receiver = null;
        ChatRoomId = newChatRoomId;
    }

    /// <summary>
    /// Sets the translated content of the message.
    /// </summary>
    /// <param name="translatedContent">The translated content.</param>
    public void SetTranslatedContent(string translatedContent)
    {
        TranslatedContent = translatedContent;
    }
}