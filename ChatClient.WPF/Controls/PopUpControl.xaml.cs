using System.Windows;
using System.Windows.Controls;
using ChatClient.WPF.Behaviors;
using ChatClient.WPF.Interfaces;
using Timer = System.Timers.Timer;

namespace ChatClient.WPF.Controls;

public partial class PopUpControl : ContentControl, IPopUp
{
    private double _timeout = 0;
    private DialogButton _selectedButton = null!;
    private Timer? _timer;
    private bool _isClosed = false;
    private IWindowBase _windowBase = null!;
    private readonly object _lock = new object();

    public PopUpControl()
    {
        InitializeComponent();
    }

    public void Close()
    {
        lock (_lock)
        {
            if (_isClosed) return;
            _isClosed = true;
        }

        if (_timer is { Enabled: true }) _timer.Enabled = false;
        _timer = null;
        
        _windowBase.ClosePopUp();
        _windowBase = null!;
    }

    public void SetButtons(DialogButton[] buttons)
    {
        ButtonsPanel.Children.Clear();
        foreach (var button in buttons)
            ButtonsPanel.Children.Add(GetButton(button));
    }

    public void SetMessage(string message)
    {
        MessageBlock.Text = message;
    }

    public void SetTitle(string title)
    {
        this.Title.Text = title;
    }

    public DialogButton GetSelectedButton()
    {
        return _selectedButton;
    }

    public async void ShowPopUp()
    {
        if (_timeout == 0)
        {
            Application.Current.Dispatcher.Invoke(() => _windowBase.ShowPopUp(this));
            await WaitSelectButton();
            return;
        }

        _timer = new Timer();
        _timer.Interval = _timeout * 1000;
        _timer.Elapsed += (sender, args) =>
        {
            _timer.Stop();
            _selectedButton = new DialogButton(ButtonType.None, "Cancel by auto timeout");
            Application.Current.Dispatcher.Invoke(Close);
        };
        _timer.Start();
        _windowBase.ShowPopUp(this);
    }

    private async Task WaitSelectButton()
    {
        while (_selectedButton == null)
            await Task.Delay(500);
    }

    public void SetTimeout(double timeout)
    {
        _timeout = timeout;
    }

    public void SetWindowBase(IWindowBase windowBase)
    {
        _windowBase = windowBase;
    }

    private Button GetButton(DialogButton dialogButton)
    {
        var button = new Button
        {
            Style = (Style)FindResource("MainButtonStyle"),
            Content = dialogButton.Title,
            Margin = new Thickness(0, 0, 10, 0)
        };
        
        if (dialogButton.ButtonType == ButtonType.None)
            button.Visibility = Visibility.Collapsed;
        else if (dialogButton.ButtonType == ButtonType.No)
            button.Style = (Style)FindResource("RedMainButtonStyle");
        
        button.Click += (sender, args) =>
        {
            _selectedButton = dialogButton;
            this.Close();
        };
        return button;
    }

    private void ButtonBase_CloseClick(object sender, RoutedEventArgs e)
    {
        _selectedButton = new DialogButton(ButtonType.None, "Cancel by close");
        this.Close();
    }
}