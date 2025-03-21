using System.Collections;
using System.Collections.Specialized;
using System.ComponentModel;
using ChatClient.WPF.Interfaces.Models;

namespace ChatClient.WPF.Models;

public class DataCollectionView<T> : IDataCollectionView<T>
{
    private readonly Mutex _collectionMutex = new Mutex(true, "DataCollection_Mutex");

    private readonly int _test = new Random().Next(0, 666);
    private Predicate<T>? _filterPredicate;
    private Func<T, IComparable>? _sortPredicate;
    private IEnumerable<T>? _firstCollection;
    private List<T> _filteredItems;
    
    public T this[int index]
    {
        get => _filteredItems[index];
        set
        {
            if (this.IsReadOnly)
                throw new NotSupportedException("Collection is read only");
            
            _filteredItems[index] = value;
            Refresh(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Replace, value, index));
        }
    }

    public DataCollectionView(IEnumerable<T> source, bool isReadOnly = false)
    {
        this.IsReadOnly = isReadOnly;
        this.SourceCollection = new List<T>(source) ?? throw new ArgumentNullException(nameof(source));
        _filteredItems = new List<T>(this.SourceCollection);
    }

    public DataCollectionView(bool isReadOnly = false)
    {
        this.IsReadOnly = isReadOnly;
        this.SourceCollection = new List<T>();
        _filteredItems = this.SourceCollection.ToList();
    }

    private List<T> SourceCollection { get; set; }

    public Predicate<T>? FilterPredicate
    {
        get => _filterPredicate;
        set
        {
            if (_filterPredicate == value) return;
            _filterPredicate = value;
            Refresh();
        }
    }

    public IEnumerator<T> GetEnumerator()
    {
        foreach (var item in _filteredItems)
            yield return item;
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    private void Refresh(NotifyCollectionChangedEventArgs notify)
    {
        if (!_collectionMutex.WaitOne(10000))
            throw new TimeoutException("Could not acquire mutex");

        var t = _test;
        if (_filterPredicate != null)
        {
            _filteredItems.Clear();
            foreach (var item in this.SourceCollection)
            {
                if (_filterPredicate(item))
                {
                    _filteredItems.Add(item);
                }
            }
        }
        else
        {
            _filteredItems = new List<T>(this.SourceCollection);
        }

        Sort();

        if (_isSorted) return;
        OnCollectionChanged(notify);
        _collectionMutex.ReleaseMutex();
    }

    public void Refresh()
    {
        this.Refresh(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
    }

    public void SortBy(Func<T, IComparable> sortPredicate)
    {
        if (!_collectionMutex.WaitOne(10000))
            throw new TimeoutException("Could not acquire mutex");

        _sortPredicate = sortPredicate;
        Sort();
        if (!_isSorted)
            _collectionMutex.ReleaseMutex();
    }

    public void SortBy(IEnumerable<T> firstCollection)
    {
        if (!_collectionMutex.WaitOne(10000))
            throw new TimeoutException("Could not acquire mutex");

        _firstCollection = firstCollection;
        Sort();
        if (!_isSorted)
            _collectionMutex.ReleaseMutex();
    }

    private bool _isSorted = false;
    private void Sort()
    {
        _isSorted = false;
        if (_filteredItems is not List<T> list || (_sortPredicate == null && _firstCollection == null)) return;
        var newList = new List<T>();
        if (_firstCollection != null) newList.AddRange(_firstCollection);

        foreach (var item in list)
            if (!newList.Contains(item))
                newList.Add(item);

        if (_sortPredicate != null) newList.Sort((x, y) => _sortPredicate(x).CompareTo(_sortPredicate(y)));

        _filteredItems = newList;
        OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
        _isSorted = true;
        _collectionMutex.ReleaseMutex();
    }

    public void Clear()
    {
        if (!_collectionMutex.WaitOne(10000))
            throw new TimeoutException("Could not acquire mutex");

        this.SourceCollection = new List<T>();
        _filteredItems.Clear();
        _firstCollection = null;
        OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
        _collectionMutex.ReleaseMutex();
    }

    public bool Contains(T item)
    {
        if (!_collectionMutex.WaitOne(10000))
            throw new TimeoutException("Could not acquire mutex");

        var contain = this.SourceCollection.Contains(item);
        _collectionMutex.ReleaseMutex();
        return contain;
    }

    public void CopyTo(T[] array, int arrayIndex = 0)
    {
        if (!_collectionMutex.WaitOne(10000))
            throw new TimeoutException("Could not acquire mutex");

        if (array == null)
            throw new ArgumentNullException(nameof(array));
        if (arrayIndex < 0 || arrayIndex > this.Count - 1)
            throw new ArgumentOutOfRangeException(nameof(arrayIndex));
        if (array.Length > this.Count - arrayIndex)
            throw new ArgumentException("Array too small", nameof(array));

        var count = 0;
        foreach (var item in this.SourceCollection)
        {
            if (++count > array.Length) break;
            array[arrayIndex++] = item;
        }

        _collectionMutex.ReleaseMutex();
    }

    public void Add(T obj)
    {
        if (this.IsReadOnly)
            throw new NotSupportedException("Collection is read only");
        
        if (!_collectionMutex.WaitOne(10000))
            throw new TimeoutException("Could not acquire mutex");

        this.SourceCollection.Add(obj);
        Refresh(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, obj,
            this.SourceCollection.IndexOf(obj)));
        _collectionMutex.ReleaseMutex();
    }

    public bool Remove(T obj)
    {
        if (!_collectionMutex.WaitOne(10000))
            throw new TimeoutException("Could not acquire mutex");

        var result = false;
        var index = this.SourceCollection.IndexOf(obj);
        if (index >= 0)
        {
            result = true;
            this.SourceCollection.RemoveAt(index);

            Refresh(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, obj, index));

        }

        _collectionMutex.ReleaseMutex();
        return result;
    }

    public event NotifyCollectionChangedEventHandler? CollectionChanged;
    public event PropertyChangedEventHandler? PropertyChanged;
    event PropertyChangedEventHandler? INotifyPropertyChanged.PropertyChanged
    {
        add => PropertyChanged += value;
        remove => PropertyChanged -= value;
    }

    protected virtual void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
    {
        CollectionChanged?.Invoke(this, e);
        OnPropertyChanged(nameof(this.Count));
        OnPropertyChanged(nameof(this.IsEmpty));
    }

    protected virtual void OnPropertyChanged(string propertyName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
    
    public bool IsEmpty => _filteredItems.Count == 0;
    public int Count => _filteredItems.Count;
    public bool IsReadOnly { get; }
}