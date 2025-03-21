using System.Windows.Controls;
using ChatClient.WPF.Interfaces.ViewModels.Pages;
using ChatClient.WPF.Interfaces.Views.Pages;

namespace ChatClient.WPF.Views.Pages;

public partial class AuthenticationPage : Page, IAuthenticationPage
{
    public AuthenticationPage()
    {
        InitializeComponent();
    }

    public AuthenticationPage(IAuthenticationPageVm viewModel) : this()
    {
        DataContext = viewModel;
    }

    public IPageBaseVm? DataContext { get; }
}