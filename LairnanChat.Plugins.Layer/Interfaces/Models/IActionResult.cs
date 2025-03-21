using LairnanChat.Plugins.Layer.Enums;

namespace LairnanChat.Plugins.Layer.Interfaces.Models;

public interface IActionResult
{
    ResultType ResultType { get; }
    string? ResultData { get; }
    string? ResultDataType { get; }
    object? Data { get; }
}