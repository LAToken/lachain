using System.Linq;
using NUnit.Framework;
using Lachain.Crypto.ThresholdSignature;
using Lachain.Utility.Serialization;

namespace Lachain.CryptoTest
{
    public class ThresholdCryptoTest
    {
        [Test]
        public void ThresholdKeyGen()
        {
            const int n = 7, f = 2;
            var keygen = new TrustedKeyGen(n, f);
            var shares = keygen.GetPrivateShares().ToArray();
            var data = 0xdeadbeef.ToBytes().ToArray();
            var pubKeys = new PublicKeySet(shares.Select(share => share.GetPublicKeyShare()), f);
            var signers = new IThresholdSigner[n];
            for (var i = 0; i < n; ++i)
            {
                signers[i] = new ThresholdSigner(data, shares[i], pubKeys);
            }

            var signatureShares = signers.Select(signer => signer.Sign()).ToArray();
            var sigs = new Signature[n];
            var success = new bool[n];
            for (var i = 0; i < n; ++i) success[i] = true;
            for (var i = 0; i < n; ++i)
            {
                for (var j = 0; j < n; ++j)
                {
                    success[i] &= signers[i].AddShare(j, signatureShares[j], out var sig);
                    if (sigs[i] == null && sig != null)
                        sigs[i] = sig;
                }
            }

            for (int i = 0; i < n; ++i)
            {
                Assert.IsTrue(success[i], $"Player {i} did not terminate successfully");
                Assert.IsTrue(pubKeys.SharedPublicKey.ValidateSignature(sigs[i], data));
            }
        }
    }
}