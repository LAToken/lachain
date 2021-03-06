﻿using System;
using System.Collections.Generic;
using System.Linq;
using Lachain.Logger;
using Lachain.Utility.Benchmark;
using Lachain.Utility.Serialization;

namespace Lachain.Crypto.ThresholdSignature
{
    public class ThresholdSigner : IThresholdSigner
    {
        public static readonly TimeBenchmark SignBenchmark = new TimeBenchmark();
        public static readonly TimeBenchmark VerifyBenchmark = new TimeBenchmark();
        public static readonly TimeBenchmark CombineBenchmark = new TimeBenchmark();

        private static readonly ILogger<ThresholdSigner> Logger = LoggerFactory.GetLoggerForClass<ThresholdSigner>();
        private readonly byte[] _dataToSign;
        private readonly PrivateKeyShare _privateKeyShare;
        private readonly PublicKeySet _publicKeySet;
        private readonly Signature?[] _collectedShares;
        private Signature? _signature;
        private int _collectedSharesNumber;

        public ThresholdSigner(IEnumerable<byte> dataToSign, PrivateKeyShare privateKeyShare, PublicKeySet publicKeySet)
        {
            if (!publicKeySet.Keys.Contains(privateKeyShare.GetPublicKeyShare()))
                throw new ArgumentException(
                    "Invalid private key share for threshold signature: " +
                    "corresponding public key is not in keyring"
                );
            _dataToSign = dataToSign.ToArray();
            _privateKeyShare = privateKeyShare;
            _publicKeySet = publicKeySet;
            _collectedShares = new Signature[publicKeySet.Count];
            _collectedSharesNumber = 0;
            _signature = null;
        }

        public Signature Sign()
        {
            return SignBenchmark.Benchmark(() => _privateKeyShare.HashAndSign(_dataToSign));
        }

        public bool AddShare(int idx, Signature sigShare, out Signature? result)
        {
            result = null;
            if (idx < 0 || idx >= _publicKeySet.Count)
            {
                Logger.LogWarning($"Public key (?) is not recognized (index {idx})");
                return false;
            }

            var pubKey = _publicKeySet[idx];
            if (_collectedShares[idx] != null)
            {
                Logger.LogWarning($"Signature share {idx} input twice");
                if (sigShare != _collectedShares[idx])
                    return false;
                _collectedSharesNumber--; // to compensate increment later
            }

            if (!IsShareValid(pubKey, sigShare))
            {
                Logger.LogWarning($"Signature share {idx} is not valid: {sigShare.ToHex()}");
                return false;
            }

            if (_collectedSharesNumber > _publicKeySet.Threshold)
            {
                result = _signature;
                return true;
            }

            Logger.LogTrace($"Collected signature share #{idx}: {sigShare.RawSignature.ToHex()}");
            _collectedShares[idx] = sigShare;
            _collectedSharesNumber += 1;
            if (_collectedSharesNumber <= _publicKeySet.Threshold) return true;
            var signature = CombineBenchmark.Benchmark(() => _publicKeySet.AssembleSignature(
                _collectedShares.Select((share, i) => new KeyValuePair<int, Signature>(i, share))
                    .Where(pair => pair.Value != null).ToArray()
            ));
            if (!_publicKeySet.SharedPublicKey.ValidateSignature(signature, _dataToSign))
                throw new Exception("Fatal error: all shares are valid but combined signature is not");
            _signature = signature;
            result = signature;
            return true;
        }   

        private bool IsShareValid(PublicKey pubKey, Signature sigShare)
        {
            return VerifyBenchmark.Benchmark(() => pubKey.ValidateSignature(sigShare, _dataToSign));
        }
    }
}