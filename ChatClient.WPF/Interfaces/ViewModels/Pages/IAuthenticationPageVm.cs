using System.Windows.Input;
using ChatClient.WPF.Models;

namespace ChatClient.WPF.Interfaces.ViewModels.Pages;

public interface IAuthenticationPageVm : IPageBaseVm
{
    string Username { get; }
    string Password { get; }
    Languages SelectedLanguage { get; }
    
    string ErrorMessage { get; }
    
    ICommand LoginCommand { get; }
    ICommand RegisterCommand { get; }
}