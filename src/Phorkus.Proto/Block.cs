// <auto-generated>
//     Generated by the protocol buffer compiler.  DO NOT EDIT!
//     source: block.proto
// </auto-generated>
#pragma warning disable 1591, 0612, 3021
#region Designer generated code

using pb = global::Google.Protobuf;
using pbc = global::Google.Protobuf.Collections;
using pbr = global::Google.Protobuf.Reflection;
using scg = global::System.Collections.Generic;
namespace Phorkus.Proto {

  /// <summary>Holder for reflection information generated from block.proto</summary>
  public static partial class BlockReflection {

    #region Descriptor
    /// <summary>File descriptor for block.proto</summary>
    public static pbr::FileDescriptor Descriptor {
      get { return descriptor; }
    }
    private static pbr::FileDescriptor descriptor;

    static BlockReflection() {
      byte[] descriptorData = global::System.Convert.FromBase64String(
          string.Concat(
            "CgtibG9jay5wcm90bxoNZGVmYXVsdC5wcm90bxoObXVsdGlzaWcucHJvdG8i",
            "sAEKC0Jsb2NrSGVhZGVyEg8KB3ZlcnNpb24YASABKA0SIQoPcHJldl9ibG9j",
            "a19oYXNoGAIgASgLMgguVUludDI1NhIdCgttZXJrbGVfcm9vdBgDIAEoCzII",
            "LlVJbnQyNTYSEQoJdGltZXN0YW1wGAQgASgEEg0KBWluZGV4GAUgASgEEh0K",
            "CXZhbGlkYXRvchgHIAEoCzIKLlB1YmxpY0tleRINCgVub25jZRgGIAEoBCKf",
            "AQoFQmxvY2sSHAoGaGVhZGVyGAEgASgLMgwuQmxvY2tIZWFkZXISFgoEaGFz",
            "aBgCIAEoCzIILlVJbnQyNTYSJAoSdHJhbnNhY3Rpb25faGFzaGVzGAQgAygL",
            "MgguVUludDI1NhIbCghtdWx0aXNpZxgDIAEoCzIJLk11bHRpU2lnEh0KC2F2",
            "ZXJhZ2VfZmVlGAUgASgLMgguVUludDI1NkIjChFjb20ubGF0b2tlbi5wcm90",
            "b6oCDVBob3JrdXMuUHJvdG9iBnByb3RvMw=="));
      descriptor = pbr::FileDescriptor.FromGeneratedCode(descriptorData,
          new pbr::FileDescriptor[] { global::Phorkus.Proto.DefaultReflection.Descriptor, global::Phorkus.Proto.MultisigReflection.Descriptor, },
          new pbr::GeneratedClrTypeInfo(null, new pbr::GeneratedClrTypeInfo[] {
            new pbr::GeneratedClrTypeInfo(typeof(global::Phorkus.Proto.BlockHeader), global::Phorkus.Proto.BlockHeader.Parser, new[]{ "Version", "PrevBlockHash", "MerkleRoot", "Timestamp", "Index", "Validator", "Nonce" }, null, null, null),
            new pbr::GeneratedClrTypeInfo(typeof(global::Phorkus.Proto.Block), global::Phorkus.Proto.Block.Parser, new[]{ "Header", "Hash", "TransactionHashes", "Multisig", "AverageFee" }, null, null, null)
          }));
    }
    #endregion

  }
  #region Messages
  public sealed partial class BlockHeader : pb::IMessage<BlockHeader> {
    private static readonly pb::MessageParser<BlockHeader> _parser = new pb::MessageParser<BlockHeader>(() => new BlockHeader());
    private pb::UnknownFieldSet _unknownFields;
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public static pb::MessageParser<BlockHeader> Parser { get { return _parser; } }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public static pbr::MessageDescriptor Descriptor {
      get { return global::Phorkus.Proto.BlockReflection.Descriptor.MessageTypes[0]; }
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    pbr::MessageDescriptor pb::IMessage.Descriptor {
      get { return Descriptor; }
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public BlockHeader() {
      OnConstruction();
    }

    partial void OnConstruction();

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public BlockHeader(BlockHeader other) : this() {
      version_ = other.version_;
      prevBlockHash_ = other.prevBlockHash_ != null ? other.prevBlockHash_.Clone() : null;
      merkleRoot_ = other.merkleRoot_ != null ? other.merkleRoot_.Clone() : null;
      timestamp_ = other.timestamp_;
      index_ = other.index_;
      validator_ = other.validator_ != null ? other.validator_.Clone() : null;
      nonce_ = other.nonce_;
      _unknownFields = pb::UnknownFieldSet.Clone(other._unknownFields);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public BlockHeader Clone() {
      return new BlockHeader(this);
    }

    /// <summary>Field number for the "version" field.</summary>
    public const int VersionFieldNumber = 1;
    private uint version_;
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public uint Version {
      get { return version_; }
      set {
        version_ = value;
      }
    }

    /// <summary>Field number for the "prev_block_hash" field.</summary>
    public const int PrevBlockHashFieldNumber = 2;
    private global::Phorkus.Proto.UInt256 prevBlockHash_;
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public global::Phorkus.Proto.UInt256 PrevBlockHash {
      get { return prevBlockHash_; }
      set {
        prevBlockHash_ = value;
      }
    }

    /// <summary>Field number for the "merkle_root" field.</summary>
    public const int MerkleRootFieldNumber = 3;
    private global::Phorkus.Proto.UInt256 merkleRoot_;
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public global::Phorkus.Proto.UInt256 MerkleRoot {
      get { return merkleRoot_; }
      set {
        merkleRoot_ = value;
      }
    }

    /// <summary>Field number for the "timestamp" field.</summary>
    public const int TimestampFieldNumber = 4;
    private ulong timestamp_;
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public ulong Timestamp {
      get { return timestamp_; }
      set {
        timestamp_ = value;
      }
    }

    /// <summary>Field number for the "index" field.</summary>
    public const int IndexFieldNumber = 5;
    private ulong index_;
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public ulong Index {
      get { return index_; }
      set {
        index_ = value;
      }
    }

    /// <summary>Field number for the "validator" field.</summary>
    public const int ValidatorFieldNumber = 7;
    private global::Phorkus.Proto.PublicKey validator_;
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public global::Phorkus.Proto.PublicKey Validator {
      get { return validator_; }
      set {
        validator_ = value;
      }
    }

    /// <summary>Field number for the "nonce" field.</summary>
    public const int NonceFieldNumber = 6;
    private ulong nonce_;
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public ulong Nonce {
      get { return nonce_; }
      set {
        nonce_ = value;
      }
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public override bool Equals(object other) {
      return Equals(other as BlockHeader);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public bool Equals(BlockHeader other) {
      if (ReferenceEquals(other, null)) {
        return false;
      }
      if (ReferenceEquals(other, this)) {
        return true;
      }
      if (Version != other.Version) return false;
      if (!object.Equals(PrevBlockHash, other.PrevBlockHash)) return false;
      if (!object.Equals(MerkleRoot, other.MerkleRoot)) return false;
      if (Timestamp != other.Timestamp) return false;
      if (Index != other.Index) return false;
      if (!object.Equals(Validator, other.Validator)) return false;
      if (Nonce != other.Nonce) return false;
      return Equals(_unknownFields, other._unknownFields);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public override int GetHashCode() {
      int hash = 1;
      if (Version != 0) hash ^= Version.GetHashCode();
      if (prevBlockHash_ != null) hash ^= PrevBlockHash.GetHashCode();
      if (merkleRoot_ != null) hash ^= MerkleRoot.GetHashCode();
      if (Timestamp != 0UL) hash ^= Timestamp.GetHashCode();
      if (Index != 0UL) hash ^= Index.GetHashCode();
      if (validator_ != null) hash ^= Validator.GetHashCode();
      if (Nonce != 0UL) hash ^= Nonce.GetHashCode();
      if (_unknownFields != null) {
        hash ^= _unknownFields.GetHashCode();
      }
      return hash;
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public override string ToString() {
      return pb::JsonFormatter.ToDiagnosticString(this);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public void WriteTo(pb::CodedOutputStream output) {
      if (Version != 0) {
        output.WriteRawTag(8);
        output.WriteUInt32(Version);
      }
      if (prevBlockHash_ != null) {
        output.WriteRawTag(18);
        output.WriteMessage(PrevBlockHash);
      }
      if (merkleRoot_ != null) {
        output.WriteRawTag(26);
        output.WriteMessage(MerkleRoot);
      }
      if (Timestamp != 0UL) {
        output.WriteRawTag(32);
        output.WriteUInt64(Timestamp);
      }
      if (Index != 0UL) {
        output.WriteRawTag(40);
        output.WriteUInt64(Index);
      }
      if (Nonce != 0UL) {
        output.WriteRawTag(48);
        output.WriteUInt64(Nonce);
      }
      if (validator_ != null) {
        output.WriteRawTag(58);
        output.WriteMessage(Validator);
      }
      if (_unknownFields != null) {
        _unknownFields.WriteTo(output);
      }
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public int CalculateSize() {
      int size = 0;
      if (Version != 0) {
        size += 1 + pb::CodedOutputStream.ComputeUInt32Size(Version);
      }
      if (prevBlockHash_ != null) {
        size += 1 + pb::CodedOutputStream.ComputeMessageSize(PrevBlockHash);
      }
      if (merkleRoot_ != null) {
        size += 1 + pb::CodedOutputStream.ComputeMessageSize(MerkleRoot);
      }
      if (Timestamp != 0UL) {
        size += 1 + pb::CodedOutputStream.ComputeUInt64Size(Timestamp);
      }
      if (Index != 0UL) {
        size += 1 + pb::CodedOutputStream.ComputeUInt64Size(Index);
      }
      if (validator_ != null) {
        size += 1 + pb::CodedOutputStream.ComputeMessageSize(Validator);
      }
      if (Nonce != 0UL) {
        size += 1 + pb::CodedOutputStream.ComputeUInt64Size(Nonce);
      }
      if (_unknownFields != null) {
        size += _unknownFields.CalculateSize();
      }
      return size;
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public void MergeFrom(BlockHeader other) {
      if (other == null) {
        return;
      }
      if (other.Version != 0) {
        Version = other.Version;
      }
      if (other.prevBlockHash_ != null) {
        if (prevBlockHash_ == null) {
          prevBlockHash_ = new global::Phorkus.Proto.UInt256();
        }
        PrevBlockHash.MergeFrom(other.PrevBlockHash);
      }
      if (other.merkleRoot_ != null) {
        if (merkleRoot_ == null) {
          merkleRoot_ = new global::Phorkus.Proto.UInt256();
        }
        MerkleRoot.MergeFrom(other.MerkleRoot);
      }
      if (other.Timestamp != 0UL) {
        Timestamp = other.Timestamp;
      }
      if (other.Index != 0UL) {
        Index = other.Index;
      }
      if (other.validator_ != null) {
        if (validator_ == null) {
          validator_ = new global::Phorkus.Proto.PublicKey();
        }
        Validator.MergeFrom(other.Validator);
      }
      if (other.Nonce != 0UL) {
        Nonce = other.Nonce;
      }
      _unknownFields = pb::UnknownFieldSet.MergeFrom(_unknownFields, other._unknownFields);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public void MergeFrom(pb::CodedInputStream input) {
      uint tag;
      while ((tag = input.ReadTag()) != 0) {
        switch(tag) {
          default:
            _unknownFields = pb::UnknownFieldSet.MergeFieldFrom(_unknownFields, input);
            break;
          case 8: {
            Version = input.ReadUInt32();
            break;
          }
          case 18: {
            if (prevBlockHash_ == null) {
              prevBlockHash_ = new global::Phorkus.Proto.UInt256();
            }
            input.ReadMessage(prevBlockHash_);
            break;
          }
          case 26: {
            if (merkleRoot_ == null) {
              merkleRoot_ = new global::Phorkus.Proto.UInt256();
            }
            input.ReadMessage(merkleRoot_);
            break;
          }
          case 32: {
            Timestamp = input.ReadUInt64();
            break;
          }
          case 40: {
            Index = input.ReadUInt64();
            break;
          }
          case 48: {
            Nonce = input.ReadUInt64();
            break;
          }
          case 58: {
            if (validator_ == null) {
              validator_ = new global::Phorkus.Proto.PublicKey();
            }
            input.ReadMessage(validator_);
            break;
          }
        }
      }
    }

  }

  public sealed partial class Block : pb::IMessage<Block> {
    private static readonly pb::MessageParser<Block> _parser = new pb::MessageParser<Block>(() => new Block());
    private pb::UnknownFieldSet _unknownFields;
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public static pb::MessageParser<Block> Parser { get { return _parser; } }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public static pbr::MessageDescriptor Descriptor {
      get { return global::Phorkus.Proto.BlockReflection.Descriptor.MessageTypes[1]; }
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    pbr::MessageDescriptor pb::IMessage.Descriptor {
      get { return Descriptor; }
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public Block() {
      OnConstruction();
    }

    partial void OnConstruction();

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public Block(Block other) : this() {
      header_ = other.header_ != null ? other.header_.Clone() : null;
      hash_ = other.hash_ != null ? other.hash_.Clone() : null;
      transactionHashes_ = other.transactionHashes_.Clone();
      multisig_ = other.multisig_ != null ? other.multisig_.Clone() : null;
      averageFee_ = other.averageFee_ != null ? other.averageFee_.Clone() : null;
      _unknownFields = pb::UnknownFieldSet.Clone(other._unknownFields);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public Block Clone() {
      return new Block(this);
    }

    /// <summary>Field number for the "header" field.</summary>
    public const int HeaderFieldNumber = 1;
    private global::Phorkus.Proto.BlockHeader header_;
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public global::Phorkus.Proto.BlockHeader Header {
      get { return header_; }
      set {
        header_ = value;
      }
    }

    /// <summary>Field number for the "hash" field.</summary>
    public const int HashFieldNumber = 2;
    private global::Phorkus.Proto.UInt256 hash_;
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public global::Phorkus.Proto.UInt256 Hash {
      get { return hash_; }
      set {
        hash_ = value;
      }
    }

    /// <summary>Field number for the "transaction_hashes" field.</summary>
    public const int TransactionHashesFieldNumber = 4;
    private static readonly pb::FieldCodec<global::Phorkus.Proto.UInt256> _repeated_transactionHashes_codec
        = pb::FieldCodec.ForMessage(34, global::Phorkus.Proto.UInt256.Parser);
    private readonly pbc::RepeatedField<global::Phorkus.Proto.UInt256> transactionHashes_ = new pbc::RepeatedField<global::Phorkus.Proto.UInt256>();
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public pbc::RepeatedField<global::Phorkus.Proto.UInt256> TransactionHashes {
      get { return transactionHashes_; }
    }

    /// <summary>Field number for the "multisig" field.</summary>
    public const int MultisigFieldNumber = 3;
    private global::Phorkus.Proto.MultiSig multisig_;
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public global::Phorkus.Proto.MultiSig Multisig {
      get { return multisig_; }
      set {
        multisig_ = value;
      }
    }

    /// <summary>Field number for the "average_fee" field.</summary>
    public const int AverageFeeFieldNumber = 5;
    private global::Phorkus.Proto.UInt256 averageFee_;
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public global::Phorkus.Proto.UInt256 AverageFee {
      get { return averageFee_; }
      set {
        averageFee_ = value;
      }
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public override bool Equals(object other) {
      return Equals(other as Block);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public bool Equals(Block other) {
      if (ReferenceEquals(other, null)) {
        return false;
      }
      if (ReferenceEquals(other, this)) {
        return true;
      }
      if (!object.Equals(Header, other.Header)) return false;
      if (!object.Equals(Hash, other.Hash)) return false;
      if(!transactionHashes_.Equals(other.transactionHashes_)) return false;
      if (!object.Equals(Multisig, other.Multisig)) return false;
      if (!object.Equals(AverageFee, other.AverageFee)) return false;
      return Equals(_unknownFields, other._unknownFields);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public override int GetHashCode() {
      int hash = 1;
      if (header_ != null) hash ^= Header.GetHashCode();
      if (hash_ != null) hash ^= Hash.GetHashCode();
      hash ^= transactionHashes_.GetHashCode();
      if (multisig_ != null) hash ^= Multisig.GetHashCode();
      if (averageFee_ != null) hash ^= AverageFee.GetHashCode();
      if (_unknownFields != null) {
        hash ^= _unknownFields.GetHashCode();
      }
      return hash;
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public override string ToString() {
      return pb::JsonFormatter.ToDiagnosticString(this);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public void WriteTo(pb::CodedOutputStream output) {
      if (header_ != null) {
        output.WriteRawTag(10);
        output.WriteMessage(Header);
      }
      if (hash_ != null) {
        output.WriteRawTag(18);
        output.WriteMessage(Hash);
      }
      if (multisig_ != null) {
        output.WriteRawTag(26);
        output.WriteMessage(Multisig);
      }
      transactionHashes_.WriteTo(output, _repeated_transactionHashes_codec);
      if (averageFee_ != null) {
        output.WriteRawTag(42);
        output.WriteMessage(AverageFee);
      }
      if (_unknownFields != null) {
        _unknownFields.WriteTo(output);
      }
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public int CalculateSize() {
      int size = 0;
      if (header_ != null) {
        size += 1 + pb::CodedOutputStream.ComputeMessageSize(Header);
      }
      if (hash_ != null) {
        size += 1 + pb::CodedOutputStream.ComputeMessageSize(Hash);
      }
      size += transactionHashes_.CalculateSize(_repeated_transactionHashes_codec);
      if (multisig_ != null) {
        size += 1 + pb::CodedOutputStream.ComputeMessageSize(Multisig);
      }
      if (averageFee_ != null) {
        size += 1 + pb::CodedOutputStream.ComputeMessageSize(AverageFee);
      }
      if (_unknownFields != null) {
        size += _unknownFields.CalculateSize();
      }
      return size;
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public void MergeFrom(Block other) {
      if (other == null) {
        return;
      }
      if (other.header_ != null) {
        if (header_ == null) {
          header_ = new global::Phorkus.Proto.BlockHeader();
        }
        Header.MergeFrom(other.Header);
      }
      if (other.hash_ != null) {
        if (hash_ == null) {
          hash_ = new global::Phorkus.Proto.UInt256();
        }
        Hash.MergeFrom(other.Hash);
      }
      transactionHashes_.Add(other.transactionHashes_);
      if (other.multisig_ != null) {
        if (multisig_ == null) {
          multisig_ = new global::Phorkus.Proto.MultiSig();
        }
        Multisig.MergeFrom(other.Multisig);
      }
      if (other.averageFee_ != null) {
        if (averageFee_ == null) {
          averageFee_ = new global::Phorkus.Proto.UInt256();
        }
        AverageFee.MergeFrom(other.AverageFee);
      }
      _unknownFields = pb::UnknownFieldSet.MergeFrom(_unknownFields, other._unknownFields);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public void MergeFrom(pb::CodedInputStream input) {
      uint tag;
      while ((tag = input.ReadTag()) != 0) {
        switch(tag) {
          default:
            _unknownFields = pb::UnknownFieldSet.MergeFieldFrom(_unknownFields, input);
            break;
          case 10: {
            if (header_ == null) {
              header_ = new global::Phorkus.Proto.BlockHeader();
            }
            input.ReadMessage(header_);
            break;
          }
          case 18: {
            if (hash_ == null) {
              hash_ = new global::Phorkus.Proto.UInt256();
            }
            input.ReadMessage(hash_);
            break;
          }
          case 26: {
            if (multisig_ == null) {
              multisig_ = new global::Phorkus.Proto.MultiSig();
            }
            input.ReadMessage(multisig_);
            break;
          }
          case 34: {
            transactionHashes_.AddEntriesFrom(input, _repeated_transactionHashes_codec);
            break;
          }
          case 42: {
            if (averageFee_ == null) {
              averageFee_ = new global::Phorkus.Proto.UInt256();
            }
            input.ReadMessage(averageFee_);
            break;
          }
        }
      }
    }

  }

  #endregion

}

#endregion Designer generated code
