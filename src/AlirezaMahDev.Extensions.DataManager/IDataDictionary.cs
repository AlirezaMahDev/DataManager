namespace AlirezaMahDev.Extensions.DataManager;

public interface IDataDictionary<TValue, TItem, TKey> : IDataCollection<TValue, TItem>
    where TValue : unmanaged, IDataDictionary<TValue, TItem, TKey>
    where TItem : unmanaged, IDataDictionaryItem<TItem, TKey>
    where TKey : unmanaged, IEquatable<TKey>;