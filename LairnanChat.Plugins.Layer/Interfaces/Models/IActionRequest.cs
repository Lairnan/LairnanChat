using LairnanChat.Plugins.Layer.Enums;

namespace LairnanChat.Plugins.Layer.Interfaces.Models;

public interface IActionRequest
{
    RequestType RequestType { get; }
    string? RequestData { get; }
    string? RequestDataType { get; }
    object? Data { get; }
}