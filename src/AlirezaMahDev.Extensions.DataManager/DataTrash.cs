using System.Runtime.InteropServices;

namespace AlirezaMahDev.Extensions.DataManager;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public record struct DataTrash(long Child) : IDataCollection<DataTrash, DataTrashItem>, IDataValueDefault<DataTrash>
{
    public static DataTrash Default { get; } = new(-1);
}