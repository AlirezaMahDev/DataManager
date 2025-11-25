namespace AlirezaMahDev.Extensions.DataManager;

public record struct DataLocationWrap<TValue, TWrap>(DataLocation<TValue> Location)
    where TValue : unmanaged, IDataValue<TValue>;