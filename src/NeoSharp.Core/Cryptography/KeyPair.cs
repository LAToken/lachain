﻿using NeoSharp.Core.Extensions;
using NeoSharp.Cryptography;

namespace NeoSharp.Core.Cryptography
{
    public class KeyPair
    {
        public readonly byte[] PrivateKey;
        public readonly PublicKey PublicKey;
        
        public UInt160 Address => PublicKey.EncodedData.ToScriptHash();
        
        public KeyPair(byte[] privateKey)
        {
            PrivateKey = privateKey;
            PublicKey = new PublicKey(Crypto.Default.ComputePublicKey(privateKey, true));
        }

        public bool Equals(KeyPair other)
        {
            if (ReferenceEquals(this, other))
                return true;
            return !(other is null) && PublicKey.Equals(other.PublicKey);
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as KeyPair);
        }

        public override int GetHashCode()
        {
            return PublicKey.GetHashCode();
        }

        public override string ToString()
        {
            return PublicKey.ToString();
        }
    }
}