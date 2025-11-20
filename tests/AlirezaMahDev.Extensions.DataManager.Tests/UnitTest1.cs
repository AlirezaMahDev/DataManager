namespace AlirezaMahDev.Extensions.DataManager.Test;

public class UnitTest1
{
    [Fact]
    public void Test1()
    {
        using var access = new DataAccess("data.db");
        var root = access.GetRoot();
    }
}
