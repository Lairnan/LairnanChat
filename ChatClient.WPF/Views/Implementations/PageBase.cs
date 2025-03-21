using System.Windows.Controls;
using ChatClient.WPF.Interfaces;
using ChatClient.WPF.Interfaces.ViewModels.Pages;

namespace ChatClient.WPF.Views.Implementations;

public class PageBase : Page, IPageBase
{
    public PageBase()
    {
        this.Title = "Empty page";
    }

    public PageBase(IPageBaseVm pageBaseVm) : this()
    {
        this.DataContext = pageBaseVm;
    }

    public new IPageBaseVm? DataContext { get; }
}