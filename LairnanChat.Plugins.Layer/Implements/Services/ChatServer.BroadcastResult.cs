using System.Net.WebSockets;
using System.Text.Json;
using LairnanChat.Plugins.Layer.Enums;
using LairnanChat.Plugins.Layer.Implements.Models;

namespace LairnanChat.Plugins.Layer.Implements.Services;

public partial class ChatServer
{
    /// <summary>
    /// Broadcast a message to all connected clients
    /// </summary>
    /// <param name="actionResult">Message that to broadcast</param>
    /// <param name="senderUser">User that send message</param>
    private async Task BroadcastResult(ActionResult actionResult, User senderUser)
    {
        switch (actionResult.ResultType)
        {
            case ResultType.SendMessage:
                await SendMessage(actionResult, senderUser);
                break;
            case ResultType.CreateChat:
                await CreateChat(actionResult);
                break;
            default:
                await DefaultAction(actionResult, senderUser);
                break;
        }
    }

    private async Task DefaultAction(ActionResult actionResult, User senderUser)
    {
        var wsClient = _clients.FirstOrDefault(s => s.Value == senderUser);
        if (wsClient.Key != null)
        {
            await SendObjectToClient(wsClient.Key, actionResult);
        }
    }

    private async Task CreateChat(ActionResult actionResult)
    {
        if (string.IsNullOrWhiteSpace(actionResult.ResultData)) return;
        var chatRoom = JsonSerializer.Deserialize<ChatRoom>(actionResult.ResultData);
        if (chatRoom == null) return;
                
        foreach (var (ws, user) in _clients.Where(s => chatRoom.ContainsUser(s.Value.Id)))
        {
            if (ws.State != WebSocketState.Open)
                continue;

            var message = new Message(user, chatRoom.Id, $"User {user.UserName} connected to chat", "en-US");
            var translatedContent = await _languagesService.TranslateAsync(message.OriginalContent!, message.OriginalLanguage, user.Language);
            message.SetTranslatedContent(translatedContent);
            await SendObjectToClient(ws, new ActionResult(ResultType.SendMessage, message));
        }
    }

    private async Task SendMessage(ActionResult actionResult, User senderUser)
    {
        var message = JsonSerializer.Deserialize<Message>(actionResult.ResultData!);
        if (message == null) return;

        var receiverIds = GetReceiversIds(senderUser, message);

        if (actionResult.ResultType != ResultType.Disconnect && !receiverIds.Contains(senderUser.Id))
            receiverIds.Add(senderUser.Id);
            
        foreach (var (webSocket, user) in _clients)
        {
            if (webSocket.State != WebSocketState.Open)
                continue;

            if (!receiverIds.Contains(user.Id)) continue;
                
            var outgoingMessage = await GetOutgoingMessage(message, user);
            await SendObjectToClient(webSocket, new ActionResult(ResultType.SendMessage, outgoingMessage));
        }
    }

    private List<Guid> GetReceiversIds(User senderUser, Message message)
    {
        var receiverIds = new List<Guid>();
        if (message.ChatRoomId != null)
        {
            var chatRoom = _chatRoomsDatabase.GetChatRoomById(message.ChatRoomId.Value);
            if (chatRoom == null)
                return receiverIds;

            if (!chatRoom.ContainsUser(senderUser.Id))
            {
                message.ChangeOriginalContent("You are not in a chat", "en-US");
                message.ChangeReceiver(senderUser);
                receiverIds.Clear();
                receiverIds.Add(senderUser.Id);
            }
            else
            {
                receiverIds.AddRange(chatRoom.Participants.Select(s => s.Id));
            }
            return receiverIds;
        }

        if (message.Receiver != null)
        {
            receiverIds.Add(message.Receiver.Id);
            return receiverIds;
        }

        var chatRooms = _chatRoomsDatabase.GetChatRoomsByParticipant(senderUser);
        foreach (var chatRoom in chatRooms)
        {
            receiverIds.AddRange(chatRoom.Participants.Select(s => s.Id));
        }
        return receiverIds;
    }
}