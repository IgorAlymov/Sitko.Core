using System;
using System.IO;
using System.Threading.Tasks;

namespace Sitko.Core.Storage.Cache
{
    public interface IStorageCache : IAsyncDisposable
    {
        internal Task<StorageItemInfo?> GetItemAsync(string path);

        internal Task<StorageItemInfo?> GetOrAddItemAsync(string path, Func<Task<StorageItemInfo?>> addItem);

        Task RemoveItemAsync(string path);
        Task ClearAsync();
    }

    public interface IStorageCache<T> : IStorageCache where T : StorageCacheOptions
    {
    }

    public interface IStorageCacheRecord
    {
        string? Metadata { get; }

        long FileSize { get; }
        DateTimeOffset Date { get; }

        public Stream OpenRead();
    }

    public abstract class StorageCacheOptions
    {
        public TimeSpan Ttl { get; set; } = TimeSpan.FromHours(12);
        public long MaxFileSizeToStore { get; set; }
        public long? MaxCacheSize { get; set; }
    }
}