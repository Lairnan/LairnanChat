using LairnanChat.Plugins.Layer.Implements;
using LairnanChat.Plugins.Layer.Implements.Models;

namespace LairnanChat.Plugins.Layer.Interfaces.Models;

public interface IChatRoom
{
    Guid Id { get; }
    
    /// <summary>
    /// Gets the name of the chat room.
    /// </summary>
    string RoomName { get; }

    /// <summary>
    /// Gets the list of participants in the chat room.
    /// </summary>
    IList<User> Participants { get; }

    /// <summary>
    /// Occurs when a new message is received in the chat room.
    /// </summary>
    event EventHandler<Message> MessageReceived;

    /// <summary>
    /// Sends the specified message to the chat room asynchronously.
    /// </summary>
    /// <param name="message">The message to send.</param>
    void SendMessage(Message message);
    
    /// <summary>
    /// Add user in chat room
    /// </summary>
    /// <param name="user">User that connected to chat room</param>
    void AddParticipant(User user);
    
    /// <summary>
    /// Remove user from chat room
    /// </summary>
    /// <param name="user">User that disconnected from chat room</param>
    void RemoveParticipant(User user);
}