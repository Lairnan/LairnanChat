using LairnanChat.Plugins.Layer.Interfaces.Models;

namespace LairnanChat.Plugins.Layer.Implements.Models;

public class ChatRoom : IChatRoom
{
    public Guid Id { get; init; }
    
    /// <summary>
    /// Gets the name of the chat room.
    /// </summary>
    public string RoomName { get; set; }

    /// <summary>
    /// Gets the list of participants in the chat room.
    /// </summary>
    public IList<User> Participants { get; init; } = new List<User>();

    public ChatRoom() : this(Guid.NewGuid(), string.Empty)
    {
    }

    public ChatRoom(string roomName) : this(Guid.NewGuid(), roomName)
    {
    }

    public ChatRoom(Guid id, string roomName)
    {
        Id = id;
        RoomName = roomName;
    }

    public bool ContainsUser(Guid participantId)
    {
        return Participants.FirstOrDefault(p => p.Id == participantId) != null;
    }

    /// <summary>
    /// Occurs when a new message is received in the chat room.
    /// </summary>
    public event EventHandler<Message>? MessageReceived;

    /// <summary>
    /// Sends the specified message to the chat room asynchronously.
    /// </summary>
    /// <param name="message">The message to send.</param>
    public void SendMessage(Message message)
    {
        MessageReceived?.Invoke(this, message);
    }

    public void AddParticipant(User user)
    {
        if (!Participants.Contains(user))
            Participants.Add(user);
    }

    public void RemoveParticipant(User user)
    {
        Participants.Remove(user);
    }
}