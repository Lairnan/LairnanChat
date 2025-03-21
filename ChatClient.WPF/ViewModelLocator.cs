using ChatClient.WPF.Interfaces.ViewModels.Pages;
using ChatClient.WPF.Interfaces.ViewModels.Windows;

namespace ChatClient.WPF;

public class ViewModelLocator
{
    // Windows
    public IMainWindowVm MainViewModel => IoC.Resolve<IMainWindowVm>();
    public IAdditionalWindowVm AdditionalViewModel => IoC.Resolve<IAdditionalWindowVm>();
    
    // Pages
    public IAuthenticationPageVm AuthenticationPageVm => IoC.Resolve<IAuthenticationPageVm>();
}