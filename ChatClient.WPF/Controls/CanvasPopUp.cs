using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace ChatClient.WPF.Controls;

public class CanvasPopUp : Canvas
{
    private UIElement? _draggedItem = null;
    private Point _itemRelativePosition;
    
    public static readonly DependencyProperty PopUpProperty = DependencyProperty.Register(nameof(PopUp), typeof(ContentControl),
        typeof(CanvasPopUp), new PropertyMetadata(null,
            OnPopUpChanged, null));

    private static void OnPopUpChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var instance = (CanvasPopUp)d;
        var content = (ContentControl)e.NewValue;
        if (content == null)
        {
            instance._draggedItem = null;
            instance.Children.Clear();
            return;
        }

        ((Border)content.Content).MouseLeftButtonDown += instance.PopUpObject_OnMouseLeftButtonDown;
        
        var parent = VisualTreeHelper.GetParent(content) as Panel;
        parent?.Children.Remove(content);
        
        if (instance.Children.Count > 0)
            instance.Children.Clear();
        
        instance.ReleaseMouseCapture();
        
        Point point;
        var window = Window.GetWindow(instance);
        if (window != null) point = new Point(window.Width / 2 - content.Width / 2, window.Height / 2 - content.Height / 2);
        else point = new Point(0, 0);
        
        Canvas.SetTop(content, point.Y);
        Canvas.SetLeft(content, point.X);
        instance.Children.Add(content);
    }

    public ContentControl? PopUp
    {
        get => (ContentControl?)GetValue(PopUpProperty);
        set => SetValue(PopUpProperty, value);
    }

    public CanvasPopUp()
    {
        this.PreviewMouseLeftButtonUp += CanvasPopUp_PreviewMouseLeftButtonUp;
        this.PreviewMouseMove += CanvasPopUp_PreviewMouseMove;
    }

    public void PopUpObject_OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        this._draggedItem = PopUp;
        _itemRelativePosition = e.GetPosition(_draggedItem);
        e.Handled = true;
    }

    private void CanvasPopUp_PreviewMouseMove(object sender, MouseEventArgs e)
    {
        if (_draggedItem == null)
            return;
        
        var newPos = e.GetPosition(this) - _itemRelativePosition;
        var window = Window.GetWindow(this);
        if (newPos.X < 0) newPos.X = 0;
        if (newPos.Y < 0) newPos.Y = 0;
        if (newPos.X + PopUp!.Width > window?.Width) newPos.X = window.Width - PopUp.Width;
        if (newPos.Y + PopUp.Height > window?.Height) newPos.Y = window.Height - PopUp.Height;
        
        Canvas.SetTop(_draggedItem, newPos.Y);
        Canvas.SetLeft(_draggedItem, newPos.X);
        this.CaptureMouse();
        e.Handled = true;
    }

    private void CanvasPopUp_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
    {
        if (this._draggedItem != null)
        {
            this._draggedItem = null;
            this.ReleaseMouseCapture();
            e.Handled = true;
        }
    }
}