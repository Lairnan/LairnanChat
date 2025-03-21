using System.Windows.Input;
using ChatClient.WPF.Interfaces;
using ChatClient.WPF.Interfaces.ViewModels.Pages;
using ChatClient.WPF.Interfaces.ViewModels.Windows;
using ChatClient.WPF.Services.Interface;
using MvvmBuilder.Commands;
using MvvmBuilder.Notifies;

namespace ChatClient.WPF.ViewModels;

public class AdditionalViewModel : NotifyBase, IAdditionalWindowVm
{
    private readonly IPageService _pageService;
    private ICommand? _goToBackCommand;

    public AdditionalViewModel(IPageService pageService)
    {
        _pageService = pageService;
        _pageService.OnPageChanged += NavigateAction;
        
        NavigationCommands.BrowseBack.InputGestures.Clear();
        NavigationCommands.BrowseForward.InputGestures.Clear();
        NavigationCommands.BrowseHome.InputGestures.Clear();
        NavigationCommands.BrowseStop.InputGestures.Clear();
    }

    private async void NavigateAction(IPageBase page)
    {
        this.Title = page.Title;
        this.CurrentPageVm = page.DataContext;
        if (page.DataContext != null)
        {
            page.DataContext.SetPageService(_pageService);
            page.DataContext.SetBaseWindow(this.WindowBase!);
        }

        await Task.Delay(350);
        this.CurrentPage = page;
        await Task.Delay(25);
    }

    public ICommand GoToBackCommand => _goToBackCommand ??=
        new RelayCommand(() => _pageService.GoBack(), () => _pageService.CanGoBack);

    public IPageBase CurrentPage
    {
        get => GetProperty<IPageBase>();
        private set => SetProperty(value);
    }

    public string Title
    {
        get => GetProperty<string>();
        set => SetProperty(value, startValue: "Доп. окно");
    }

    public IPageBaseVm? CurrentPageVm
    {
        get => GetProperty<IPageBaseVm?>();
        private set => SetProperty(value);
    }

    public IWindowBase WindowBase
    {
        get => GetProperty<IWindowBase>();
        set => SetProperty(value);
    }

    public void ChangePage<T>()
        where T : IPageBase
    {
        var page = IoC.Resolve<T>();
        _pageService.Navigate(page);
    }
}