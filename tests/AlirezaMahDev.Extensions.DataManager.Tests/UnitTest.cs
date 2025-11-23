using Xunit.Abstractions;

namespace AlirezaMahDev.Extensions.DataManager.Tests;

public class UnitTest(ITestOutputHelper testOutputHelper)
{
    [Fact]
    public void TestAddItem()
    {
        using (var access = new DataAccess("data.db"))
        {
            var root = access.GetRoot();
            var item1 = root.Add();
            access.Save();
        }

        using (var access = new DataAccess("data.db"))
        {
            testOutputHelper.WriteLine(access.GetRoot().GetChildren().Count().ToString());
        }
    }
    
    
    [Fact]
    public void TestAddManyItemWithKeys()
    {
        using (var access = new DataAccess("data.db"))
        {
            var root = access.GetRoot();
            var item1 = root.Add();
            access.Save();
        }

        using (var access = new DataAccess("data.db"))
        {
            testOutputHelper.WriteLine(access.GetRoot().GetChildren().Count().ToString());
        }
    }
}