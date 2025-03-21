using System.Net.WebSockets;
using System.Text;

namespace LairnanChat.Plugins.Layer;

public record MessageReceive(bool IsClosed, string? Value);

public static class WebSocketExtensions
{
    public static async Task<MessageReceive> ReceiveFullMessageAsync(this WebSocket webSocket, int bufferSize = 8192, CancellationToken cancellationToken = default)
    {
        MessageReceive? messageReceive = null;
        var buffer = new byte[bufferSize];
        using var ms = new MemoryStream();
        WebSocketReceiveResult result;

        do
        {
            result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), cancellationToken);
            if (result.MessageType == WebSocketMessageType.Close)
            {
                messageReceive = new MessageReceive(true, null);
                break;
            }
            ms.Write(buffer, 0, result.Count);
        } while (!result.EndOfMessage);

        return messageReceive ?? new MessageReceive(false, Encoding.UTF8.GetString(ms.ToArray()));
    }
}