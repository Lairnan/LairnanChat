using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using ChatClient.WPF.Interfaces;
using ChatClient.WPF.Interfaces.ViewModels.Windows;
using ChatClient.WPF.Interfaces.Views.Windows;

namespace ChatClient.WPF.Views;

public partial class AdditionalWindow : Window, IAdditionalWindow
{
    private ContentControl? _popUp = null;

    public AdditionalWindow()
    {
        InitializeComponent();
        if (base.DataContext is IWindowBaseVm windowBaseVm)
        {
            this.DataContext = windowBaseVm;
            windowBaseVm.WindowBase = this;
        }
    }

    protected override void OnGotFocus(RoutedEventArgs e)
    {
        Global.LastActiveWindow = this;
        base.OnGotFocus(e);
    }
    
    public AdditionalWindow(IAdditionalWindowVm viewModel) : this()
    {
        this.DataContext = viewModel;
        viewModel.WindowBase = this;
    }

    public new IWindowBaseVm? DataContext { get; }
    
    public void ShowPopUp(IPopUp popUp)
    {
        if (_popUp != null)
            ClosePopUp();

        _popUp = (ContentControl)popUp;
        this.GridPopUpWrapper.PopUp = _popUp;
        this.GridPopUpWrapper.Visibility = Visibility.Visible;
    }

    public void ClosePopUp()
    {
        if (_popUp == null) return;
        this.GridPopUpWrapper.PopUp = null;
        this.GridPopUpWrapper.Visibility = Visibility.Collapsed;
        _popUp = null;
    }

    public void DragMovePopUp(object sender, MouseButtonEventArgs e)
    {
        if (this.GridPopUpWrapper.PopUp == null) return;
        this.GridPopUpWrapper.PopUpObject_OnMouseLeftButtonDown(sender, e);
    }
}