namespace AlirezaMahDev.Extensions.DataManager;

public interface IDataValue<TValue> : IEquatable<TValue>
    where TValue : unmanaged, IDataValue<TValue>;