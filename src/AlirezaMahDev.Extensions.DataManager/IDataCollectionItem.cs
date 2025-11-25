namespace AlirezaMahDev.Extensions.DataManager;

public interface IDataCollectionItem<TValue> : IDataValue<TValue>
    where TValue : unmanaged, IDataCollectionItem<TValue>
{
    long Next { get; set; }
}