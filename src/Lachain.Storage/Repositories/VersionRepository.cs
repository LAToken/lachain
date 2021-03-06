﻿using System.Linq;
using Lachain.Utility.Serialization;

namespace Lachain.Storage.Repositories
{
    public class VersionRepository : IVersionRepository
    {
        private readonly IRocksDbContext _dbContext;

        public VersionRepository(IRocksDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public ulong GetVersion(uint repository)
        {
            var rawVersion = _dbContext.Get(EntryPrefix.StorageVersionIndex.BuildPrefix(repository));
            return rawVersion?.AsReadOnlySpan().ToUInt64() ?? 0u;
        }

        public void SetVersion(uint repository, ulong version, RocksDbAtomicWrite tx)
        {
            tx.Put(EntryPrefix.StorageVersionIndex.BuildPrefix(repository), version.ToBytes().ToArray());
        }
    }
}