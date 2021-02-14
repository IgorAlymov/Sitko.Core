using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Sitko.Core.Storage.Cache;
using Sitko.Core.Storage.Metadata;

namespace Sitko.Core.Storage.FileSystem
{
    public sealed class FileSystemStorage<T> : Storage<T> where T : StorageOptions, IFileSystemStorageOptions
    {
        public FileSystemStorage(T options, ILogger<FileSystemStorage<T>> logger, IStorageCache? cache = null,
            IStorageMetadataProvider? metadataProvider = null) : base(
            options, logger, cache, metadataProvider)
        {
        }

        protected override async Task<bool> DoSaveAsync(string path, Stream file,
            CancellationToken? cancellationToken = null)
        {
            var dirName = Path.GetDirectoryName(path);
            if (string.IsNullOrEmpty(dirName))
            {
                return false;
            }

            var dirPath = Path.Combine(Options.StoragePath, dirName);
            if (!Directory.Exists(dirPath))
            {
                Directory.CreateDirectory(dirPath);
            }

            var fullPath = Path.Combine(Options.StoragePath, path);
            await using var fileStream = File.Create(fullPath);
            file.Seek(0, SeekOrigin.Begin);
            await file.CopyToAsync(fileStream, cancellationToken ?? CancellationToken.None);
            return true;
        }

        protected override Task<bool> DoDeleteAsync(string filePath, CancellationToken? cancellationToken = null)
        {
            var path = Path.Combine(Options.StoragePath, filePath);
            if (File.Exists(path))
            {
                try
                {
                    File.Delete(path);
                    return Task.FromResult(true);
                }
                catch (Exception ex)
                {
                    Logger.LogError(ex, "Error while deleting file {File}: {ErrorText}", path, ex.ToString());
                }
            }

            return Task.FromResult(false);
        }

        protected override Task<bool> DoIsFileExistsAsync(StorageItem item, CancellationToken? cancellationToken = null)
        {
            var fullPath = Path.Combine(Options.StoragePath, item.FilePath);
            return Task.FromResult(File.Exists(fullPath));
        }

        protected override Task DoDeleteAllAsync(CancellationToken? cancellationToken = null)
        {
            if (Directory.Exists(Options.StoragePath))
            {
                Directory.Delete(Options.StoragePath, true);
            }

            return Task.CompletedTask;
        }

        internal override Task<StorageItemDownloadInfo?> DoGetFileAsync(string path,
            CancellationToken? cancellationToken = null)
        {
            StorageItemDownloadInfo? result = null;
            var fullPath = Path.Combine(Options.StoragePath, path);
            var fileInfo = new FileInfo(fullPath);

            if (fileInfo.Exists)
            {
                result = new StorageItemDownloadInfo(fileInfo.Length, fileInfo.LastWriteTimeUtc,
                    () => new FileStream(fullPath, FileMode.Open));
            }

            return Task.FromResult(result);
        }

        internal override Task<IEnumerable<StorageItemInfo>> GetAllItemsAsync(string path,
            CancellationToken? cancellationToken = null)
        {
            var items = new List<StorageItemInfo>();
            ListFolder(items, string.IsNullOrEmpty(Options.Prefix) ? "/" : Options.Prefix);
            return Task.FromResult<IEnumerable<StorageItemInfo>>(items);
        }

        private void ListFolder(List<StorageItemInfo> items, string path)
        {
            var fullPath = path == "/" ? Options.StoragePath : Path.Combine(Options.StoragePath, path.Trim('/'));
            if (Directory.Exists(fullPath))
            {
                foreach (var info in new DirectoryInfo(fullPath)
                    .EnumerateFileSystemInfos())
                {
                    if (info is DirectoryInfo dir)
                    {
                        ListFolder(items, Helpers.PreparePath(Path.Combine(path, dir.Name))!);
                    }

                    if (info is FileInfo file)
                    {
                        var item = new StorageItemInfo(Helpers.PreparePath(Path.Combine(path, file.Name))!.Trim('/'),
                            file.Length, file.LastWriteTimeUtc);
                        items.Add(item);
                    }
                }
            }
        }
    }
}
