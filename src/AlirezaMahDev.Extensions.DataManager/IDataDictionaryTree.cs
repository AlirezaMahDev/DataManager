namespace AlirezaMahDev.Extensions.DataManager;

public interface IDataDictionaryTree<TValue, TKey> : IDataDictionary<TValue, TValue, TKey>,
    IDataDictionaryItem<TValue, TKey>
    where TValue : unmanaged, IDataDictionaryTree<TValue, TKey>
    where TKey : unmanaged, IEquatable<TKey>;