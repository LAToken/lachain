﻿using System.Collections.Generic;
using System.Linq;
using Lachain.Consensus;
using Lachain.Crypto.ThresholdSignature;
using Lachain.Logger;
using Lachain.Proto;
using Lachain.Storage.Repositories;
using PublicKey = Lachain.Crypto.TPKE.PublicKey;

namespace Lachain.Core.Blockchain.Validators
{
    public class ValidatorManager : IValidatorManager
    {
        private readonly ISnapshotIndexRepository _snapshotIndexRepository;
        private static readonly ILogger<ValidatorManager> Logger = LoggerFactory.GetLoggerForClass<ValidatorManager>();

        public ValidatorManager(ISnapshotIndexRepository snapshotIndexRepository)
        {
            _snapshotIndexRepository = snapshotIndexRepository;
        }

        public IPublicConsensusKeySet GetValidators(long afterBlock)
        {
            var state = _snapshotIndexRepository.GetSnapshotForBlock((ulong) afterBlock).Validators.GetConsensusState();
            var n = state.Validators.Count;
            var f = (n - 1) / 3;
            return new PublicConsensusKeySet(
                n, f,
                PublicKey.FromBytes(state.TpkePublicKey.ToByteArray()),
                new PublicKeySet(
                    state.Validators.Select(v => Crypto.ThresholdSignature.PublicKey.FromBytes(v.ThresholdSignaturePublicKey.ToByteArray())),
                    f
                ),
                state.Validators.Select(v => v.PublicKey)
            );
        }

        public IReadOnlyCollection<ECDSAPublicKey> GetValidatorsPublicKeys(long afterBlock)
        {
            return _snapshotIndexRepository.GetSnapshotForBlock((ulong) afterBlock).Validators
                .GetValidatorsPublicKeys()
                .ToArray();
        }

        public ECDSAPublicKey GetPublicKey(uint validatorIndex, long afterBlock)
        {
            return GetValidatorsPublicKeys(afterBlock).ElementAt((int) validatorIndex);
        }

        public int GetValidatorIndex(ECDSAPublicKey publicKey, long afterBlock)
        {
            return GetValidatorsPublicKeys(afterBlock)
                .Select((key, index) => new {key, index})
                .Where(arg => publicKey.Equals(arg.key))
                .Select(arg => arg.index)
                .First();
        }

        public bool IsValidatorForBlock(ECDSAPublicKey key, long block)
        {
            return GetValidatorsPublicKeys(block - 1).Contains(key);
        }
    }
}