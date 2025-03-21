using ChatClient.WPF.Interfaces;

namespace ChatClient.WPF.Services.Interface;

public interface IPageService
{
    bool CanGoBack { get; }
    event Action<IPageBase> OnPageChanged;

    void Navigate(IPageBase page);
    void GoBack();
}