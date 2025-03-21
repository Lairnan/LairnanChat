using ChatClient.WPF.Behaviors;

namespace ChatClient.WPF.Interfaces;

public interface IPopUp
{
    void SetButtons(DialogButton[] buttons);
    void SetMessage(string message);
    void SetTitle(string title);
    
    DialogButton GetSelectedButton();
    void ShowPopUp();

    void SetTimeout(double timeout);

    void SetWindowBase(IWindowBase windowBase);
}