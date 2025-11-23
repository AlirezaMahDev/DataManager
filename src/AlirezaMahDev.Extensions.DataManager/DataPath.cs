using System.Runtime.InteropServices;

namespace AlirezaMahDev.Extensions.DataManager;

public interface IDataCollection
{
    public long Child { get; set; }
}

public interface IDataCollectionItem
{
    public long Next { get; set; }
}

public interface IDataTreeCollection : IDataCollection, IDataCollectionItem
{
}

public interface IDataDictionaryItem<TKey> : IDataCollectionItem
    where TKey : unmanaged, IEquatable<TKey>
{
    public TKey Key { get; set; }
}

public interface IDataDictionary<TKey> : IDataCollection
    where TKey : unmanaged, IEquatable<TKey>
{
}

public interface IDataTreeDictionary<TKey> : IDataTreeCollection
    where TKey : unmanaged, IEquatable<TKey>
{
    public TKey Key { get; set; }
}

public interface IDataValue<TSelf> : IEquatable<TSelf>
    where TSelf : unmanaged, IDataValue<TSelf>
{
    static abstract TSelf Default { get; }
}

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public record struct DataPath(String64 Key, long Next, long Child) : IDataValue<DataPath>, IDataTreeDictionary<String64>
{
    public static DataPath Default { get; } = new(default, -1L, -1L);
}

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public record struct DataTrash(long Child)
    : IDataValue<DataTrash>, IDataCollection
{
    public static DataTrash Default { get; } = new(-1);
}

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public record struct DataTrashItem(long Offset, int Length, long Next)
    : IDataValue<DataTrashItem>, IDataCollectionItem
{
    public static DataTrashItem Default { get; } = new(-1L, -1, -1);
}