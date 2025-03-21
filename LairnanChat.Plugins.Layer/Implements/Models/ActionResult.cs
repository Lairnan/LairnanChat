using System.Text.Encodings.Web;
using System.Text.Json;
using LairnanChat.Plugins.Layer.Enums;
using LairnanChat.Plugins.Layer.Interfaces.Models;

namespace LairnanChat.Plugins.Layer.Implements.Models;

public class ActionResult : IActionResult
{
    private readonly JsonSerializerOptions _jsonOptions = new()
    {
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
        WriteIndented = true
    };
    
    public ResultType ResultType { get; set; }
    public string? ResultData { get; set; }
    public string? ResultDataType { get; set; }

    public object? Data
    {
        get
        {
            if (ResultData == null || ResultDataType == null) return null;
            var obj = JsonSerializer.Deserialize(ResultData, Type.GetType(ResultDataType)!, _jsonOptions);
            return obj;
        }
    }
    
    public ActionResult() : this(ResultType.Connect)
    {
    }

    public ActionResult(ResultType resultType, object? resultData)
    {
        ResultType = resultType;
        ResultData = JsonSerializer.Serialize(resultData, _jsonOptions);
        ResultDataType = resultData?.GetType().FullName;
    }

    public ActionResult(ResultType resultType, string? resultData = null)
    {
        ResultType = resultType;
        ResultData = resultData;
        ResultDataType = typeof(string).FullName;
    }
}