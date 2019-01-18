﻿using System;

namespace Phorkus.Storage.Repositories
{
    public class VersionRepository
    {
        private readonly IRocksDbContext _dbContext;

        public VersionRepository(IRocksDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public ulong GetVersion(uint repository)
        {
            var rawVersion = _dbContext.Get(EntryPrefix.StorageVersionIndex.BuildPrefix(repository));
            return rawVersion != null ? BitConverter.ToUInt64(rawVersion, 0) : 0u;
        }

        public void SetVersion(uint repository, ulong version)
        {
            _dbContext.Save(EntryPrefix.StorageVersionIndex.BuildPrefix(repository), BitConverter.GetBytes(version));
        }
    }
}