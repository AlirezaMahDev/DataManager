using System.Runtime.InteropServices;

namespace AlirezaMahDev.Extensions.DataManager;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public record struct DataPath(String64 Key, long Next, long Child, long Data, int Size)
    : IDataDictionaryTree<DataPath, String64>, IDataValueDefault<DataPath>, IDataStorage<DataPath>
{
    public static DataPath Default { get; } = new(default, -1L, -1L, -1L, -1);
}