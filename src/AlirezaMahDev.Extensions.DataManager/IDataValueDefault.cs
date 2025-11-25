namespace AlirezaMahDev.Extensions.DataManager;

public interface IDataValueDefault<TValue> : IEquatable<TValue>
    where TValue : unmanaged, IDataValue<TValue>
{
    static abstract TValue Default { get; }
}