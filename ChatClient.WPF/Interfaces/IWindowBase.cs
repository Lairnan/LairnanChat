using System.Windows.Input;
using ChatClient.WPF.Interfaces.ViewModels.Windows;

namespace ChatClient.WPF.Interfaces;

public interface IWindowBase : IViewBase<IWindowBaseVm>
{
    void Show();
    void Close();
    bool? ShowDialog();
    bool? DialogResult { get; set; }
    
    void ShowPopUp(IPopUp popUp);
    void ClosePopUp();
    void DragMovePopUp(object sender, MouseButtonEventArgs e);
}