using System;
using System.Linq;
using System.Numerics;
using Lachain.Consensus.ThresholdKeygen;
using Lachain.Consensus.ThresholdKeygen.Data;
using Lachain.Core.Blockchain.Error;
using Lachain.Core.Blockchain.Interface;
using Lachain.Core.Blockchain.Pool;
using Lachain.Core.Blockchain.SystemContracts;
using Lachain.Core.Blockchain.SystemContracts.ContractManager;
using Lachain.Core.Blockchain.SystemContracts.Interface;
using Lachain.Core.Blockchain.VM;
using Lachain.Crypto;
using Lachain.Crypto.ThresholdSignature;
using Lachain.Logger;
using Lachain.Proto;
using Lachain.Storage.Repositories;
using Lachain.Utility;
using Lachain.Utility.Serialization;
using Lachain.Utility.Utils;
using PublicKey = Lachain.Crypto.TPKE.PublicKey;

namespace Lachain.Core.Vault
{
    public class KeyGenManager : IKeyGenManager
    {
        private static readonly ILogger<KeyGenManager> Logger = LoggerFactory.GetLoggerForClass<KeyGenManager>();

        private readonly ITransactionBuilder _transactionBuilder;
        private readonly IPrivateWallet _privateWallet;
        private readonly ITransactionPool _transactionPool;
        private readonly ITransactionSigner _transactionSigner;
        private readonly IKeyGenRepository _keyGenRepository;

        public KeyGenManager(
            IBlockManager blockManager,
            ITransactionBuilder transactionBuilder,
            IPrivateWallet privateWallet,
            ITransactionPool transactionPool,
            ITransactionSigner transactionSigner,
            IKeyGenRepository keyGenRepository
        )
        {
            _transactionBuilder = transactionBuilder;
            _privateWallet = privateWallet;
            _transactionPool = transactionPool;
            _transactionSigner = transactionSigner;
            _keyGenRepository = keyGenRepository;
            blockManager.OnSystemContractInvoked += BlockManagerOnSystemContractInvoked;
        }

        private void BlockManagerOnSystemContractInvoked(object _, InvocationContext context)
        {
            if (context.Receipt is null) return;
            var tx = context.Receipt.Transaction;
            if (
                !tx.To.Equals(ContractRegisterer.GovernanceContract) &&
                !tx.To.Equals(ContractRegisterer.StakingContract)
            ) return;
            if (tx.Invocation.Length < 4) return;

            var signature = ContractEncoder.MethodSignatureAsInt(tx.Invocation);
            var decoder = new ContractDecoder(tx.Invocation.ToArray());
            if (signature == ContractEncoder.MethodSignatureAsInt(StakingInterface.MethodFinishVrfLottery))
            {
                Logger.LogTrace($"Detected call of GovernanceContract.{GovernanceInterface.MethodChangeValidators}");
                var data = new GovernanceContract(context).GetNextValidators();
                var publicKeys =
                    (data ?? throw new ArgumentException("Cannot parse method args"))
                    .Select(x => x.ToPublicKey())
                    .ToArray();
                if (!publicKeys.Contains(_privateWallet.EcdsaKeyPair.PublicKey))
                {
                    Logger.LogWarning("Skipping validator change event since we are not new validator");
                    return;
                }

                var keygen = GetCurrentKeyGen();
                if (keygen != null)
                    throw new ArgumentException("Cannot start keygen, since one is already running");
                var faulty = (publicKeys.Length - 1) / 3;
                keygen = new TrustlessKeygen(_privateWallet.EcdsaKeyPair, publicKeys, faulty);
                var commitTx = MakeCommitTransaction(keygen.StartKeygen());
                if (_transactionPool.Add(commitTx) is var error && error != OperatingError.Ok)
                    Logger.LogError($"Error creating commit transaction ({commitTx.Hash.ToHex()}): {error}");
                else
                    Logger.LogInformation($"KeyGen Commit transaction sent");

                _keyGenRepository.SaveKeyGenState(keygen.ToBytes());
            }
            else if (signature == ContractEncoder.MethodSignatureAsInt(GovernanceInterface.MethodKeygenCommit))
            {
                Logger.LogTrace($"Detected call of GovernanceContract.{GovernanceInterface.MethodKeygenCommit}");
                var keygen = GetCurrentKeyGen();
                if (keygen is null)
                {
                    Logger.LogWarning("Skipping call since there is no keygen running");
                    return;
                }

                var sender = keygen.GetSenderByPublicKey(context.Receipt.RecoverPublicKey());
                if (sender < 0)
                {
                    Logger.LogWarning($"Skipping call because of invalid sender: {sender}");
                    return;
                }

                var args = decoder.Decode(GovernanceInterface.MethodKeygenCommit);
                var commitment = Commitment.FromBytes(args[0] as byte[] ?? throw new Exception());
                var encryptedRows = args[1] as byte[][] ?? throw new Exception();
                var sendValueTx = MakeSendValueTransaction(
                    keygen.HandleCommit(
                        sender,
                        new CommitMessage {Commitment = commitment, EncryptedRows = encryptedRows}
                    )
                );
                if (_transactionPool.Add(sendValueTx) is var error && error != OperatingError.Ok)
                    Logger.LogError($"Error creating send value transaction ({sendValueTx.Hash.ToHex()}): {error}");
                else
                    Logger.LogInformation($"KeyGen Send value transaction sent");

                _keyGenRepository.SaveKeyGenState(keygen.ToBytes());
            }
            else if (signature == ContractEncoder.MethodSignatureAsInt(GovernanceInterface.MethodKeygenSendValue))
            {
                Logger.LogTrace($"Detected call of GovernanceContract.{GovernanceInterface.MethodKeygenSendValue}");
                var keygen = GetCurrentKeyGen();
                if (keygen is null) return;
                var sender = keygen.GetSenderByPublicKey(context.Receipt.RecoverPublicKey());
                if (sender < 0) return;
                var args = decoder.Decode(GovernanceInterface.MethodKeygenSendValue);
                var proposer = args[0] as UInt256 ?? throw new Exception();
                var encryptedValues = args[1] as byte[][] ?? throw new Exception();

                if (keygen.HandleSendValue(
                    sender,
                    new ValueMessage {Proposer = (int) proposer.ToBigInteger(false), EncryptedValues = encryptedValues}
                ))
                {
                    var keys = keygen.TryGetKeys() ?? throw new Exception();
                    var confirmTx = MakeConfirmTransaction(keys);
                    if (_transactionPool.Add(confirmTx) is var error && error != OperatingError.Ok)
                        Logger.LogError($"Error creating confirm transaction ({confirmTx.Hash.ToHex()}): {error}");
                    else
                        Logger.LogInformation($"KeyGen Confirm transaction sent");
                }

                _keyGenRepository.SaveKeyGenState(keygen.ToBytes());
            }
            else if (signature == ContractEncoder.MethodSignatureAsInt(GovernanceInterface.MethodKeygenConfirm))
            {
                Logger.LogTrace($"Detected call of GovernanceContract.{GovernanceInterface.MethodKeygenConfirm}");
                var keygen = GetCurrentKeyGen();
                if (keygen is null) return;
                var sender = keygen.GetSenderByPublicKey(context.Receipt.RecoverPublicKey());
                if (sender < 0) return;

                var args = decoder.Decode(GovernanceInterface.MethodKeygenConfirm);
                var tpkePublicKey = PublicKey.FromBytes(args[0] as byte[] ?? throw new Exception());
                var tsKeys = new PublicKeySet(
                    (args[1] as byte[][] ?? throw new Exception()).Select(x =>
                        Crypto.ThresholdSignature.PublicKey.FromBytes(x)
                    ),
                    keygen.Faulty
                );

                if (keygen.HandleConfirm(tpkePublicKey, tsKeys))
                {
                    var keys = keygen.TryGetKeys() ?? throw new Exception();
                    Logger.LogTrace($"Generated keyring with public hash {keys.PublicPartHash().ToHex()}");
                    Logger.LogTrace($"  - TPKE public key: {keys.TpkePublicKey.ToHex()}");
                    Logger.LogTrace(
                        $"  - TS public key: {keys.ThresholdSignaturePrivateKey.GetPublicKeyShare().ToHex()}");
                    Logger.LogTrace(
                        $"  - TS public key set: {string.Join(", ", keys.ThresholdSignaturePublicKeySet.Keys.Select(key => key.ToHex()))}"
                    );
                    var lastBlockInCurrentCycle = (context.Receipt.Block / StakingContract.CycleDuration + 1) *
                                                  StakingContract.CycleDuration;
                    _privateWallet.AddThresholdSignatureKeyAfterBlock(
                        lastBlockInCurrentCycle, keys.ThresholdSignaturePrivateKey
                    );
                    _privateWallet.AddTpkePrivateKeyAfterBlock(lastBlockInCurrentCycle, keys.TpkePrivateKey);
                    Logger.LogDebug("New keyring saved to wallet");
                    _keyGenRepository.SaveKeyGenState(Array.Empty<byte>());
                }
                else
                {
                    _keyGenRepository.SaveKeyGenState(keygen.ToBytes());
                }
            }
        }

        private TransactionReceipt MakeConfirmTransaction(ThresholdKeyring keyring)
        {
            var tx = _transactionBuilder.InvokeTransaction(
                _privateWallet.EcdsaKeyPair.PublicKey.GetAddress(),
                ContractRegisterer.GovernanceContract,
                Money.Zero,
                GovernanceInterface.MethodKeygenConfirm,
                keyring.TpkePublicKey.ToBytes(),
                keyring.ThresholdSignaturePublicKeySet.Keys.Select(key => key.ToBytes()).ToArray()
            );
            return _transactionSigner.Sign(tx, _privateWallet.EcdsaKeyPair);
        }

        private TransactionReceipt MakeSendValueTransaction(ValueMessage valueMessage)
        {
            var tx = _transactionBuilder.InvokeTransaction(
                _privateWallet.EcdsaKeyPair.PublicKey.GetAddress(),
                ContractRegisterer.GovernanceContract,
                Money.Zero,
                GovernanceInterface.MethodKeygenSendValue,
                new BigInteger(valueMessage.Proposer).ToUInt256(),
                valueMessage.EncryptedValues
            );
            return _transactionSigner.Sign(tx, _privateWallet.EcdsaKeyPair);
        }

        private TransactionReceipt MakeCommitTransaction(CommitMessage commitMessage)
        {
            var tx = _transactionBuilder.InvokeTransaction(
                _privateWallet.EcdsaKeyPair.PublicKey.GetAddress(),
                ContractRegisterer.GovernanceContract,
                Money.Zero,
                GovernanceInterface.MethodKeygenCommit,
                commitMessage.Commitment.ToBytes(),
                commitMessage.EncryptedRows
            );
            return _transactionSigner.Sign(tx, _privateWallet.EcdsaKeyPair);
        }

        private TrustlessKeygen? GetCurrentKeyGen()
        {
            var bytes = _keyGenRepository.LoadKeyGenState();
            if (bytes is null || bytes.Length == 0) return null;
            return TrustlessKeygen.FromBytes(bytes, _privateWallet.EcdsaKeyPair);
        }
    }
}