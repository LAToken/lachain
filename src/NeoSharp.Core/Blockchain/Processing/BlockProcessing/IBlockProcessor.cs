﻿using System;
using System.Threading.Tasks;
using NeoSharp.Core.Models;

namespace NeoSharp.Core.Blockchain.Processing.BlockProcessing
{
    public interface IBlockProcessor : IDisposable
    {
        /// <summary>
        /// Run process that will process blocks
        /// </summary>
        void Run();

        /// <summary>
        /// Add the specified block to the blockchain
        /// </summary>
        /// <param name="block"></param>
        /// <returns></returns>
        Task<Block> AddBlock(Block block);
        
        void WaitUntilBlockProcessed(uint index);
    }
}