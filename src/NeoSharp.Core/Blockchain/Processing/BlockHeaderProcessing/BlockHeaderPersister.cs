﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NeoSharp.Core.Extensions;
using NeoSharp.Core.Logging;
using NeoSharp.Core.Models;
using NeoSharp.Core.Models.OperationManager;
using NeoSharp.Core.Storage.Blockchain;

namespace NeoSharp.Core.Blockchain.Processing.BlockHeaderProcessing
{
    public class BlockHeaderPersister : IBlockHeaderPersister
    {
        private readonly IBlockRepository _blockRepository;
        private readonly ISigner<BlockHeader> _blockHeaderSigner;
        private readonly IBlockchainContext _blockchainContext;
        private readonly IBlockHeaderValidator _blockHeaderValidator;
        private readonly ILogger<BlockHeaderPersister> _logger;

        public BlockHeaderPersister(
            IBlockRepository blockRepository,
            ISigner<BlockHeader> blockHeaderSigner,
            IBlockchainContext blockchainContext,
            IBlockHeaderValidator blockHeaderValidator,
            ILogger<BlockHeaderPersister> logger)
        {
            _blockRepository = blockRepository;
            _blockHeaderSigner = blockHeaderSigner;
            _blockchainContext = blockchainContext;
            _blockHeaderValidator = blockHeaderValidator;
            _logger = logger;
        }

        public async Task Update(BlockHeader blockHeader)
        {
            if (blockHeader == null)
                throw new ArgumentNullException(nameof(blockHeader));

            await _blockRepository.AddBlockHeader(blockHeader);
        }

        public async Task<IEnumerable<BlockHeader>> Persist(params BlockHeader[] blockHeaders)
        {
            if (blockHeaders == null)
                throw new ArgumentNullException(nameof(blockHeaders));

            List<BlockHeader> blockHeadersToPersist;
            if (_blockchainContext.LastBlockHeader != null)
            {
                blockHeadersToPersist = blockHeaders
                    .Where(bh => bh != null && bh.Index > _blockchainContext.LastBlockHeader.Index)
                    .Distinct(bh => bh.Index)
                    .OrderBy(bh => bh.Index)
                    .ToList();
            }
            else
            {
                blockHeadersToPersist = blockHeaders.ToList();
            }

            foreach (var blockHeader in blockHeadersToPersist)
            {
                _blockHeaderSigner.Sign(blockHeader);

                if (!_blockHeaderValidator.IsValid(blockHeader))
                {
                    _logger.LogInformation(
                        $"Block header with hash {blockHeader.Hash} and index {blockHeader.Index} is invalid and will not be persist.");
                    blockHeadersToPersist.Remove(blockHeader);
                    break;
                }

                await _blockRepository.AddBlockHeader(blockHeader);
                _blockchainContext.LastBlockHeader = blockHeader;
            }

            return blockHeadersToPersist;
        }
    }
}