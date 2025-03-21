using System.ComponentModel;
using System.Text.Json.Serialization;

namespace LairnanChat.Plugins.Layer.Implements.Models;

/// <summary>
/// Represents chat server details including its display name, URL, and current connection status.
/// </summary>
public class ChatServerInfo : INotifyPropertyChanged
{
    private bool _isConnected;
    private string _name = "";
    private string _url = "";

    /// <summary>
    /// Event for property change notifications (used for UI binding).
    /// </summary>
    public event PropertyChangedEventHandler? PropertyChanged;

    public ChatServerInfo(string name, string url, bool isConnected = false)
    {
        Name = name;
        Url = url;
        IsConnected = isConnected;
    }

    [JsonConstructor]
    public ChatServerInfo() : this(string.Empty, string.Empty, false)
    {
    }

    /// <summary>User-friendly name of the chat server.</summary>
    public string Name
    {
        get => _name;
        set
        {
            if (value == _name) return;
            _name = value;
            OnPropertyChanged(nameof(Name));
            OnPropertyChanged(nameof(DisplayName));
        }
    }

    /// <summary>Chat server endpoint URL.</summary>
    public string Url
    {
        get => _url;
        set
        {
            if (value == _url) return;
            _url = value;
            OnPropertyChanged(nameof(Url));
            OnPropertyChanged(nameof(DisplayName));
        }
    }

    /// <summary>Indicates whether currently connected to the server.</summary>
    [JsonIgnore]
    public bool IsConnected
    {
        get => _isConnected;
        set
        {
            if (_isConnected == value) return;
            _isConnected = value;
            OnPropertyChanged(nameof(IsConnected));
        }
    }

    [JsonIgnore]
    public string DisplayName
    {
        get
        {
            if (string.IsNullOrWhiteSpace(Url) && string.IsNullOrWhiteSpace(Name)) return "Error on convert info";
            if (!string.IsNullOrWhiteSpace(Name)) return Name;
            if (string.IsNullOrWhiteSpace(Url)) return "Error on convert info";
            var uri = new Uri(Url);
            return !uri.IsDefaultPort ? $"{uri.Host}:{uri.Port}" : uri.Host;
        }
    }

    protected virtual void OnPropertyChanged(string propertyName)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
}