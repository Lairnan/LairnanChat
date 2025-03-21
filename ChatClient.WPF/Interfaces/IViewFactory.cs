namespace ChatClient.WPF.Interfaces;

public interface IViewFactory
{
    IWindowBase? CreateView<TWindow, TPage>()
        where TWindow : IWindowBase
        where TPage : IPageBase;

    IWindowBase? IsWindowCreated<TWindow, TPage>()
        where TWindow : IWindowBase
        where TPage : IPageBase;

    IWindowBase? IsWindowCreated<TWindow>()
        where TWindow : IWindowBase;

    IWindowBase CreateOnlyOneView<TWindow, TPage>()
        where TWindow : IWindowBase
        where TPage : IPageBase;
}