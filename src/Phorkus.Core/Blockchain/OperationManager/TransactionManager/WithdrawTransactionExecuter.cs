﻿using Phorkus.Core.Blockchain.State;
using Phorkus.Proto;
using Phorkus.Utility;
using Phorkus.Utility.Utils;

namespace Phorkus.Core.Blockchain.OperationManager.TransactionManager
{
    public class WithdrawTransactionExecuter : ITransactionExecuter
    {
        private readonly IValidatorManager _validatorManager;

        public WithdrawTransactionExecuter(IValidatorManager validatorManager)
        {
            _validatorManager = validatorManager;
        }

        public OperatingError Execute(Block block, Transaction transaction, IBlockchainSnapshot snapshot)
        {
            var balances = snapshot.Balances;
            var error = Verify(transaction);
            if (error != OperatingError.Ok)
                return error;
            var withdraw = transaction.Withdraw;
            if (!withdraw.Value.IsZero())
            {
                var assetName = withdraw.BlockchainType == BlockchainType.Bitcoin
                    ? snapshot.Assets.GetAssetByName("BTC").Hash
                    : snapshot.Assets.GetAssetByName("ETH").Hash;
                var newSupply = new Money(snapshot.Assets.GetAssetByHash(assetName).Supply) + new Money(withdraw.Value);
                snapshot.Assets.GetAssetByHash(assetName).Supply = newSupply.ToUInt256();
                balances.TransferBalance(transaction.From, withdraw.Recipient,
                    withdraw.BlockchainType == BlockchainType.Bitcoin
                        ? snapshot.Assets.GetAssetByName("BTC").Hash
                        : snapshot.Assets.GetAssetByName("ETH").Hash, new Money(withdraw.Value));
            }

            /* TODO: "invoke smart-contract code here" */
            return OperatingError.Ok;
        }

        public OperatingError Verify(Transaction transaction)
        {
            if (transaction.Type != TransactionType.Withdraw)
                return OperatingError.InvalidTransaction;
            var confirm = transaction.Deposit;
            if (confirm?.BlockchainType is null)
                return OperatingError.InvalidTransaction;
            if (confirm?.TransactionHash is null)
                return OperatingError.InvalidTransaction;
            if (confirm?.Timestamp is null)
                return OperatingError.InvalidTransaction;
            if (confirm?.AddressFormat is null)
                return OperatingError.InvalidTransaction;
            if (confirm.Recipient is null)
                return OperatingError.InvalidTransaction;
            if (confirm.Value is null)
                return OperatingError.InvalidTransaction;
            if (!_validatorManager.CheckValidator(transaction.From))
            {
                return OperatingError.InvalidTransaction;
            }

            throw new OperationNotSupportedException();
        }
    }
}