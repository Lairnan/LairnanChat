using System.Windows;
using ChatClient.WPF.Interfaces;

namespace ChatClient.WPF.Services;

public class ViewFactory : IViewFactory
{
    public IWindowBase? CreateView<TWindow, TPage>()
        where TWindow : IWindowBase
        where TPage : IPageBase
    {
        var window = IsWindowCreated<TWindow, TPage>();
        if (window != null) return window;
        window = IoC.Resolve<TWindow>();

        var vm = window.DataContext;
        if (vm == null) return null;

        vm.ChangePage<TPage>();
        return window;
    }

    public IWindowBase? IsWindowCreated<TWindow, TPage>()
        where TWindow : IWindowBase
        where TPage : IPageBase
    {
        var window = Application.Current.Windows.OfType<TWindow>().FirstOrDefault(s =>
            s.DataContext is { CurrentPage: TPage });
        return window;
    }

    public IWindowBase? IsWindowCreated<TWindow>()
        where TWindow : IWindowBase
    {
        var window = Application.Current.Windows.OfType<TWindow>().FirstOrDefault();
        return window;
    }

    public IWindowBase CreateOnlyOneView<TWindow, TPage>()
        where TWindow : IWindowBase
        where TPage : IPageBase
    {
        foreach (var win in Application.Current.Windows.OfType<Window>().ToList())
        {
            if (win is not TWindow { DataContext.CurrentPage: TPage })
            {
                win.Close();
            }
        }

        // Проверяем, есть ли уже нужное окно
        var existing = IsWindowCreated<TWindow, TPage>();
        if (existing != null)
        {
            return existing;
        }

        // Создаём новое окно с нужной страницей
        var newWindow = IoC.Resolve<TWindow>();
        var vm = newWindow.DataContext;
        if (vm == null) return newWindow;

        vm.ChangePage<TPage>();
        newWindow.Show();

        return newWindow;
    }
}