using System.Collections;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace ChatClient.WPF.Controls;

public partial class MultiSelectComboBox
{
    #region Hiden dependencies.
    public new static readonly DependencyProperty SelectedIndexProperty = DependencyProperty.Register(
        nameof(SelectedIndex), typeof(int), typeof(MultiSelectComboBox), new FrameworkPropertyMetadata(default(int),
            FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
            null, null, false, UpdateSourceTrigger.PropertyChanged));

    public new int SelectedIndex
    {
        get => (int)GetValue(SelectedIndexProperty);
        private set => SetValue(SelectedIndexProperty, value);
    }

    public new static readonly DependencyProperty SelectedItemProperty = DependencyProperty.Register(
        nameof(SelectedItem), typeof(object), typeof(MultiSelectComboBox), new FrameworkPropertyMetadata(default,
            FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
            null, null, false, UpdateSourceTrigger.PropertyChanged));

    public new object SelectedItem
    {
        get => GetValue(SelectedItemProperty);
        private set => SetValue(SelectedItemProperty, value);
    }

    public new static readonly DependencyProperty SelectedValueProperty = DependencyProperty.Register(
        nameof(SelectedValue), typeof(object), typeof(MultiSelectComboBox), new FrameworkPropertyMetadata(default,
            FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
            null, null, false, UpdateSourceTrigger.PropertyChanged));

    public new object SelectedValue
    {
        get => GetValue(SelectedValueProperty);
        private set => SetValue(SelectedValueProperty, value);
    }

    public new static readonly DependencyProperty SelectedValuePathProperty = DependencyProperty.Register(
        nameof(SelectedValuePath), typeof(string), typeof(MultiSelectComboBox), new FrameworkPropertyMetadata(default(string),
            FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
            null, null, false, UpdateSourceTrigger.PropertyChanged));

    public new string SelectedValuePath
    {
        get => (string)GetValue(SelectedValuePathProperty);
        private set => SetValue(SelectedValuePathProperty, value);
    }
    #endregion
    
    #region Dependency Properties.
    public new static readonly DependencyProperty ItemContainerStyleProperty = DependencyProperty.Register(
        nameof(ItemContainerStyle), typeof(Style), typeof(MultiSelectComboBox), new PropertyMetadata(default(Style)));

    public new Style ItemContainerStyle
    {
        get => (Style)GetValue(ItemContainerStyleProperty);
        private set => SetValue(ItemContainerStyleProperty, value);
    }

    public static readonly DependencyProperty SelectedItemsOverrideProperty =
        DependencyProperty.Register(nameof(SelectedItemsOverride), typeof(IList),
            typeof(MultiSelectComboBox), new FrameworkPropertyMetadata(default(IList), FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
                SelectedItemsChanged, null, false, UpdateSourceTrigger.PropertyChanged));

    private static void SelectedItemsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if(d is not MultiSelectComboBox multiSelectComboBox) return;
        var coll = e.NewValue as IList;
        multiSelectComboBox.UpdateMultiSelectItems();
        multiSelectComboBox.UpdateSelectionBoxItem(coll, multiSelectComboBox.Delimiter);
    }

    public IList? SelectedItemsOverride
    {
        get => (IList)GetValue(SelectedItemsOverrideProperty);
        set => SetValue(SelectedItemsOverrideProperty, value);
    }

    public static readonly DependencyProperty DelimiterProperty = DependencyProperty.Register(
        nameof(Delimiter), typeof(string), typeof(MultiSelectComboBox), new FrameworkPropertyMetadata(default(string),
            FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
            null, null, false, UpdateSourceTrigger.PropertyChanged));

    public string Delimiter
    {
        get => (string)GetValue(DelimiterProperty);
        set => SetValue(DelimiterProperty, value);
    }
    #endregion
    
    public MultiSelectComboBox()
    {
        InitializeComponent();
    }

    #region Override items container.
    protected override bool IsItemItsOwnContainerOverride(object item)
    {
        return item is MultiSelectComboBoxItem;
    }

    protected override DependencyObject GetContainerForItemOverride()
    {
        var multiSelectComboBoxItem = new MultiSelectComboBoxItem
        { Content = this.ItemTemplate.LoadContent() };
        multiSelectComboBoxItem.CheckedChanged += MultiSelectComboBoxItemOnCheckedChanged;
        return multiSelectComboBoxItem;
    }

    private uint _index;
    protected override void PrepareContainerForItemOverride(DependencyObject? element, object? item)
    {
        if (element is MultiSelectComboBoxItem multiSelectComboBoxItem)
        {
            multiSelectComboBoxItem.IsChecked = this.SelectedItemsOverride?.Contains(item) ?? false;
            multiSelectComboBoxItem.Value = item;
            multiSelectComboBoxItem.Index = _index;
            _index++;
        }
        else base.PrepareContainerForItemOverride(element, item);
    }
    #endregion
    
    #region Update multi select items.
    private void UpdateMultiSelectItems()
    {
        if (this.SelectedItemsOverride == null) return;
        foreach (var item in this.ItemsSource)
        {
            var multiSelectComboBoxItem = (MultiSelectComboBoxItem)this.ItemContainerGenerator.ContainerFromItem(item);
            if (multiSelectComboBoxItem == null) continue;
            multiSelectComboBoxItem.IsChecked = this.SelectedItemsOverride.Contains(item);
            multiSelectComboBoxItem.Value = item;
        }
    }

    private void MultiSelectComboBoxItemOnCheckedChanged(bool isChecked, object? item, uint index)
    {
        if (item == null) return;
        
        if (this.SelectedItemsOverride == null) return;
        
        switch (isChecked)
        {
            case false when this.SelectedItemsOverride.Contains(item):
                this.SelectedItemsOverride.Remove(item);
                break;
            case true when !this.SelectedItemsOverride.Contains(item):
                this.SelectedItemsOverride.Add(item);
                break;
        }

        UpdateSelectionBoxItem(this.SelectedItemsOverride, this.Delimiter);
    }

    private void UpdateSelectionBoxItem(IEnumerable? selectedItems, string delimiter)
    {
        var enumerable = selectedItems?.Cast<object>().ToList();
        if (enumerable?.Any() ?? false)
        {
            var text = new StringBuilder();
            var count = enumerable.Count;
            var i = 0;
            foreach (var obj in this.ItemsSource.Cast<object>())
            {
                if(count < i) break;
                if(!enumerable.Contains(obj)) continue;
                if (i > 0) text.Append(delimiter);
                text.Append(obj);
                i++;
            }
            this.SelectionBoxItem = text.ToString();
        }
        else this.SelectionBoxItem = "";
    }
    #endregion
}

public class MultiSelectComboBoxItem : ComboBoxItem
{
    #region Dependency Properties.
    public static readonly DependencyProperty IsCheckedProperty = DependencyProperty.Register(
        nameof(IsChecked), typeof(bool), typeof(MultiSelectComboBoxItem), new FrameworkPropertyMetadata(default(bool),
            FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
            IsCheckedChanged, null, false, UpdateSourceTrigger.PropertyChanged));

    public bool IsChecked
    {
        get => (bool)GetValue(IsCheckedProperty);
        set => SetValue(IsCheckedProperty, value);
    }

    public static readonly DependencyProperty ValueProperty = DependencyProperty.Register(
        nameof(Value), typeof(object), typeof(MultiSelectComboBoxItem), new FrameworkPropertyMetadata(default,
            FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
            null, null, false, UpdateSourceTrigger.PropertyChanged));

    public object? Value
    {
        get => GetValue(ValueProperty);
        set => SetValue(ValueProperty, value);
    }

    public static readonly DependencyProperty IndexProperty = DependencyProperty.Register(
        nameof(Index), typeof(uint), typeof(MultiSelectComboBoxItem), new FrameworkPropertyMetadata(default(uint),
            FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
            null, null, false, UpdateSourceTrigger.PropertyChanged));

    public uint Index
    {
        get => (uint)GetValue(IndexProperty);
        set => SetValue(IndexProperty, value);
    }
    #endregion

    public delegate void CheckedChangedEventHandler(bool isChecked, object? item, uint index);
    public event CheckedChangedEventHandler? CheckedChanged;
    
    private static void IsCheckedChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not MultiSelectComboBoxItem multiSelectComboBoxItem) return;
        var item = (bool)e.NewValue;
        multiSelectComboBoxItem.CheckedChanged?.Invoke(item, multiSelectComboBoxItem.Value, multiSelectComboBoxItem.Index);
    }
}