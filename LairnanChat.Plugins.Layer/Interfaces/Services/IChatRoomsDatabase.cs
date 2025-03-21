using LairnanChat.Plugins.Layer.Implements.Models;

namespace LairnanChat.Plugins.Layer.Interfaces.Services;

public interface IChatRoomsDatabase
{
    ChatRoom? GetChatRoomById(Guid chatRoomId);
    IList<ChatRoom> GetChatRoomsByParticipant(User user);
    bool CanJoinThisChat(Guid chatRoomId, User user);
    void AddChatRoom(ChatRoom chatRoom);
    ChatRoom? GetGeneralChatRoom();
    IEnumerable<ChatRoom> GetChatRooms();
}