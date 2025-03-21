using LairnanChat.Plugins.Layer.Implements.Models;
using LairnanChat.Plugins.Layer.Interfaces.Services;
using Microsoft.Extensions.Configuration;

namespace LairnanChat.Plugins.Layer.Implements.Services
{
    public class ChatRoomsDatabase : IChatRoomsDatabase
    {
        private readonly List<ChatRoom> _chatRooms;
        private readonly Guid _generalChatRoomId;

        public ChatRoomsDatabase(IConfiguration configuration)
        {
            var roomName = configuration.GetValue<string>("GeneralChatName") ?? "LairnanChat General";
            var generalChatRoom = new ChatRoom(roomName);

            _chatRooms = [ generalChatRoom ];
            _generalChatRoomId = generalChatRoom.Id;
        }

        public ChatRoom? GetGeneralChatRoom()
        {
            return _chatRooms.FirstOrDefault(s => s.Id == _generalChatRoomId);
        }

        public void AddChatRoom(ChatRoom chatRoom)
        {
            _chatRooms.Add(chatRoom);
        }

        public IEnumerable<ChatRoom> GetChatRooms()
        {
            return _chatRooms.AsEnumerable();
        }

        public ChatRoom? GetChatRoomById(Guid id)
        {
            return _chatRooms.FirstOrDefault(c => c.Id == id);
        }

        public List<ChatRoom> GetChatRoomByName(string chatRoomName)
        {
            return _chatRooms.Where(c => c.RoomName == chatRoomName).ToList();
        }

        public bool CanJoinThisChat(Guid id, User user)
        {
            var room = GetChatRoomById(id);
            return room != null && room.Participants.All(u => u.Id != user.Id);
        }

        public IList<ChatRoom> GetChatRoomsByParticipant(User user)
        {
            return _chatRooms.Where(c => c.Participants.Any(u => u.Id == user.Id)).ToList();
        }
    }
}