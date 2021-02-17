﻿using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Sitko.Core.Storage.Metadata.Postgres.DB;
using Sitko.Core.Storage.Metadata.Postgres.DB.Models;

namespace Sitko.Core.Storage.Metadata.Postgres
{
    public class
        PostgresStorageMetadataProvider<TStorageOptions> : BaseStorageMetadataProvider<
            PostgresStorageMetadataProviderOptions, TStorageOptions> where TStorageOptions : StorageOptions
    {
        public PostgresStorageMetadataProvider(PostgresStorageMetadataProviderOptions options,
            TStorageOptions storageOptions,
            ILogger<PostgresStorageMetadataProvider<TStorageOptions>> logger) : base(options, storageOptions, logger)
        {
        }

        private StorageDbContext GetDbContext()
        {
            return new(Options.ConnectionString, Options.Schema);
        }

        private Task<StorageItemRecord?> GetItemRecordAsync(StorageDbContext dbContext, string filePath,
            CancellationToken? cancellationToken)
        {
            return dbContext.Records.FirstOrDefaultAsync(r =>
                    r.Storage == StorageOptions.Name && r.FilePath == filePath,
                cancellationToken ?? CancellationToken.None);
        }

        protected override async Task DoDeleteMetadataAsync(string filePath, CancellationToken? cancellationToken)
        {
            await using var dbContext = GetDbContext();
            var record = await GetItemRecordAsync(dbContext, filePath, cancellationToken);
            if (record is not null)
            {
                dbContext.Records.Remove(record);
                await dbContext.SaveChangesAsync(cancellationToken ?? CancellationToken.None);
            }
        }

        protected override async Task DoDeleteAllMetadataAsync(CancellationToken? cancellationToken)
        {
            await using var dbContext = GetDbContext();
            await dbContext.Database.ExecuteSqlRawAsync(
                $"DELETE FROM \"StorageItemRecords\" WHERE \"Storage\" = '{StorageOptions.Name}';",
                cancellationToken ?? CancellationToken.None);
        }

        protected override async Task<IEnumerable<StorageNode>> DoGetDirectoryContentsAsync(string path,
            CancellationToken? cancellationToken = null)
        {
            await using var dbContext = GetDbContext();
            if (path.StartsWith("/"))
            {
                path = path.Substring(1);
            }

            var records = await dbContext.Records
                .Where(r => r.Storage == StorageOptions.Name && r.Path.StartsWith(path))
                .ToListAsync(cancellationToken ?? CancellationToken.None);

            var root = StorageNode.CreateDirectory("/", "/");
            foreach (var itemRecord in records)
            {
                var item = new StorageItem(
                    itemRecord.FilePath, itemRecord.LastModified, itemRecord.FileSize,
                    null, itemRecord.Metadata);
                root.AddItem(item);
            }

            var parts = PreparePath(path.Trim('/'))!.Split("/");
            var current = root;
            foreach (var part in parts)
            {
                current = current?.Children.Where(n => n.Type == StorageNodeType.Directory)
                    .FirstOrDefault(f => f.Name == part);
            }

            return current?.Children ?? new StorageNode[0];
        }

        private static string? PreparePath(string? path)
        {
            return path?.Replace("\\", "/").Replace("//", "/");
        }

        protected override async Task<StorageItemMetadata?> DoGetMetadataJsonAsync(string path,
            CancellationToken? cancellationToken = null)
        {
            await using var dbContext = GetDbContext();
            var record = await GetItemRecordAsync(dbContext, path, cancellationToken);
            return record?.Metadata;
        }

        protected override async Task DoSaveMetadataAsync(StorageItem storageItem, StorageItemMetadata? metadata = null,
            CancellationToken? cancellationToken = null)
        {
            await using var dbContext = GetDbContext();
            var record = await GetItemRecordAsync(dbContext, storageItem.FilePath, cancellationToken);
            if (record is null)
            {
                record = new StorageItemRecord(StorageOptions.Name, storageItem);
                await dbContext.Records.AddAsync(record);
            }

            record.Metadata = metadata;
            await dbContext.SaveChangesAsync(cancellationToken ?? CancellationToken.None);
        }

        protected override async Task DoInitAsync()
        {
            await base.DoInitAsync();
            await using var dbContext = GetDbContext();
            await dbContext.Database.MigrateAsync();
        }

        public override ValueTask DisposeAsync()
        {
            return new();
        }
    }

    public class PostgresStorageMetadataProviderOptions : StorageMetadataProviderOptions
    {
        public string ConnectionString { get; set; }
        public string Schema { get; set; } = "public";
    }
}
