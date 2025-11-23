using System.Runtime.CompilerServices;

namespace AlirezaMahDev.Extensions.DataManager;

static class DataLocationExtensions
{
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

    extension<TValue>(IEnumerable<DataLocation<TValue>> enumerable)
        where TValue : unmanaged, IDataCollection, IDataValue<TValue>
    {
    }

    extension<TValue>(IAsyncEnumerable<DataLocation<TValue>> asyncEnumerable)
        where TValue : unmanaged, IDataCollection, IDataValue<TValue>
    {
    }

    extension<TValue>(DataLocation<TValue> location)
        where TValue : unmanaged, IDataCollection, IDataValue<TValue>
    {
        public DataLocation<TValue>? GetChild() =>
            location.Value.Child == -1 ? null : location.Access.Read<TValue>(location.Value.Child);

        public async Task<DataLocation<TValue>?> GetChildAsync(CancellationToken cancellationToken = default) =>
            location.Value.Child == -1
                ? null
                : await location.Access.ReadAsync<TValue>(location.Value.Child, cancellationToken);

        public DataLocation<TValue>? GetNext() =>
            location.Value.Next == -1 ? null : location.Access.Read<TValue>(location.Value.Next);

        public async Task<DataLocation<TValue>?> GetNextAsync(CancellationToken cancellationToken = default) =>
            location.Value.Next == -1
                ? null
                : await location.Access.ReadAsync<TValue>(location.Value.Next, cancellationToken);

        public IEnumerable<DataLocation<TValue>> GetChildren()
        {
            var current = location.GetChild();
            while (current.HasValue)
            {
                yield return current.Value;
                current = current.Value.GetNext();
            }
        }

        public async IAsyncEnumerable<DataLocation<TValue>> GetChildrenAsync(
            [EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            var current = await location.GetChildAsync(cancellationToken);
            while (current.HasValue)
            {
                yield return current.Value;
                current = await current.Value.GetNextAsync(cancellationToken);
            }
        }

        public DataLocation<TValue> Add()
        {
            var dataLocation = location.Access.Create<TValue>();
            return location.Add(dataLocation);
        }

        public async ValueTask<DataLocation<TValue>> AddAsync(CancellationToken cancellationToken = default)
        {
            var dataLocation = await location.Access.CreateAsync<TValue>(cancellationToken);
            return location.Add(dataLocation);
        }

        public DataLocation<TValue> Add(DataLocation<TValue> dataLocation)
        {
            location.Update(value =>
            {
                dataLocation.Update(innerValue => innerValue with { Next = value.Child });
                return value with { Child = dataLocation.Offset };
            });
            return dataLocation;
        }

        public DataLocation<TValue>? Remove(DataLocation<TValue> dataLocation) =>
            location.Remove(dataLocation.Offset);

        public async ValueTask<DataLocation<TValue>?> RemoveAsync(DataLocation<TValue> dataLocation,
            CancellationToken cancellationToken = default) =>
            await location.RemoveAsync(dataLocation.Offset, cancellationToken);

        public DataLocation<TValue>? Remove(long offset)
        {
            DataLocation<TValue>? previous = null;
            foreach (var dataLocation in location.GetChildren())
            {
                if (dataLocation.Offset == offset)
                {
                    if (previous.HasValue)
                    {
                        previous.Value.Update(value => value with { Next = dataLocation.Value.Next });
                    }
                    else
                    {
                        location.Update(value => value with { Child = dataLocation.Value.Next });
                    }

                    return dataLocation;
                }

                previous = dataLocation;
            }

            return null;
        }

        public async ValueTask<DataLocation<TValue>?> RemoveAsync(long offset,
            CancellationToken cancellationToken = default)
        {
            DataLocation<TValue>? previous = null;
            await foreach (var dataLocation in location.GetChildrenAsync(cancellationToken: cancellationToken))
            {
                if (dataLocation.Offset == offset)
                {
                    if (previous.HasValue)
                    {
                        previous.Value.Update(value => value with { Next = dataLocation.Value.Next });
                    }
                    else
                    {
                        location.Update(value => value with { Child = dataLocation.Value.Next });
                    }

                    return dataLocation;
                }

                previous = dataLocation;
            }

            return null;
        }
    }

    extension<TKey, TValue>(DataLocation<TValue> location)
        where TKey : unmanaged, IEquatable<TKey>
        where TValue : unmanaged, IDataDictionary<TKey>, IDataValue<TValue>
    {
        public DataLocation<TValue>? TryGet(TKey key)
        {
            return location.GetChildren()
                .FirstOrDefault(x => x.Value.Key.Equals(key))
                .NullWhenDefault();
        }

        public async ValueTask<DataLocation<TValue>?> TryGetAsync(TKey key,
            CancellationToken cancellationToken = default)
        {
            var dataLocation = await location.GetChildrenAsync(cancellationToken)
                .FirstOrDefaultAsync(x => x.Value.Key.Equals(key), cancellationToken);
            return dataLocation.NullWhenDefault();
        }

        public DataLocation<TValue> GetOrAdd(TKey key)
        {
            var dataLocation = location.GetChildren()
                .FirstOrDefault(x => x.Value.Key.Equals(key))
                .WhenDefault(() => location.Access.Create<TValue>(value => value with { Key = key }));
            return location.Add(dataLocation);
        }

        public async ValueTask<DataLocation<TValue>> GetOrAddAsync(TKey key,
            CancellationToken cancellationToken = default)
        {
            var dataLocation = await location.GetChildrenAsync(cancellationToken)
                .FirstOrDefaultAsync(x => x.Value.Key.Equals(key), cancellationToken);
            dataLocation = await dataLocation.WhenDefaultAsync(async token =>
                    await location.Access.CreateAsync<TValue>(value => value with { Key = key }, token),
                cancellationToken: cancellationToken);
            return location.Add(dataLocation);
        }

        public DataLocation<TValue>? Remove(TKey key)
        {
            return location.GetChildren()
                .FirstOrDefault(x => x.Value.Key.Equals(key))
                .WhenNotDefault(x => location.Remove(x));
        }

        public async ValueTask<DataLocation<TValue>?> RemoveAsync(TKey key,
            CancellationToken cancellationToken = default)
        {
            var dataLocation = await location.GetChildrenAsync(cancellationToken: cancellationToken)
                .FirstOrDefaultAsync(x => x.Value.Key.Equals(key), cancellationToken: cancellationToken);
            return await dataLocation.WhenNotDefaultAsync(
                async (x, token) => await location.RemoveAsync(x, token),
                cancellationToken);
        }
    }
}