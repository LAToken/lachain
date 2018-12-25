﻿using Phorkus.Proto;
using Phorkus.Storage.State;

namespace Phorkus.Core.Blockchain.OperationManager.TransactionManager
{
    public class MinerTranscationExecuter : ITransactionExecuter
    {
        public OperatingError Execute(Block block, Transaction transaction, IBlockchainSnapshot snapshot)
        {
            return OperatingError.Ok;
        }
        
        public OperatingError Verify(Transaction transaction)
        {
            if (transaction.Type != TransactionType.Miner)
                return OperatingError.InvalidTransaction;
            return OperatingError.Ok;
        }
    }
}