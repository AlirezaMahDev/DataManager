using Xunit.Abstractions;

namespace AlirezaMahDev.Extensions.DataManager.Tests;

public class UnitTest(ITestOutputHelper testOutputHelper)
{
    [Fact]
    public void TestAddItem()
    {
        using (var access = new DataAccess("data.db"))
        {
            var locationWrap = access.GetRoot().Wrap(x => x.Collection());
            locationWrap.Clear();
            locationWrap.Add();
            access.Save();
        }

        using (var access = new DataAccess("data.db"))
        {
            var locationWrap = access.GetRoot().Wrap(x => x.Collection());
            testOutputHelper.WriteLine(locationWrap.GetChildren().Count().ToString());
        }
    }

    [Fact]
    public async Task TestAddItemAsync()
    {
        using (var access = new DataAccess("data.db"))
        {
            var locationWrap = (await access.GetRootAsync()).Wrap(x => x.Collection());
            await locationWrap.ClearAsync();
            var item1 = await locationWrap.AddAsync();
            await access.SaveAsync();
        }

        using (var access = new DataAccess("data.db"))
        {
            var locationWrap = (await access.GetRootAsync()).Wrap(x => x.Collection());
            testOutputHelper.WriteLine(locationWrap.GetChildren().Count().ToString());
        }
    }


    [Fact]
    public void TestAddManyItems()
    {
        using (var access = new DataAccess("data.db"))
        {
            var locationWrap = access.GetRoot().Wrap(x => x.Collection());
            locationWrap.Clear();
            locationWrap.Add();
            locationWrap.Add();
            locationWrap.Add();
            locationWrap.Add();
            locationWrap.Add();
            access.Save();
        }

        using (var access = new DataAccess("data.db"))
        {
            var locationWrap = access.GetRoot().Wrap(x => x.Collection());
            testOutputHelper.WriteLine(locationWrap.GetChildren().Count().ToString());
        }
    }

    [Fact]
    public async Task TestAddManyItemsAsync()
    {
        using (var access = new DataAccess("data.db"))
        {
            var locationWrap = (await access.GetRootAsync()).Wrap(x => x.Collection());
            await locationWrap.ClearAsync();
            await locationWrap.AddAsync();
            await locationWrap.AddAsync();
            await locationWrap.AddAsync();
            await locationWrap.AddAsync();
            await locationWrap.AddAsync();
            await access.SaveAsync();
        }

        using (var access = new DataAccess("data.db"))
        {
            var locationWrap = (await access.GetRootAsync()).Wrap(x => x.Collection());
            testOutputHelper.WriteLine(locationWrap.GetChildren().Count().ToString());
        }
    }

    [Fact]
    public void TestAddManyItemWithKeys()
    {
        using (var access = new DataAccess("data.db"))
        {
            var locationWrap = access.GetRoot().Wrap(x => x.Dictionary());
            locationWrap.Clear();
            locationWrap.GetOrAdd("a1");
            locationWrap.GetOrAdd("a2");
            access.Save();
        }

        using (var access = new DataAccess("data.db"))
        {
            var locationWrap = access.GetRoot().Wrap(x => x.Dictionary());
            testOutputHelper.WriteLine(locationWrap.GetChildren().Count().ToString());
        }
    }

    [Fact]
    public async Task TestAddManyItemWithKeysASync()
    {
        using (var access = new DataAccess("data.db"))
        {
            var locationWrap = (await access.GetRootAsync()).Wrap(x => x.Dictionary());
            await locationWrap.ClearAsync();
            await locationWrap.GetOrAddAsync("a1");
            await locationWrap.GetOrAddAsync("a2");
            await access.SaveAsync();
        }

        using (var access = new DataAccess("data.db"))
        {
            var locationWrap = (await access.GetRootAsync()).Wrap(x => x.Dictionary());
            testOutputHelper.WriteLine(locationWrap.GetChildren().Count().ToString());
        }
    }
}