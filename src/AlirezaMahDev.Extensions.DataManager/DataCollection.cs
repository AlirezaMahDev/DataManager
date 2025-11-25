using System.Linq.Expressions;

namespace AlirezaMahDev.Extensions.DataManager;

// public readonly record struct DataCollection<TValue, TItem>(DataLocation<TValue> Location)
//     : IDataCollection<TValue, TItem>
//     where TValue : unmanaged, IDataCollection<TValue, TItem>
//     where TItem : unmanaged, IDataCollectionItem<TItem>
// {
//     public long Child { get => Location.Value.Child; set => Location.Value.Child = value; }
//     public bool Equals(TValue other) => Location.Value.Equals(other);
// }
//
// public record struct DataCollectionTree<TValue>(DataLocation<TValue> Location)
//     where TValue : unmanaged, IDataCollectionTree<TValue>;
//
// public record struct DataDictionary<TValue, TItem, TKey>(DataLocation<TValue> Location)
//     where TValue : unmanaged, IDataDictionary<TValue, TItem, TKey>, IDataValue<TValue>
//     where TItem : unmanaged, IDataDictionaryItem<TItem, TKey>
//     where TKey : unmanaged, IEquatable<TKey>;
//
// public record struct DataDictionaryTree<TValue, TKey>(DataLocation<TValue> Location)
//     where TValue : unmanaged, IDataDictionaryTree<TValue, TKey>, IDataValue<TValue>
//     where TKey : unmanaged, IEquatable<TKey>;
//     