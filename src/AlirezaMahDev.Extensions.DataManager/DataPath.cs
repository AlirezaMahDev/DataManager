namespace AlirezaMahDev.Extensions.DataManager;

public interface IDataCollection
{
    public long Next { get; set; }
    public long Child { get; set; }
}

public interface IDataDictionary<TKey> : IDataCollection
    where TKey : unmanaged, IEquatable<TKey>
{
    public TKey Key { get; set; }
}

public interface IDataValue<TSelf> : IEquatable<TSelf>
    where TSelf : unmanaged, IDataValue<TSelf>
{
    static abstract TSelf Default { get; }
}

public record struct DataPath(String64 Key, long Next, long Child) : IDataValue<DataPath>, IDataDictionary<String64>
{
    public static DataPath Default { get; } = new(default, -1L, -1L);
}