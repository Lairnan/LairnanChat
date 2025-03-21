using System.Text.Encodings.Web;
using System.Text.Json;
using LairnanChat.Plugins.Layer.Enums;
using LairnanChat.Plugins.Layer.Interfaces.Models;

namespace LairnanChat.Plugins.Layer.Implements.Models;

public class ActionRequest : IActionRequest
{
    private readonly JsonSerializerOptions _jsonOptions = new()
    {
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
    };
    
    public RequestType RequestType { get; init; }
    public string? RequestData { get; init; }
    public string? RequestDataType { get; init; }

    public object? Data
    {
        get
        {
            if (RequestData == null || RequestDataType == null) return null;
            var obj = JsonSerializer.Deserialize(RequestData, Type.GetType(RequestDataType)!, _jsonOptions);
            return obj;
        }
    }

    public ActionRequest() : this(RequestType.Connect, null)
    {
    }

    public ActionRequest(RequestType requestType) : this(requestType, null)
    {
    }

    public ActionRequest(RequestType requestType, object? requestData)
    {
        RequestType = requestType;
        RequestData = JsonSerializer.Serialize(requestData, _jsonOptions);
        RequestDataType = requestData?.GetType().FullName;
    }
}