using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace ChatClient.WPF.Controls;

public class BaseComboBox : ComboBox
{
    public BaseComboBox()
    {
        SelectionChanged += (_, _) =>
        {
            this.SelectionBoxItem = this.SelectedItem;
            if (Command?.CanExecute(CommandParameter) ?? false) Command?.Execute(CommandParameter);
        };
    }
    
    private static readonly DependencyPropertyKey _selectionBoxItemPropertyKey =
        DependencyProperty.RegisterReadOnly("SelectionBoxItem", typeof(object), typeof(BaseComboBox),
            new FrameworkPropertyMetadata(string.Empty));
    
    public new static readonly DependencyProperty SelectionBoxItemProperty = _selectionBoxItemPropertyKey.DependencyProperty;

    public new object SelectionBoxItem
    {
        get => GetValue(SelectionBoxItemProperty);
        protected private set => SetValue(_selectionBoxItemPropertyKey, value);
    }

    public static readonly DependencyProperty CommandProperty = DependencyProperty.Register(
        nameof(Command), typeof(ICommand), typeof(BaseComboBox), new PropertyMetadata(default(ICommand)));

    public ICommand Command
    {
        get => (ICommand)GetValue(CommandProperty);
        set => SetValue(CommandProperty, value);
    }

    public static readonly DependencyProperty CommandParameterProperty = DependencyProperty.Register(
        nameof(CommandParameter), typeof(object), typeof(BaseComboBox), new PropertyMetadata(default(object)));

    public object CommandParameter
    {
        get => GetValue(CommandParameterProperty);
        set => SetValue(CommandParameterProperty, value);
    }
}