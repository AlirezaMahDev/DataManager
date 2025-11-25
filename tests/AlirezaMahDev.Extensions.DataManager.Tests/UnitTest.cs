using Xunit.Abstractions;

namespace AlirezaMahDev.Extensions.DataManager.Tests;

public class UnitTest(ITestOutputHelper testOutputHelper)
{
    [Fact]
    public void TestAddItem()
    {
        using var access = new TempDataAccess();
        var locationWrap = access.GetRoot().Wrap(x => x.Collection());
        locationWrap.Clear();
        locationWrap.Add();
        access.Flush();
        testOutputHelper.WriteLine(locationWrap.GetChildren().Count().ToString());
        foreach (var dataLocation in locationWrap.GetChildren())
        {
            testOutputHelper.WriteLine(dataLocation.Offset.ToString());
        }
    }

    [Fact]
    public async Task TestAddItemAsync()
    {
        using var access = new TempDataAccess();
        var locationWrap = (await access.GetRootAsync()).Wrap(x => x.Collection());
        await locationWrap.ClearAsync();
        await locationWrap.AddAsync();
        await access.FlushAsync();
        testOutputHelper.WriteLine(locationWrap.GetChildren().Count().ToString());
        foreach (var dataLocation in locationWrap.GetChildren())
        {
            testOutputHelper.WriteLine(dataLocation.Offset.ToString());
        }
    }


    [Fact]
    public void TestAddManyItems()
    {
        using var access = new TempDataAccess();
        var locationWrap = access.GetRoot().Wrap(x => x.Collection());
        locationWrap.Clear();
        locationWrap.Add();
        locationWrap.Add();
        locationWrap.Add();
        locationWrap.Add();
        locationWrap.Add();
        access.Flush();
        testOutputHelper.WriteLine(locationWrap.GetChildren().Count().ToString());
        foreach (var dataLocation in locationWrap.GetChildren())
        {
            testOutputHelper.WriteLine(dataLocation.Offset.ToString());
        }
    }

    [Fact]
    public async Task TestAddManyItemsAsync()
    {
        using var access = new TempDataAccess();
        var locationWrap = (await access.GetRootAsync()).Wrap(x => x.Collection());
        await locationWrap.ClearAsync();
        await locationWrap.AddAsync();
        await locationWrap.AddAsync();
        await locationWrap.AddAsync();
        await locationWrap.AddAsync();
        await locationWrap.AddAsync();
        await access.FlushAsync();
        testOutputHelper.WriteLine(locationWrap.GetChildren().Count().ToString());
        foreach (var dataLocation in locationWrap.GetChildren())
        {
            testOutputHelper.WriteLine(dataLocation.Offset.ToString());
        }
    }

    [Fact]
    public void TestAddManyItemWithKeys()
    {
        using var access = new TempDataAccess();
        var locationWrap = access.GetRoot().Wrap(x => x.Dictionary());
        locationWrap.Clear();
        locationWrap.GetOrAdd("a1");
        locationWrap.GetOrAdd("a2");
        access.Flush();
        testOutputHelper.WriteLine(locationWrap.GetChildren().Count().ToString());
        foreach (var dataLocation in locationWrap.GetChildren())
        {
            testOutputHelper.WriteLine(dataLocation.Value.Key.ToString());
        }
    }

    [Fact]
    public async Task TestAddManyItemWithKeysASync()
    {
        using var access = new TempDataAccess();
        var locationWrap = (await access.GetRootAsync()).Wrap(x => x.Dictionary());
        await locationWrap.ClearAsync();
        await locationWrap.GetOrAddAsync("a1");
        await locationWrap.GetOrAddAsync("a2");
        await access.FlushAsync();
        testOutputHelper.WriteLine(locationWrap.GetChildren().Count().ToString());
        foreach (var dataLocation in locationWrap.GetChildren())
        {
            testOutputHelper.WriteLine(dataLocation.Value.Key.ToString());
        }
    }
}