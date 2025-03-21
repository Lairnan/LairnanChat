using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using ChatClient.WPF.Interfaces;
using ChatClient.WPF.Interfaces.ViewModels.Windows;

namespace ChatClient.WPF.Views.Implementations;

public class WindowBase : Window, IWindowBase
{
    private ContentControl? _popUp = null;
    
    public WindowBase()
    {
        InitializeComponent();
    }

    private void InitializeComponent()
    {
        this.Title = "WindowBase";
        this.Width = 400;
        this.Height = 200;
        this.AllowsTransparency = true;
        this.Background = new SolidColorBrush(Colors.Transparent);
        this.ResizeMode = ResizeMode.NoResize;
        this.WindowStartupLocation = WindowStartupLocation.CenterScreen;
        this.WindowStyle = WindowStyle.None;
        this.Visibility = Visibility.Visible;
    }

    public new IWindowBaseVm? DataContext { get; }
    
    public void ShowPopUp(IPopUp popUp)
    {
        _popUp = (ContentControl)popUp;
        this.AddChild(_popUp);
        ((Border)_popUp.Content).MouseLeftButtonDown += DragMovePopUp;
        this.ShowDialog();
    }

    public void ClosePopUp()
    {
        if (_popUp == null) return;
        _popUp = null;
        this.DialogResult = true;
        this.Close();
    }

    public void DragMovePopUp(object sender, MouseButtonEventArgs e)
    {
        if (_popUp == null) return;
        this.DragMove();
    }
}