namespace AlirezaMahDev.Extensions.DataManager;

public interface IDataCollection<TValue, TItem> : IDataValue<TValue>
    where TValue : unmanaged, IDataCollection<TValue, TItem>
    where TItem : unmanaged, IDataCollectionItem<TItem>
{
    long Child { get; set; }
}