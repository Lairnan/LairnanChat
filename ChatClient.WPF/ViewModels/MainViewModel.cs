using System.Collections.ObjectModel;
using System.Windows.Input;
using ChatClient.WPF.Interfaces;
using ChatClient.WPF.Interfaces.ViewModels.Pages;
using ChatClient.WPF.Interfaces.ViewModels.Windows;
using ChatClient.WPF.Interfaces.Views.Pages;
using ChatClient.WPF.Services.Interface;
using ChatClient.WPF.Views.Implementations;
using LairnanChat.Plugins.Layer.Implements.Models;
using LairnanChat.Plugins.Layer.Interfaces.Services;
using MvvmBuilder.Commands;
using MvvmBuilder.Notifies;

namespace ChatClient.WPF.ViewModels;

public class MainViewModel : NotifyBase, IMainWindowVm
{
    private readonly IPageService _pageService;
    private readonly IChatServerManager _chatServerManager;

    private IPageBaseVm? _currentPageVm;
    private ICommand? _goToBackCommand;
    private ICommand? _selectedServerChangedCommand;

    public MainViewModel(IPageService pageService, IChatServerManager chatServerManager)
    {
        _chatServerManager = chatServerManager;
        this.Servers = new ObservableCollection<ChatServerInfo>(_chatServerManager.AvailableServers);
        this.SelectedServer = this.Servers.FirstOrDefault();
        SelectedServerChangedCommand.Execute(this.SelectedServer);
        
        _pageService = pageService;
        _pageService.OnPageChanged += NavigateAction;
        this.ChangePage<IAuthenticationPage>();
        
		NavigationCommands.BrowseBack.InputGestures.Clear();
        NavigationCommands.BrowseForward.InputGestures.Clear();
        NavigationCommands.BrowseHome.InputGestures.Clear();
        NavigationCommands.BrowseStop.InputGestures.Clear();
    }

    public IPageBase CurrentPage
    {
        get => GetProperty<IPageBase>(() => new PageBase());
        private set => SetProperty(value);
    }

    public string Title
    {
        get => GetProperty<string>();
        set => SetProperty(value, startValue: "Главное окно");
    }

    public ICommand GoToBackCommand => _goToBackCommand ??=
        new RelayCommand(() => _pageService.GoBack(), () => _pageService.CanGoBack);

    private async void NavigateAction(IPageBase page)
    {
        this.Title = page.Title;
        this.CurrentPageVm = page.DataContext;

        if (page.DataContext != null)
        {
            page.DataContext.SetPageService(_pageService);
            page.DataContext.SetBaseWindow(this.WindowBase);
        }
        
        await Task.Delay(350);
        this.CurrentPage = page;
        await Task.Delay(25);
    }
    
    public IPageBaseVm? CurrentPageVm
    {
        get => _currentPageVm;
        private set => SetProperty(ref _currentPageVm, value);
    }

    public IWindowBase WindowBase
    {
        get => GetProperty<IWindowBase>();
        set => SetProperty(value, action: s => { this.CurrentPageVm?.SetBaseWindow(s); });
    }

    public void ChangePage<T>()
        where T : IPageBase
    {
        var page = IoC.Resolve<T>();
        _pageService.Navigate(page);
    }

    public ObservableCollection<ChatServerInfo> Servers { get; private set; }

    public ChatServerInfo? SelectedServer
    {
        get => GetProperty<ChatServerInfo?>();
        set => SetProperty(value);
    }
    public ICommand SelectedServerChangedCommand => _selectedServerChangedCommand ??=
        new RelayCommand<ChatServerInfo>(server =>
        {
            _chatServerManager.SetActiveServer(server);
            if (Servers.FirstOrDefault(s => s.Url == server.Url) == null) Servers.Add(server);
            // TODO: сделать возможность добавлять новый сервер при выборе в списке (Добавить новый). Должен быть в самом верху, всегда
        });
}