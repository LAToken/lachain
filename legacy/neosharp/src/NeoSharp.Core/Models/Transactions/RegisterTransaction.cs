﻿using System.IO;
using NeoSharp.BinarySerialization;
using NeoSharp.BinarySerialization.Extensions;
using NeoSharp.Core.Converters;

namespace NeoSharp.Core.Models.Transactions
{
    [BinaryTypeSerializer(typeof(TransactionSerializer))]
    public class RegisterTransaction : Transaction
    {
        public const uint NameMaxSize = 50;
        
        /// <summary>
        /// Asset Type
        /// </summary>
        public AssetType AssetType;

        /// <summary>
        /// Name
        /// </summary>
        public string Name;

        /// <summary>
        /// The total number of issues, a total of two modes:
        ///   1. Limited Mode: When Amount is positive, the maximum total amount of the current asset is Amount, and cannot be modified (Equities may support expansion or additional issuance in the future, and will consider the company’s signature or a certain proportion of shareholders Signature recognition).
        ///   2. Unlimited mode: When Amount is equal to -1, the current asset can be issued by the creator indefinitely. This model has the greatest degree of freedom, but it has the lowest credibility and is not recommended for use.
        /// </summary>
        public UInt256 Supply;
        
        /// <summary>
        /// Precision
        /// </summary>
        public byte Precision;

        /// <summary>
        /// Asset Manager Contract Hash Value
        /// </summary>
        public UInt160 Owner;

        /// <summary>
        /// Constructor
        /// </summary>
        public RegisterTransaction() : base(TransactionType.RegisterTransaction)
        {
        }

        protected override void DeserializeExclusiveData(IBinarySerializer deserializer, BinaryReader reader,
            BinarySerializerSettings settings = null)
        {
            AssetType = (AssetType) reader.ReadByte();
            Name = reader.ReadVarString(NameMaxSize);
            Supply = new UInt256(reader.ReadBytes(UInt256.BufferLength));
            Precision = reader.ReadByte();
            Owner = new UInt160(reader.ReadBytes(UInt160.BufferLength));
        }

        protected override int SerializeExclusiveData(IBinarySerializer serializer, BinaryWriter writer,
            BinarySerializerSettings settings = null)
        {
            var result = 1;
            writer.Write((byte) AssetType);
            result += writer.WriteVarString(Name, NameMaxSize);
            result += writer.WriteArray(Supply.ToArray());
            writer.Write(Precision);
            result++;
            result += writer.WriteArray(Owner.ToArray());

            return result;
        }
    }
}