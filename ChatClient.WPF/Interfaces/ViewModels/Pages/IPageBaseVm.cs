using ChatClient.WPF.Services.Interface;

namespace ChatClient.WPF.Interfaces.ViewModels.Pages;

public interface IPageBaseVm : IViewModelBase
{
    void SetBaseWindow(IWindowBase windowBase);
    void SetPageService(IPageService pageService);
}