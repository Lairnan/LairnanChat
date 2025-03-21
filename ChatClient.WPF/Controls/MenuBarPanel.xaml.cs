using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interop;

namespace ChatClient.WPF.Controls;

public partial class MenuBarPanel : UserControl
{
    public static readonly DependencyProperty HeaderProperty =
        DependencyProperty.Register(nameof(Header), typeof(object), typeof(MenuBarPanel),
            new UIPropertyMetadata(null, OnHeaderChanged));

    public new static readonly DependencyProperty ContentProperty =
        DependencyProperty.Register(nameof(Content), typeof(object), typeof(MenuBarPanel),
            new UIPropertyMetadata(null, OnContentChanged));

    public MenuBarPanel()
    {
        InitializeComponent();
        Loaded += (_, _) =>
        {
            var window = Window.GetWindow(this);
            if (window == null) return;

            window.MaxHeight = SystemParameters.MaximizedPrimaryScreenHeight;
            SwitchStateBtn.Visibility = window.ResizeMode != ResizeMode.NoResize
                ? Visibility.Visible
                : Visibility.Collapsed;
            SwitchStateBtn.Content = window.WindowState == WindowState.Maximized ? "2" : "1";

            window.StateChanged += (_, _) =>
                SwitchStateBtn.Content = window.WindowState == WindowState.Maximized ? "2" : "1";
        };
    }

    public object Header
    {
        get => GetValue(HeaderProperty);
        set => SetValue(HeaderProperty, value);
    }

    public new object Content
    {
        get => GetValue(ContentProperty);
        set => SetValue(ContentProperty, value);
    }

    private static void OnHeaderChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is MenuBarPanel panel) panel.HeaderPresenter.Content = e.NewValue;
    }

    private static void OnContentChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is MenuBarPanel panel) panel.ContentPresenter.Content = e.NewValue;
    }


    [DllImport("user32.dll")]
    private static extern IntPtr SendMessage(IntPtr hWnd, int wMsg, int wParam, int lParam);

    private void PnlControlBar_OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (e.ClickCount == 2)
        {
            SwitchState();
            return;
        }

        var window = Window.GetWindow(this);
        if (window == null) return;

        var helper = new WindowInteropHelper(window);
        SwitchStateBtn.Content = window.WindowState == WindowState.Maximized ? "2" : "1";
        SendMessage(helper.Handle, 161, 2, 0);
    }

    private void PnlControlBar_OnMouseEnter(object sender, MouseEventArgs e)
    {
        var window = Window.GetWindow(this);
        if (window != null) window.MaxHeight = SystemParameters.MaximizedPrimaryScreenHeight;
    }

    private void CloseBtn_Click(object sender, RoutedEventArgs e)
    {
        var window = Window.GetWindow(this);
        window?.Close();
    }

    private void MinimizeBtn_Click(object sender, RoutedEventArgs e)
    {
        var window = Window.GetWindow(this);
        if (window != null) window.WindowState = WindowState.Minimized;
    }

    private void SwitchStateBtn_Click(object sender, RoutedEventArgs e)
    {
        SwitchState();
    }

    private void SwitchState()
    {
        var window = Window.GetWindow(this);
        if (window == null) return;

        window.WindowState =
            window.WindowState == WindowState.Maximized ? WindowState.Normal : WindowState.Maximized;
        SwitchStateBtn.Content = window.WindowState == WindowState.Maximized ? "2" : "1";
    }
}