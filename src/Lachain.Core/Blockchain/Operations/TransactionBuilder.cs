﻿using System;
using System.Collections.Generic;
using System.Linq;
using Google.Protobuf;
using Lachain.Core.Blockchain.Interface;
using Lachain.Core.Blockchain.Pool;
using Lachain.Core.Blockchain.SystemContracts.ContractManager;
using Lachain.Core.Blockchain.SystemContracts.Interface;
using Lachain.Core.Blockchain.VM;
using Lachain.Logger;
using Lachain.Proto;
using Lachain.Storage.State;
using Lachain.Utility;
using Lachain.Utility.Utils;

namespace Lachain.Core.Blockchain.Operations
{
    public class TransactionBuilder : ITransactionBuilder
    {
        private static readonly ILogger<TransactionBuilder> Logger =
            LoggerFactory.GetLoggerForClass<TransactionBuilder>();

        private readonly IStateManager _stateManager;
        private readonly ITransactionPool _transactionPool;

        public TransactionBuilder(IStateManager stateManager, ITransactionPool transactionPool)
        {
            _stateManager = stateManager;
            _transactionPool = transactionPool;
        }

        public Transaction TransferTransaction(UInt160 from, UInt160 to, Money value, ulong gasPrice, byte[]? input)
        {
            var nonce = _transactionPool.GetNextNonceForAddress(from);
            if (gasPrice == 0) gasPrice = (ulong) _stateManager.CurrentSnapshot.NetworkGasPrice;
            var tx = new Transaction
            {
                To = to,
                Value = value.ToUInt256(),
                From = from,
                GasPrice = gasPrice,
                GasLimit = GasMetering.DefaultBlockGasLimit,
                Nonce = nonce
            };
            if (input != null)
                tx.Invocation = ByteString.CopyFrom(input);
            return tx;
        }

        public Transaction DeployTransaction(UInt160 from, IEnumerable<byte> byteCode, byte[]? input)
        {
            var nonce = _transactionPool.GetNextNonceForAddress(from);
            var abi = ContractEncoder.Encode(DeployInterface.MethodDeploy, byteCode);
            var tx = new Transaction
            {
                Invocation = ByteString.CopyFrom(abi),
                From = from,
                To = ContractRegisterer.DeployContract, 
                GasPrice = _CalcEstimatedBlockFee(),
                /* TODO: "calculate gas limit for input size" */
                GasLimit = GasMetering.DefaultBlockGasLimit,
                Nonce = nonce,
                Value = UInt256Utils.Zero,
            };
            return tx;
        }

        public Transaction TokenTransferTransaction(UInt160 contract, UInt160 from, UInt160 to, Money value)
        {
            var nonce = _transactionPool.GetNextNonceForAddress(from);
            var abi = ContractEncoder.Encode("transfer(address,uint256)", to, value.ToUInt256());
            var tx = new Transaction
            {
                To = contract,
                Invocation = ByteString.CopyFrom(abi),
                From = from,
                GasPrice = _CalcEstimatedBlockFee(),
                /* TODO: "calculate gas limit for input size" */
                GasLimit = GasMetering.DefaultBlockGasLimit,
                Nonce = nonce,
                Value = UInt256Utils.Zero
            };
            return tx;
        }

        public Transaction InvokeTransaction(UInt160 from, UInt160 contract, Money value, string methodSignature,
            params dynamic[] values)
        {
            var nonce = _transactionPool.GetNextNonceForAddress(from);
            var abi = ContractEncoder.Encode(methodSignature, values);
            var tx = new Transaction
            {
                To = contract,
                Invocation = ByteString.CopyFrom(abi),
                From = from,
                GasPrice = (ulong) _stateManager.CurrentSnapshot.NetworkGasPrice,
                /* TODO: "calculate gas limit for input size" */
                GasLimit = 100000000,
                Nonce = nonce,
                Value = value.ToUInt256()
            };
            return tx;
        }

        public Transaction InvokeTransactionWithGasPrice(UInt160 from, UInt160 contract, Money value,
            string methodSignature,
            ulong gasPrice,
            params dynamic[] values)
        {
            var nonce = _transactionPool.GetNextNonceForAddress(from);
            var abi = ContractEncoder.Encode(methodSignature, values);
            var tx = new Transaction
            {
                To = contract,
                Invocation = ByteString.CopyFrom(abi),
                From = from,
                GasPrice = gasPrice,
                /* TODO: "calculate gas limit for input size" */
                GasLimit = 100000000,
                Nonce = nonce,
                Value = value.ToUInt256()
            };
            return tx;
        }

        private ulong _CalcEstimatedBlockFee()
        {
            var block = _stateManager.LastApprovedSnapshot.Blocks.GetBlockByHeight(
                _stateManager.LastApprovedSnapshot.Blocks.GetTotalBlockHeight());
            return block is null ? 0 : _CalcEstimatedBlockFee(block.TransactionHashes);
        }

        private ulong _CalcEstimatedBlockFee(IEnumerable<UInt256> txHashes)
        {
            var arrayOfHashes = txHashes as UInt256[] ?? txHashes.ToArray();
            if (arrayOfHashes.Length == 0)
                return 0;
            var sum = arrayOfHashes.SelectMany(txHash =>
                {
                    var tx = _stateManager.CurrentSnapshot.Transactions.GetTransactionByHash(txHash);
                    return tx is null ? Enumerable.Empty<TransactionReceipt>() : new[] {tx};
                })
                .Aggregate(0UL, (current, tx) => current + tx.Transaction.GasPrice);
            return sum / (ulong) arrayOfHashes.Length;
        }
    }
}