using System.Text.Json;
using LairnanChat.Plugins.Layer.Enums;
using LairnanChat.Plugins.Layer.Implements.Models;
using Microsoft.Extensions.Logging;

namespace LairnanChat.Plugins.Layer.Implements.Services;

public partial class ChatServer
{
    /// <summary>
    /// Prepare request before broadcast
    /// </summary>
    /// <param name="actionRequest">Request for prepare</param>
    /// <param name="user">Listener user</param>
    private ActionResult? PrepareRequest(ActionRequest actionRequest, User user)
    {
        _logger.LogInformation("[{MethodName}] is starting", nameof(PrepareRequest));
        var actionResult = actionRequest.RequestType switch
        {
            RequestType.SendMessage => SendMessage(actionRequest),
            RequestType.ConnectToChat => ConnectToChat(actionRequest, user),
            RequestType.CreateChat => CreateChat(actionRequest, user),
            _ => null
        };
        
        if (actionResult != null)
        {
            var actionResultJson = JsonSerializer.Serialize(actionResult);
            _logger.LogInformation("[{MethodName}] Sending action result: {message}", nameof(PrepareRequest), actionResultJson);
        }
        else
        {
            _logger.LogInformation("[{MethodName}] Preparing message result actual is null", nameof(PrepareRequest));
        }
        return actionResult;
    }

    private ActionResult? CreateChat(ActionRequest actionRequest, User user)
    {
        ActionResult? actionResult = null;
        var chatRoom = JsonSerializer.Deserialize<ChatRoom>(actionRequest.RequestData!);
        if (chatRoom != null)
        {
            if (_chatRoomsDatabase.GetChatRoomById(chatRoom.Id) != null)
            {
                actionResult = new ActionResult(ResultType.Error, "Chat already exists, you cannot recreate this chat");
                return actionResult;
            }
            if (chatRoom.Participants.All(s => s.Id != user.Id))
            {
                chatRoom.AddParticipant(user);
            }
            _chatRoomsDatabase.AddChatRoom(chatRoom);
            actionResult = new ActionResult(ResultType.CreateChat, chatRoom);
        }
        return actionResult;
    }

    private ActionResult? ConnectToChat(ActionRequest actionRequest, User user)
    {
        ActionResult? actionResult = null;
        var chatRoomId = JsonSerializer.Deserialize<Guid>(actionRequest.RequestData!);
        if (chatRoomId != Guid.Empty)
        {
            if (!_chatRoomsDatabase.CanJoinThisChat(chatRoomId, user))
            {
                actionResult = new ActionResult(ResultType.NoPermission, "Cannot join this chat");
                return actionResult;
            }
                    
            var existingRoom = _chatRoomsDatabase.GetChatRoomById(chatRoomId);
            if (existingRoom == null)
            {
                actionResult = new ActionResult(ResultType.Error, "Chat not exists");
                return actionResult;
            }
                    
            var message = new Message(user, chatRoomId, $"User {user.UserName} joined the chat", "en-US");
            actionResult = new ActionResult(ResultType.SendMessage, message);
        }
        return actionResult;
    }

    private static ActionResult? SendMessage(ActionRequest actionRequest)
    {
        ActionResult? actionResult = null;
        var message = JsonSerializer.Deserialize<Message>(actionRequest.RequestData!);
        if (message != null)
        {
            actionResult = new ActionResult(ResultType.SendMessage, message);
        }
        return actionResult;
    }
}