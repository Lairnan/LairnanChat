using System.Net.WebSockets;
using System.Windows.Input;
using ChatClient.WPF.Behaviors;
using ChatClient.WPF.Interfaces;
using ChatClient.WPF.Interfaces.ViewModels.Pages;
using ChatClient.WPF.Interfaces.ViewModels.Windows;
using ChatClient.WPF.Interfaces.Views.Pages;
using ChatClient.WPF.Interfaces.Views.Windows;
using ChatClient.WPF.Models;
using ChatClient.WPF.Services.Interface;
using LairnanChat.Plugins.Layer.Enums;
using LairnanChat.Plugins.Layer.Implements.Models;
using LairnanChat.Plugins.Layer.Interfaces.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MvvmBuilder.Commands;
using MvvmBuilder.Notifies;

namespace ChatClient.WPF.ViewModels.Pages;

public class AuthenticationPageVm : NotifyBase, IAuthenticationPageVm
{
    #region Private fields.

    private IPageService? _pageService;
    private IWindowBase _windowBase = null!;
    
    private readonly ILogger<AuthenticationPageVm> _logger;
    private readonly IChatServerManager _chatServerManager;
    private readonly IViewFactory _viewFactory;
    private IChatService? _chatService = null;

    private string _selectedLanguage = string.Empty;
    private ICommand? _loginCommand;
    private ICommand? _registerCommand;
    
    #endregion

    public AuthenticationPageVm(ILogger<AuthenticationPageVm> logger, IChatServerManager chatServerManager, IViewFactory viewFactory, IConfiguration configuration)
    {
        logger.LogInformation("AuthenticationPageVm initialized");
        _logger = logger;
        
        _chatServerManager = chatServerManager;
        _viewFactory = viewFactory;
        OnLanguagesChange(SelectedLanguage);
        _chatService = _chatServerManager.ActiveChatService;
        _chatServerManager.ActiveChatServiceChanged += ActiveServerChanged;
    }

    private void ActiveServerChanged(IChatService? obj)
    {
        _chatService = obj;
        if (obj == null) return;
        
        Task.Run(async () =>
        {
            await Task.Delay(5000);
            CheckConnectionServer();
        });
    }

    private static bool IsConnected(WebSocketState state)
    {
        return state is WebSocketState.Open or WebSocketState.Connecting;
    }

    private void CheckConnectionServer()
    {
        if (_pageService == null) return;
        var window = _viewFactory.IsWindowCreated<IMainWindow, IAuthenticationPage>();
        if (window == null)
        {
            if (_chatService == null || !IsConnected(_chatService.GetConnectionStatus()))
            {
                Factory.ShowPopUp("Error", "You are not connected to server.", 0, true, new DialogButton(ButtonType.Ok, "Ok"));
                if (_pageService != null)
                {
                    var getChatsPage = IoC.Resolve<IAuthenticationPage>();
                    _pageService.Navigate(getChatsPage);
                }
            }
            return;
        }
        
        if (_chatService == null || !IsConnected(_chatService.GetConnectionStatus())) return;

        Factory.ShowPopUp("Warning", "You already connected to the server.", 0, true, new DialogButton(ButtonType.Ok, "Ok"));
        if (_pageService != null)
        {
            var getChatsPage = IoC.Resolve<IGetChatsPage>();
            _pageService.Navigate(getChatsPage);
        }
    }

    #region Properties

    public string Username
    {
        get => GetProperty<string>();
        set => SetProperty(value);
    }

    public string Password
    {
        get => GetProperty<string>();
        set => SetProperty(value);
    }

    public Languages SelectedLanguage
    {
        get => GetProperty<Languages>();
        set => SetProperty(value, OnLanguagesChange, Languages.Russian);
    }

    private void OnLanguagesChange(Languages lang)
    {
        _selectedLanguage = lang switch
        {
            Languages.Russian => "ru-RU",
            Languages.English => "en-US",
            _ => "ru-RU"
        };
    }

    public string ErrorMessage
    {
        get => GetProperty<string>();
        private set => SetProperty(value);
    }
    
    public ICommand LoginCommand => _loginCommand ??= new RelayCommand(async void () =>
    {
        if (_chatService == null)
        {
            ErrorMessage = "Need to select server";
            return;
        }
        ErrorMessage = string.Empty;
        var authUser = new AuthUser(Username, Password, _selectedLanguage);
        try
        {
            var actionResult = await _chatService.ConnectAsync(authUser);
            if (actionResult.ResultType == ResultType.Error)
            {
                ErrorMessage = "Error connecting to server. See logs for details.";
                return;
            }
        }
        catch (Exception ex)
        {
            ErrorMessage = "Error connecting to server. See logs for details.";
            _logger.LogCritical(ex, "Error connecting to server");
        }
    }, () => !string.IsNullOrWhiteSpace(Username) && !string.IsNullOrWhiteSpace(Password));
    
    public ICommand RegisterCommand => _registerCommand ??= new RelayCommand(async () =>
    {
        if (_chatService == null)
        {
            ErrorMessage = "Need to select server";
            return;
        }
        try
        {
            if (_chatService.GetConnectionStatus() != WebSocketState.Open)
                return;

            await _chatService.DisconnectAsync();
        }
        catch (Exception ex)
        {
            _logger.LogCritical(ex, "Error disconnecting from server");
        }
    });

    #endregion

    #region Implementation IPageBaseVm

    public void SetBaseWindow(IWindowBase windowBase)
    {
        _windowBase = windowBase;
    }

    public void SetPageService(IPageService pageService)
    {
        _pageService = pageService;
        Task.Run(async () =>
        {
            await Task.Delay(5000);
            CheckConnectionServer();
        });
    }

    #endregion
}