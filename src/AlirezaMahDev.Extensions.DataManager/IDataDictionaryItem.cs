using System.Linq.Expressions;

namespace AlirezaMahDev.Extensions.DataManager;

public interface IDataDictionaryItem<TValue, TKey> : IDataCollectionItem<TValue>
    where TValue : unmanaged, IDataDictionaryItem<TValue, TKey>
    where TKey : unmanaged, IEquatable<TKey>
{
    TKey Key { get; set; }
}