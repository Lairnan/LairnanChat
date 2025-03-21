using System.Collections.ObjectModel;
using System.Windows.Input;
using ChatClient.WPF.Models;
using LairnanChat.Plugins.Layer.Implements.Models;

namespace ChatClient.WPF.Interfaces.ViewModels.Windows;

public interface IMainWindowVm : IWindowBaseVm
{
    ObservableCollection<ChatServerInfo> Servers { get; }
    ChatServerInfo? SelectedServer { get; set; }
    ICommand SelectedServerChangedCommand { get; }
}