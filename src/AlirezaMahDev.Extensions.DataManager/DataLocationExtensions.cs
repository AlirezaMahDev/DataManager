using System.Linq.Expressions;
using System.Runtime.CompilerServices;

namespace AlirezaMahDev.Extensions.DataManager;

static class DataLocationExtensions
{
    extension(DataLocation<DataTrash> location)
    {
        public void Add<TDataLocation>(TDataLocation dataLocation)
            where TDataLocation : IDataLocationBase
        {
            dataLocation.Access.GetTrash()
                .Wrap(x => x.Collection())
                .Add(x => x with
                {
                    Offset = dataLocation.Offset,
                    Length = dataLocation.Length
                });
        }

        public async ValueTask AddAsync<TDataLocation>(TDataLocation dataLocation,
            CancellationToken cancellationToken = default)
            where TDataLocation : IDataLocationBase
        {
            await (await dataLocation.Access.GetTrashAsync(cancellationToken))
                .Wrap(x => x.Collection())
                .AddAsync(x => x with
                    {
                        Offset = dataLocation.Offset,
                        Length = dataLocation.Length
                    },
                    cancellationToken);
        }
    }

    extension<TValue>(DataLocation<TValue> location)
        where TValue : unmanaged, IDataValue<TValue>
    {
        public bool IsDefault => location.Value.Equals(default);

        public DataLocation<TValue> WhenDefault(Func<DataLocation<TValue>> func) =>
            location.IsDefault ? func() : location;

        public async ValueTask<DataLocation<TValue>> WhenDefaultAsync(
            Func<CancellationToken, ValueTask<DataLocation<TValue>>> func,
            CancellationToken cancellationToken = default) =>
            location.IsDefault ? await func(cancellationToken) : location;

        public TResult? WhenNotDefault<TResult>(Func<DataLocation<TValue>, TResult> func) =>
            location.IsDefault ? func(location) : default;

        public async ValueTask<TResult?> WhenNotDefaultAsync<TResult>(
            Func<DataLocation<TValue>, CancellationToken, ValueTask<TResult?>> func,
            CancellationToken cancellationToken = default) =>
            location.IsDefault ? await func(location, cancellationToken) : default;


        public DataLocation<TValue>? NullWhenDefault() =>
            location.IsDefault ? null : location;
    }

    extension<TValue, TWrap>(DataLocation<TValue> location)
        where TValue : unmanaged, IDataValue<TValue>
    {
        public DataLocationWrap<TValue, TWrap> Wrap(Expression<Func<TValue, TWrap>> expression) => new(location);
    }

    extension<TValue, TWrap>(DataLocationWrap<TValue, TWrap> locationWrap)
        where TValue : unmanaged, IDataValue<TValue>
    {
        public DataLocationWrap<TValue, TInnerWrap> Wrap<TInnerWrap>(Expression<Func<TValue, TInnerWrap>> expression) =>
            locationWrap.Location.Wrap(expression);
    }

    extension<TValue, TItem>(IDataCollection<TValue, TItem> value)
        where TValue : unmanaged, IDataCollection<TValue, TItem>
        where TItem : unmanaged, IDataCollectionItem<TItem>
    {
        public IDataCollection<TValue, TItem> Collection() => null!;
    }

    extension<TValue>(IDataCollectionTree<TValue> value)
        where TValue : unmanaged, IDataCollectionTree<TValue>
    {
        public IDataCollection<TValue, TValue> TreeCollection() => null!;
    }

    extension<TValue, TItem, TKey>(IDataDictionary<TValue, TItem, TKey> value)
        where TValue : unmanaged, IDataDictionary<TValue, TItem, TKey>
        where TItem : unmanaged, IDataDictionaryItem<TItem, TKey>
        where TKey : unmanaged, IEquatable<TKey>
    {
        public IDataDictionary<TValue, TItem, TKey> Dictionary() => null!;
    }


    extension<TValue, TKey>(IDataDictionaryTree<TValue, TKey> value)
        where TValue : unmanaged, IDataDictionaryTree<TValue, TKey>
        where TKey : unmanaged, IEquatable<TKey>
    {
        public IDataDictionary<TValue, TValue, TKey> TreeDictionary() => null!;
    }

    extension<TValue>(IDataStorage<TValue> value)
        where TValue : unmanaged, IDataStorage<TValue>
    {
        public IDataStorage<TValue> Storage() => null!;
    }


    extension<TValue>(IDataCollectionItem<TValue> value)
        where TValue : unmanaged, IDataCollectionItem<TValue>
    {
        public IDataCollectionItem<TValue> CollectionItem() => null!;
    }

    extension<TValue>(DataLocationWrap<TValue, IDataStorage<TValue>> locationWrap)
        where TValue : unmanaged, IDataStorage<TValue>
    {
        public DataLocation? TryGetData()
        {
            return locationWrap.Location.Value.Data == -1
                ? null
                : locationWrap.Location.Access.Read(locationWrap.Location.Value.Data, locationWrap.Location.Value.Size);
        }

        public async ValueTask<DataLocation?> TryGetDataAsync(CancellationToken cancellationToken = default)
        {
            return locationWrap.Location.Value.Data == -1
                ? null
                : await locationWrap.Location.Access.ReadAsync(
                    locationWrap.Location.Value.Data,
                    locationWrap.Location.Value.Size,
                    cancellationToken);
        }

        public DataLocation<TDataValue>? TryGetData<TDataValue>()
            where TDataValue : unmanaged, IDataValue<TDataValue>
        {
            return locationWrap.Location.Value.Data == -1
                ? null
                : DataLocation<TDataValue>.Size == locationWrap.Location.Value.Size
                    ? locationWrap.Location.Access.Read<TDataValue>(locationWrap.Location.Value.Data)
                    : throw new InvalidDataException($"{typeof(TDataValue).Name} size must be {
                        locationWrap.Location.Value.Size}");
        }

        public async ValueTask<DataLocation<TDataValue>?> TryGetDataAsync<TDataValue>(
            CancellationToken cancellationToken =
                default)
            where TDataValue : unmanaged, IDataValue<TDataValue>
        {
            return locationWrap.Location.Value.Data == -1
                ? null
                : DataLocation<TDataValue>.Size == locationWrap.Location.Value.Size
                    ? await locationWrap.Location.Access.ReadAsync<TDataValue>(locationWrap.Location.Value.Data,
                        cancellationToken)
                    : throw new InvalidDataException($"{typeof(TDataValue).Name} size must be {
                        locationWrap.Location.Value.Size}");
        }

        public DataLocation CreateNewData(int length)
        {
            var dataLocation = locationWrap.TryGetData();
            dataLocation?.Access.GetTrash().Add(dataLocation.Value);
            return locationWrap.Location.Access.Create(length);
        }

        public async ValueTask<DataLocation> CreateNewDataAsync(int length,
            CancellationToken cancellationToken = default)
        {
            var dataLocation = await locationWrap.TryGetDataAsync(cancellationToken);
            if (dataLocation.HasValue)
                await (await dataLocation.Value.Access.GetTrashAsync(cancellationToken))
                    .AddAsync(dataLocation.Value, cancellationToken);
            return await locationWrap.Location.Access.CreateAsync(length, cancellationToken: cancellationToken);
        }

        public DataLocation<TDataValue> CreateNewData<TDataValue>()
            where TDataValue : unmanaged, IDataValue<TDataValue>, IDataValueDefault<TDataValue>
        {
            var dataLocation = locationWrap.TryGetData();
            dataLocation?.Access.GetTrash().Add(dataLocation.Value);
            return locationWrap.Location.Access.Create<TDataValue>();
        }

        public async ValueTask<DataLocation<TDataValue>> CreateNewDataAsync<TDataValue>(
            CancellationToken cancellationToken = default)
            where TDataValue : unmanaged, IDataValue<TDataValue>, IDataValueDefault<TDataValue>
        {
            var dataLocation = await locationWrap.TryGetDataAsync(cancellationToken);
            if (dataLocation.HasValue)
                await (await dataLocation.Value.Access.GetTrashAsync(cancellationToken))
                    .AddAsync(dataLocation.Value, cancellationToken);
            return await locationWrap.Location.Access.CreateAsync<TDataValue>(cancellationToken: cancellationToken);
        }

        public DataLocation GetOrCreateData(int length)
        {
            var dataLocation = locationWrap.TryGetData();
            return dataLocation.HasValue
                ? dataLocation.Value.Length == length
                    ? dataLocation.Value
                    : throw new InvalidDataException(
                        $"data length is {dataLocation.Value.Length} but you need {length}")
                : locationWrap.CreateNewData(length);
        }

        public async ValueTask<DataLocation> GetOrCreateDataAsync(int length,
            CancellationToken cancellationToken = default)
        {
            var dataLocation = await locationWrap.TryGetDataAsync(cancellationToken);
            return dataLocation.HasValue
                ? dataLocation.Value.Length == length
                    ? dataLocation.Value
                    : throw new InvalidDataException(
                        $"data length is {dataLocation.Value.Length} but you need {length}")
                : await locationWrap.CreateNewDataAsync(length, cancellationToken);
        }

        public DataLocation<TDataValue> GetOrCreateData<TDataValue>()
            where TDataValue : unmanaged, IDataValue<TDataValue>, IDataValueDefault<TDataValue>
        {
            var dataLocation = locationWrap.TryGetData<TValue, TDataValue>();
            return dataLocation.HasValue
                ? dataLocation.Value.Length == DataLocation<TDataValue>.Size
                    ? dataLocation.Value
                    : throw new InvalidDataException(
                        $"data length is {dataLocation.Value.Length} but you need {DataLocation<TDataValue>.Size}")
                : locationWrap.CreateNewData<TValue, TDataValue>();
        }

        public async ValueTask<DataLocation<TDataValue>> GetOrCreateDataAsync<TDataValue>(
            CancellationToken cancellationToken = default)
            where TDataValue : unmanaged, IDataValue<TDataValue>, IDataValueDefault<TDataValue>
        {
            var dataLocation = await locationWrap.TryGetDataAsync<TValue, TDataValue>(cancellationToken);
            return dataLocation.HasValue
                ? dataLocation.Value.Length == DataLocation<TDataValue>.Size
                    ? dataLocation.Value
                    : throw new InvalidDataException(
                        $"data length is {dataLocation.Value.Length} but you need {DataLocation<TDataValue>.Size}")
                : await locationWrap.CreateNewDataAsync<TValue, TDataValue>(cancellationToken);
        }


        public DataLocation GetOrCreateNewData(int length)
        {
            var dataLocation = locationWrap.TryGetData();
            return dataLocation.HasValue && dataLocation.Value.Length == length
                ? dataLocation.Value
                : locationWrap.CreateNewData(length);
        }

        public async ValueTask<DataLocation> GetOrCreateDataNewAsync(int length,
            CancellationToken cancellationToken = default)
        {
            var dataLocation = await locationWrap.TryGetDataAsync(cancellationToken);
            return dataLocation.HasValue && dataLocation.Value.Length == length
                ? dataLocation.Value
                : await locationWrap.CreateNewDataAsync(length, cancellationToken);
        }

        public DataLocation<TDataValue> GetOrCreateNewData<TDataValue>()
            where TDataValue : unmanaged, IDataValue<TDataValue>, IDataValueDefault<TDataValue>
        {
            var dataLocation = locationWrap.TryGetData<TValue, TDataValue>();
            return dataLocation.HasValue && dataLocation.Value.Length == DataLocation<TDataValue>.Size
                ? dataLocation.Value
                : locationWrap.CreateNewData<TValue, TDataValue>();
        }

        public async ValueTask<DataLocation<TDataValue>> GetOrCreateNewDataAsync<TDataValue>(
            CancellationToken cancellationToken = default)
            where TDataValue : unmanaged, IDataValue<TDataValue>, IDataValueDefault<TDataValue>
        {
            var dataLocation = await locationWrap.TryGetDataAsync<TValue, TDataValue>(cancellationToken);
            return dataLocation.HasValue && dataLocation.Value.Length == DataLocation<TDataValue>.Size
                ? dataLocation.Value
                : await locationWrap.CreateNewDataAsync<TValue, TDataValue>(cancellationToken);
        }
    }

    extension<TValue, TItem>(DataLocationWrap<TValue, IDataCollection<TValue, TItem>> locationWrap)
        where TValue : unmanaged, IDataCollection<TValue, TItem>
        where TItem : unmanaged, IDataCollectionItem<TItem>, IDataValueDefault<TItem>
    {
        public DataLocation<TItem> Add()
        {
            var dataLocation = locationWrap.Location.Access.Create<TItem>();
            return locationWrap.Add(dataLocation);
        }

        public DataLocation<TItem> Add(Func<TItem, TItem> func)
        {
            var dataLocation = locationWrap.Location.Access.Create(func);
            return locationWrap.Add(dataLocation);
        }

        public async ValueTask<DataLocation<TItem>> AddAsync(CancellationToken cancellationToken = default)
        {
            var dataLocation = await locationWrap.Location.Access.CreateAsync<TItem>(cancellationToken);
            return locationWrap.Add(dataLocation);
        }

        public async ValueTask<DataLocation<TItem>> AddAsync(Func<TItem, TItem> func,
            CancellationToken cancellationToken = default)
        {
            var dataLocation = await locationWrap.Location.Access.CreateAsync(func, cancellationToken);
            return locationWrap.Add(dataLocation);
        }

        public async ValueTask<DataLocation<TItem>> AddAsync(Func<TItem, CancellationToken, ValueTask<TItem>> func,
            CancellationToken cancellationToken = default)
        {
            var dataLocation = await locationWrap.Location.Access.CreateAsync(func, cancellationToken);
            return locationWrap.Add(dataLocation);
        }
    }

    extension<TValue>(DataLocationWrap<TValue, IDataCollectionItem<TValue>> locationWrap)
        where TValue : unmanaged, IDataCollectionItem<TValue>, IDataValue<TValue>
    {
        public DataLocation<TValue>? GetNext() =>
            locationWrap.Location.Value.Next == -1
                ? null
                : locationWrap.Location.Access.Read<TValue>(locationWrap.Location.Value.Next);

        public async Task<DataLocation<TValue>?> GetNextAsync(CancellationToken cancellationToken = default) =>
            locationWrap.Location.Value.Next == -1
                ? null
                : await locationWrap.Location.Access.ReadAsync<TValue>(locationWrap.Location.Value.Next,
                    cancellationToken);
    }

    extension<TValue, TItem>(DataLocationWrap<TValue, IDataCollection<TValue, TItem>> locationWrap)
        where TValue : unmanaged, IDataCollection<TValue, TItem>
        where TItem : unmanaged, IDataCollectionItem<TItem>
    {
        public DataLocation<TItem>? GetChild() =>
            locationWrap.Location.Value.Child == -1
                ? null
                : locationWrap.Location.Access.Read<TItem>(locationWrap.Location.Value.Child);

        public async Task<DataLocation<TItem>?> GetChildAsync(CancellationToken cancellationToken = default) =>
            locationWrap.Location.Value.Child == -1
                ? null
                : await locationWrap.Location.Access.ReadAsync<TItem>(locationWrap.Location.Value.Child,
                    cancellationToken);

        public IEnumerable<DataLocation<TItem>> GetChildren()
        {
            var current = locationWrap.GetChild();
            while (current.HasValue)
            {
                yield return current.Value;
                current = current.Value.Wrap(x => x.CollectionItem()).GetNext();
            }
        }

        public async IAsyncEnumerable<DataLocation<TItem>> GetChildrenAsync(
            [EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            var current = await locationWrap.GetChildAsync(cancellationToken);
            while (current.HasValue)
            {
                yield return current.Value;
                current = await current.Value.Wrap(x => x.CollectionItem()).GetNextAsync(cancellationToken);
            }
        }

        public DataLocation<TItem> Add(DataLocation<TItem> dataLocation)
        {
            locationWrap.Location.Update(value =>
            {
                dataLocation.Update(innerValue => innerValue with { Next = value.Child });
                return value with { Child = dataLocation.Offset };
            });
            return dataLocation;
        }

        public DataLocation<TItem>? Remove(long offset)
        {
            DataLocation<TItem>? previous = null;
            foreach (var dataLocation in locationWrap.GetChildren())
            {
                if (dataLocation.Offset == offset)
                {
                    if (previous.HasValue)
                    {
                        previous.Value.Update(value => value with { Next = dataLocation.Value.Next });
                    }
                    else
                    {
                        locationWrap.Location.Update(value => value with { Child = dataLocation.Value.Next });
                    }

                    locationWrap.Location.Access.GetTrash().Add(dataLocation);
                    return dataLocation;
                }

                previous = dataLocation;
            }

            return null;
        }

        public async ValueTask<DataLocation<TItem>?> RemoveAsync(long offset,
            CancellationToken cancellationToken = default)
        {
            DataLocation<TItem>? previous = null;
            await foreach (var dataLocation in locationWrap.GetChildrenAsync(
                               cancellationToken: cancellationToken))
            {
                if (dataLocation.Offset == offset)
                {
                    if (previous.HasValue)
                    {
                        previous.Value.Update(value => value with { Next = dataLocation.Value.Next });
                    }
                    else
                    {
                        locationWrap.Location.Update(value => value with { Child = dataLocation.Value.Next });
                    }

                    await (await locationWrap.Location.Access.GetTrashAsync(cancellationToken))
                        .AddAsync(dataLocation, cancellationToken);
                    return dataLocation;
                }

                previous = dataLocation;
            }

            return null;
        }

        public DataLocation<TItem>? Remove(DataLocation<TItem> dataLocation) =>
            locationWrap.Remove(dataLocation.Offset);

        public async ValueTask<DataLocation<TItem>?> RemoveAsync(DataLocation<TItem> dataLocation,
            CancellationToken cancellationToken = default) =>
            await locationWrap.RemoveAsync(dataLocation.Offset, cancellationToken);


        public void Clear()
        {
            foreach (var dataLocation in locationWrap.GetChildren())
            {
                locationWrap.Remove(dataLocation);
            }
        }

        public async ValueTask ClearAsync(CancellationToken cancellationToken = default)
        {
            foreach (var dataLocation in locationWrap.GetChildren())
            {
                await locationWrap.RemoveAsync(dataLocation, cancellationToken);
            }
        }
    }

    extension<TValue, TItem, TKey>(DataLocationWrap<TValue, IDataDictionary<TValue, TItem, TKey>> locationWrap)
        where TValue : unmanaged, IDataDictionary<TValue, TItem, TKey>
        where TItem : unmanaged, IDataDictionaryItem<TItem, TKey>, IDataValueDefault<TItem>
        where TKey : unmanaged, IEquatable<TKey>
    {
        public DataLocation<TItem> GetOrAdd(TKey key)
        {
            var locationWrapCollection = locationWrap
                .Wrap(x => x.Collection());
            var dataLocation = locationWrapCollection
                .GetChildren()
                .FirstOrDefault(x => x.Value.Key.Equals(key))
                .WhenDefault(() => locationWrap.Location.Access.Create<TItem>(value => value with { Key = key }));
            return locationWrapCollection.Add(dataLocation);
        }

        public async ValueTask<DataLocation<TItem>> GetOrAddAsync(TKey key,
            CancellationToken cancellationToken = default)
        {
            var locationWrapCollection = locationWrap
                .Wrap(x => x.Collection());
            var dataLocation = await locationWrapCollection
                .GetChildrenAsync(cancellationToken)
                .FirstOrDefaultAsync(x => x.Value.Key.Equals(key), cancellationToken);
            dataLocation = await dataLocation.WhenDefaultAsync(async token =>
                    await locationWrap.Location.Access.CreateAsync<TItem>(value => value with { Key = key }, token),
                cancellationToken: cancellationToken);
            return locationWrapCollection.Add(dataLocation);
        }
    }

    extension<TValue, TItem, TKey>(DataLocationWrap<TValue, IDataDictionary<TValue, TItem, TKey>> locationWrap)
        where TValue : unmanaged, IDataDictionary<TValue, TItem, TKey>
        where TItem : unmanaged, IDataDictionaryItem<TItem, TKey>
        where TKey : unmanaged, IEquatable<TKey>
    {
        public DataLocation<TItem>? TryGet(TKey key)
        {
            return locationWrap
                .Wrap(x => x.Collection())
                .GetChildren()
                .FirstOrDefault(x => x.Value.Key.Equals(key))
                .NullWhenDefault();
        }

        public async ValueTask<DataLocation<TItem>?> TryGetAsync(TKey key,
            CancellationToken cancellationToken = default)
        {
            var dataLocation = await locationWrap
                .Wrap(x => x.Collection())
                .GetChildrenAsync(cancellationToken)
                .FirstOrDefaultAsync(x => x.Value.Key.Equals(key), cancellationToken);
            return dataLocation.NullWhenDefault();
        }

        public void Clear()
        {
            locationWrap
                .Wrap(x => x.Collection())
                .Clear();
        }

        public async ValueTask ClearAsync(CancellationToken cancellationToken = default)
        {
            await locationWrap
                .Wrap(x => x.Collection())
                .ClearAsync(cancellationToken);
        }

        public DataLocation<TItem>? Remove(TKey key)
        {
            var locationWrapCollection = locationWrap
                .Wrap(x => x.Collection());
            return locationWrapCollection
                .GetChildren()
                .FirstOrDefault(x => x.Value.Key.Equals(key))
                .WhenNotDefault(x => locationWrapCollection.Remove(x));
        }

        public async ValueTask<DataLocation<TItem>?> RemoveAsync(TKey key,
            CancellationToken cancellationToken = default)
        {
            var locationWrapCollection = locationWrap
                .Wrap(x => x.Collection());
            var dataLocation = await locationWrapCollection
                .GetChildrenAsync(cancellationToken: cancellationToken)
                .FirstOrDefaultAsync(x => x.Value.Key.Equals(key), cancellationToken: cancellationToken);
            return await dataLocation.WhenNotDefaultAsync(
                async (x, token) => await locationWrapCollection.RemoveAsync(x, token),
                cancellationToken);
        }


        public IEnumerable<DataLocation<TItem>> GetChildren()
        {
            return locationWrap
                .Wrap(x => x.Collection())
                .GetChildren();
        }

        public IAsyncEnumerable<DataLocation<TItem>> GetChildrenAsync(CancellationToken cancellationToken = default)
        {
            return locationWrap
                .Wrap(x => x.Collection())
                .GetChildrenAsync(cancellationToken: cancellationToken);
        }
    }
}