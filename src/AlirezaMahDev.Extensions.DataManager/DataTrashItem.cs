using System.Runtime.InteropServices;

namespace AlirezaMahDev.Extensions.DataManager;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public record struct DataTrashItem(long Offset, int Length, long Next) : IDataCollectionItem<DataTrashItem>,IDataValueDefault<DataTrashItem>
{
    public static DataTrashItem Default { get; } = new(-1L, -1, -1);
}