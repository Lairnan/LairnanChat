using ChatClient.WPF.Interfaces;

namespace ChatClient.WPF.Behaviors;

public static class Factory
{
    public static DialogButton ShowPopUp(string title, string message, double timeout, IWindowBase window,
        params DialogButton[]? buttons)
    {
        if (buttons == null || buttons.Length == 0)
            buttons = new DialogButton[]
            {
                new(ButtonType.Ok, "Okay"),
            };

        var dialog = IoC.Resolve<IPopUp>();
        dialog.SetWindowBase(window);
        dialog.SetButtons(buttons);
        dialog.SetMessage(message);
        dialog.SetTitle(title);
        dialog.SetTimeout(timeout);
        dialog.ShowPopUp();

        var button = dialog.GetSelectedButton();
        button.Action?.Invoke();

        return button;
    }

    public static DialogButton ShowPopUp(string title, string message, double timeout = 0, bool newWindow = true,
        params DialogButton[]? buttons)
    {
        IWindowBase? window = null;
        if (!newWindow)
            window = Global.LastActiveWindow;

        window ??= IoC.Resolve<IWindowBase>();
        return ShowPopUp(title, message, timeout, window, buttons);
    }
    
    public static async Task<DialogButton> ShowPopUpAsync(string title, string message, double timeout, IWindowBase window,
        params DialogButton[]? buttons) => await Task.Run(() => ShowPopUp(title, message, timeout, window, buttons));
    
    public static async Task<DialogButton> ShowPopUpAsync(string title, string message, double timeout, bool newWindow = true,
        params DialogButton[]? buttons) => await Task.Run(() => ShowPopUp(title, message, timeout, newWindow, buttons));
}