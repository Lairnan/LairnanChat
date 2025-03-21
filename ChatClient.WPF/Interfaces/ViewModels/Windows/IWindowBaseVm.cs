using System.Windows.Input;
using ChatClient.WPF.Interfaces.ViewModels.Pages;

namespace ChatClient.WPF.Interfaces.ViewModels.Windows;

public interface IWindowBaseVm : IViewModelBase
{
    string Title { get; set; }
    IPageBase? CurrentPage { get; }
    IPageBaseVm? CurrentPageVm { get; }
    IWindowBase WindowBase { get; set; }
    ICommand GoToBackCommand { get; }

    void ChangePage<T>() where T : IPageBase;
}