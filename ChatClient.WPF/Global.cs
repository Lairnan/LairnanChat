using ChatClient.WPF.Interfaces;

namespace ChatClient.WPF;

public static class Global
{
    public static IWindowBase LastActiveWindow { get; set; } = null!;
}