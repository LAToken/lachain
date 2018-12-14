﻿using System;
using System.Linq;
using Org.BouncyCastle.Asn1;
using Org.BouncyCastle.Asn1.Sec;
using Org.BouncyCastle.Asn1.X9;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Crypto.Signers;
using Org.BouncyCastle.Math;
using Org.BouncyCastle.Math.EC;
using Org.BouncyCastle.Security;

namespace Phorkus.Crypto
{
    internal sealed class EcDsaSignerWithRecId : ECDsaSigner
    {
        public BigInteger[] GenerateSignatureWithRecId(byte[] message, out byte recId)
        {
            var parameters = key.Parameters;
            var n = parameters.N;
            var e = CalculateE(n, message);
            var d = ((ECPrivateKeyParameters) key).D;
            if (kCalculator.IsDeterministic)
                kCalculator.Init(n, d, message);
            else
                kCalculator.Init(n, random);
            var basePointMultiplier = CreateBasePointMultiplier();
            BigInteger val;
            BigInteger bigInteger;
            do
            {
                BigInteger k;
                do
                {
                    k = kCalculator.NextK();
                    var T = basePointMultiplier.Multiply(parameters.G, k).Normalize();
                    val = T.AffineXCoord.ToBigInteger().Mod(n);
                    recId = (byte) (T.YCoord.TestBitZero() ? 1 : 0);
                } while (val.SignValue == 0);

                bigInteger = k.ModInverse(n).Multiply(e.Add(d.Multiply(val))).Mod(n);
            } while (bigInteger.SignValue == 0);

            return new[] {val, bigInteger};
        }
    }

    public class BouncyCastle : ICrypto
    {
        private static readonly X9ECParameters Curve = SecNamedCurves.GetByName("secp256r1");

        private static readonly ECDomainParameters Domain
            = new ECDomainParameters(Curve.Curve, Curve.G, Curve.N, Curve.H, Curve.GetSeed());

        public bool VerifySignature(byte[] message, byte[] signature, byte[] pubkey)
        {
            var fullpubkey = DecodePublicKey(pubkey, false, out _, out _);

            var point = Curve.Curve.DecodePoint(fullpubkey);
            var keyParameters = new ECPublicKeyParameters(point, Domain);

            var signer = SignerUtilities.GetSigner("SHA256withECDSA");
            signer.Init(false, keyParameters);
            signer.BlockUpdate(message, 0, message.Length);

            if (signature.Length == 65)
            {
                signature = new DerSequence(
                        new DerInteger(new BigInteger(1, signature.Skip(1).Take(32).ToArray())),
                        new DerInteger(new BigInteger(1, signature.Skip(1).Skip(32).ToArray())))
                    .GetDerEncoded();
            }

            return signer.VerifySignature(signature);
        }

        public byte[] Sign(byte[] message, byte[] prikey)
        {
            var priv = new ECPrivateKeyParameters("ECDSA", new BigInteger(1, prikey), Domain);
            var signer = new EcDsaSignerWithRecId();
            var fullsign = new byte[65];

            message = message.Sha256();
            signer.Init(true, priv);
            var signature = signer.GenerateSignatureWithRecId(message, out var recId);
            var r = signature[0].ToByteArray();
            var s = signature[1].ToByteArray();
            var rLen = r.Length;
            var sLen = s.Length;

            // Build Signature ensuring expected format. 1byte v + 32byte r + 32byte s.
            fullsign[0] = recId;
            if (rLen < 32)
                Array.Copy(r, 0, fullsign, 33 - rLen, rLen);
            else
                Array.Copy(r, rLen - 32, fullsign, 1, 32);
            if (sLen < 32)
                Array.Copy(s, 0, fullsign, 65 - sLen, sLen);
            else
                Array.Copy(s, sLen - 32, fullsign, 33, 32);

            return fullsign;
        }

        public byte[] RecoverSignature(byte[] message, byte[] signature)
        {
            var recId = signature[0];
            var r = new BigInteger(new byte[] {0}.Concat(signature.Skip(1).Take(32)).ToArray(), 0, 33);
            var s = new BigInteger(new byte[] {0}.Concat(signature.Skip(1).Skip(32)).ToArray(), 0, 33);

            var hash = message.Sha256();

            var curve = Curve.Curve as FpCurve ?? throw new ArgumentException("Unable to cast Curve to FpCurve");
            var order = Curve.N;

            var x = r;
            if ((recId & 2) != 0)
                x = x.Add(order);

            if (x.CompareTo(curve.Q) >= 0)
                throw new ArgumentException("X too large");

            var xEnc = X9IntegerConverter.IntegerToBytes(x, X9IntegerConverter.GetByteLength(curve));
            var compEncoding = new byte[xEnc.Length + 1];

            compEncoding[0] = (byte) (0x02 + (recId & 1));
            xEnc.CopyTo(compEncoding, 1);
            var R = curve.DecodePoint(compEncoding);

            var e = CalculateE(order, hash);

            var rInv = r.ModInverse(order);
            var srInv = s.Multiply(rInv).Mod(order);
            var erInv = e.Multiply(rInv).Mod(order);

            var point = ECAlgorithms.SumOfTwoMultiplies(R, srInv, Curve.G.Negate(), erInv);
            return point.Normalize().GetEncoded(true);
        }

        public byte[] ComputeAddress(byte[] publicKey)
        {
            var decodedKey = DecodePublicKey(publicKey, false, out _, out _);
            return decodedKey.Ripemd160();
        }

        private static BigInteger CalculateE(BigInteger n, byte[] message)
        {
            var messageBitLength = message.Length * 8;
            var trunc = new BigInteger(1, message);
            if (n.BitLength < messageBitLength)
                trunc = trunc.ShiftRight(messageBitLength - n.BitLength);
            return trunc;
        }

        public byte[] ComputePublicKey(byte[] privateKey, bool compress = true)
        {
            if (privateKey == null)
                throw new ArgumentException(nameof(privateKey));

            var q = Domain.G.Multiply(new BigInteger(1, privateKey));
            var publicParams = new ECPublicKeyParameters(q, Domain);

            var result = publicParams.Q.GetEncoded(compress);
            return result;
        }

        public byte[] DecodePublicKey(byte[] pubkey, bool compress, out System.Numerics.BigInteger x,
            out System.Numerics.BigInteger y)
        {
            if (pubkey == null || pubkey.Length != 33 && pubkey.Length != 64 && pubkey.Length != 65)
                throw new ArgumentException(nameof(pubkey));

            if (pubkey.Length == 33 && pubkey[0] != 0x02 && pubkey[0] != 0x03)
                throw new ArgumentException(nameof(pubkey));
            if (pubkey.Length == 65 && pubkey[0] != 0x04) throw new ArgumentException(nameof(pubkey));

            byte[] fullpubkey;

            if (pubkey.Length == 64)
            {
                fullpubkey = new byte[65];
                fullpubkey[0] = 0x04;
                Array.Copy(pubkey, 0, fullpubkey, 1, pubkey.Length);
            }
            else
            {
                fullpubkey = pubkey;
            }

            var ret = new ECPublicKeyParameters("ECDSA", Curve.Curve.DecodePoint(fullpubkey), Domain).Q;
            var x0 = ret.XCoord.ToBigInteger();
            var y0 = ret.YCoord.ToBigInteger();

            x = System.Numerics.BigInteger.Parse(x0.ToString());
            y = System.Numerics.BigInteger.Parse(y0.ToString());

            return ret.GetEncoded(compress);
        }

        public byte[] AesEncrypt(byte[] data, byte[] key)
        {
            if (data == null || data.Length % 16 != 0) throw new ArgumentException(nameof(data));
            if (key == null || key.Length != 32) throw new ArgumentException(nameof(key));

            var cipher = CipherUtilities.GetCipher("AES/ECB/NoPadding");
            cipher.Init(true, ParameterUtilities.CreateKeyParameter("AES", key));

            return cipher.DoFinal(data);
        }

        public byte[] AesDecrypt(byte[] data, byte[] key)
        {
            if (data == null || data.Length % 16 != 0) throw new ArgumentException(nameof(data));
            if (key == null || key.Length != 32) throw new ArgumentException(nameof(key));

            var cipher = CipherUtilities.GetCipher("AES/ECB/NoPadding");
            cipher.Init(false, ParameterUtilities.CreateKeyParameter("AES", key));

            return cipher.DoFinal(data);
        }

        public byte[] AesEncrypt(byte[] data, byte[] key, byte[] iv)
        {
            if (data == null || data.Length % 16 != 0) throw new ArgumentException(nameof(data));
            if (key == null || key.Length != 32) throw new ArgumentException(nameof(key));
            if (iv == null || iv.Length != 16) throw new ArgumentException(nameof(iv));

            var cipher = CipherUtilities.GetCipher("AES/CBC/NoPadding");
            cipher.Init(true, new ParametersWithIV(ParameterUtilities.CreateKeyParameter("AES", key), iv));

            return cipher.DoFinal(data);
        }

        public byte[] AesDecrypt(byte[] data, byte[] key, byte[] iv)
        {
            if (data == null || data.Length % 16 != 0) throw new ArgumentException(nameof(data));
            if (key == null || key.Length != 32) throw new ArgumentException(nameof(key));
            if (iv == null || iv.Length != 16) throw new ArgumentException(nameof(iv));

            var cipher = CipherUtilities.GetCipher("AES/CBC/NoPadding");
            cipher.Init(false, new ParametersWithIV(ParameterUtilities.CreateKeyParameter("AES", key), iv));

            return cipher.DoFinal(data);
        }

        public byte[] SCrypt(byte[] P, byte[] S, int N, int r, int p, int dkLen)
        {
            if (P == null) throw new ArgumentException(nameof(P));
            if (S == null) throw new ArgumentException(nameof(S));
            if ((N & (N - 1)) != 0 || N < 2 || N >= Math.Pow(2, 128 * r / 8)) throw new ArgumentException(nameof(N));
            if (r < 1) throw new ArgumentException(nameof(r));
            if (p < 1 || p > int.MaxValue / (128 * r * 8)) throw new ArgumentException(nameof(p));
            if (dkLen < 1) throw new ArgumentException(nameof(dkLen));

            return Org.BouncyCastle.Crypto.Generators.SCrypt.Generate(P, S, N, r, p, dkLen);
        }

        public byte[] GenerateRandomBytes(int length)
        {
            if (length < 1)
                throw new ArgumentException(nameof(length));

            var privateKey = new byte[length];
            using (var rng = System.Security.Cryptography.RandomNumberGenerator.Create())
                rng.GetBytes(privateKey);

            return privateKey;
        }
    }
}