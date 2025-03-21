using ChatClient.WPF.Interfaces;
using ChatClient.WPF.Services.Interface;

namespace ChatClient.WPF.Services.Implementation;

public class PageService : IPageService
{
    private readonly Stack<IPageBase> _history = new();
    public bool CanGoBack => _history.Skip(1).Any();

    public event Action<IPageBase>? OnPageChanged;

    public void Navigate(IPageBase page)
    {
        page.DataContext?.SetPageService(this);
        OnPageChanged?.Invoke(page);
        _history.Push(page);
    }

    public void GoBack()
    {
        if (!this.CanGoBack) return;

        _history.Pop();
        var page = _history.Peek();
        page.DataContext?.SetPageService(this);
        OnPageChanged?.Invoke(page);
    }
}