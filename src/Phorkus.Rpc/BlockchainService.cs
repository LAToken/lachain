// <auto-generated>
//     Generated by the protocol buffer compiler.  DO NOT EDIT!
//     source: blockchain_service.proto
// </auto-generated>
#pragma warning disable 1591, 0612, 3021
#region Designer generated code

using pb = global::Google.Protobuf;
using pbc = global::Google.Protobuf.Collections;
using pbr = global::Google.Protobuf.Reflection;
using scg = global::System.Collections.Generic;
namespace Phorkus.Rpc {

  /// <summary>Holder for reflection information generated from blockchain_service.proto</summary>
  public static partial class BlockchainServiceReflection {

    #region Descriptor
    /// <summary>File descriptor for blockchain_service.proto</summary>
    public static pbr::FileDescriptor Descriptor {
      get { return descriptor; }
    }
    private static pbr::FileDescriptor descriptor;

    static BlockchainServiceReflection() {
      byte[] descriptorData = global::System.Convert.FromBase64String(
          string.Concat(
            "ChhibG9ja2NoYWluX3NlcnZpY2UucHJvdG8aC2Jsb2NrLnByb3RvIi8KF0dl",
            "dEJsb2NrQnlIZWlnaHRSZXF1ZXN0EhQKDGJsb2NrX2hlaWdodBgBIAEoBCIu",
            "ChVHZXRCbG9ja0J5SGVpZ2h0UmVwbHkSFQoFYmxvY2sYASABKAsyBi5CbG9j",
            "azJZChFCbG9ja2NoYWluU2VydmljZRJEChBHZXRCbG9ja0J5SGVpZ2h0Ehgu",
            "R2V0QmxvY2tCeUhlaWdodFJlcXVlc3QaFi5HZXRCbG9ja0J5SGVpZ2h0UmVw",
            "bHlCDqoCC1Bob3JrdXMuUnBjYgZwcm90bzM="));
      descriptor = pbr::FileDescriptor.FromGeneratedCode(descriptorData,
          new pbr::FileDescriptor[] { global::Phorkus.Proto.BlockReflection.Descriptor, },
          new pbr::GeneratedClrTypeInfo(null, new pbr::GeneratedClrTypeInfo[] {
            new pbr::GeneratedClrTypeInfo(typeof(global::Phorkus.Rpc.GetBlockByHeightRequest), global::Phorkus.Rpc.GetBlockByHeightRequest.Parser, new[]{ "BlockHeight" }, null, null, null),
            new pbr::GeneratedClrTypeInfo(typeof(global::Phorkus.Rpc.GetBlockByHeightReply), global::Phorkus.Rpc.GetBlockByHeightReply.Parser, new[]{ "Block" }, null, null, null)
          }));
    }
    #endregion

  }
  #region Messages
  public sealed partial class GetBlockByHeightRequest : pb::IMessage<GetBlockByHeightRequest> {
    private static readonly pb::MessageParser<GetBlockByHeightRequest> _parser = new pb::MessageParser<GetBlockByHeightRequest>(() => new GetBlockByHeightRequest());
    private pb::UnknownFieldSet _unknownFields;
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public static pb::MessageParser<GetBlockByHeightRequest> Parser { get { return _parser; } }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public static pbr::MessageDescriptor Descriptor {
      get { return global::Phorkus.Rpc.BlockchainServiceReflection.Descriptor.MessageTypes[0]; }
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    pbr::MessageDescriptor pb::IMessage.Descriptor {
      get { return Descriptor; }
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public GetBlockByHeightRequest() {
      OnConstruction();
    }

    partial void OnConstruction();

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public GetBlockByHeightRequest(GetBlockByHeightRequest other) : this() {
      blockHeight_ = other.blockHeight_;
      _unknownFields = pb::UnknownFieldSet.Clone(other._unknownFields);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public GetBlockByHeightRequest Clone() {
      return new GetBlockByHeightRequest(this);
    }

    /// <summary>Field number for the "block_height" field.</summary>
    public const int BlockHeightFieldNumber = 1;
    private ulong blockHeight_;
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public ulong BlockHeight {
      get { return blockHeight_; }
      set {
        blockHeight_ = value;
      }
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public override bool Equals(object other) {
      return Equals(other as GetBlockByHeightRequest);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public bool Equals(GetBlockByHeightRequest other) {
      if (ReferenceEquals(other, null)) {
        return false;
      }
      if (ReferenceEquals(other, this)) {
        return true;
      }
      if (BlockHeight != other.BlockHeight) return false;
      return Equals(_unknownFields, other._unknownFields);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public override int GetHashCode() {
      int hash = 1;
      if (BlockHeight != 0UL) hash ^= BlockHeight.GetHashCode();
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
      if (BlockHeight != 0UL) {
        output.WriteRawTag(8);
        output.WriteUInt64(BlockHeight);
      }
      if (_unknownFields != null) {
        _unknownFields.WriteTo(output);
      }
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public int CalculateSize() {
      int size = 0;
      if (BlockHeight != 0UL) {
        size += 1 + pb::CodedOutputStream.ComputeUInt64Size(BlockHeight);
      }
      if (_unknownFields != null) {
        size += _unknownFields.CalculateSize();
      }
      return size;
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public void MergeFrom(GetBlockByHeightRequest other) {
      if (other == null) {
        return;
      }
      if (other.BlockHeight != 0UL) {
        BlockHeight = other.BlockHeight;
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
            BlockHeight = input.ReadUInt64();
            break;
          }
        }
      }
    }

  }

  public sealed partial class GetBlockByHeightReply : pb::IMessage<GetBlockByHeightReply> {
    private static readonly pb::MessageParser<GetBlockByHeightReply> _parser = new pb::MessageParser<GetBlockByHeightReply>(() => new GetBlockByHeightReply());
    private pb::UnknownFieldSet _unknownFields;
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public static pb::MessageParser<GetBlockByHeightReply> Parser { get { return _parser; } }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public static pbr::MessageDescriptor Descriptor {
      get { return global::Phorkus.Rpc.BlockchainServiceReflection.Descriptor.MessageTypes[1]; }
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    pbr::MessageDescriptor pb::IMessage.Descriptor {
      get { return Descriptor; }
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public GetBlockByHeightReply() {
      OnConstruction();
    }

    partial void OnConstruction();

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public GetBlockByHeightReply(GetBlockByHeightReply other) : this() {
      block_ = other.block_ != null ? other.block_.Clone() : null;
      _unknownFields = pb::UnknownFieldSet.Clone(other._unknownFields);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public GetBlockByHeightReply Clone() {
      return new GetBlockByHeightReply(this);
    }

    /// <summary>Field number for the "block" field.</summary>
    public const int BlockFieldNumber = 1;
    private global::Phorkus.Proto.Block block_;
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public global::Phorkus.Proto.Block Block {
      get { return block_; }
      set {
        block_ = value;
      }
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public override bool Equals(object other) {
      return Equals(other as GetBlockByHeightReply);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public bool Equals(GetBlockByHeightReply other) {
      if (ReferenceEquals(other, null)) {
        return false;
      }
      if (ReferenceEquals(other, this)) {
        return true;
      }
      if (!object.Equals(Block, other.Block)) return false;
      return Equals(_unknownFields, other._unknownFields);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public override int GetHashCode() {
      int hash = 1;
      if (block_ != null) hash ^= Block.GetHashCode();
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
      if (block_ != null) {
        output.WriteRawTag(10);
        output.WriteMessage(Block);
      }
      if (_unknownFields != null) {
        _unknownFields.WriteTo(output);
      }
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public int CalculateSize() {
      int size = 0;
      if (block_ != null) {
        size += 1 + pb::CodedOutputStream.ComputeMessageSize(Block);
      }
      if (_unknownFields != null) {
        size += _unknownFields.CalculateSize();
      }
      return size;
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public void MergeFrom(GetBlockByHeightReply other) {
      if (other == null) {
        return;
      }
      if (other.block_ != null) {
        if (block_ == null) {
          block_ = new global::Phorkus.Proto.Block();
        }
        Block.MergeFrom(other.Block);
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
            if (block_ == null) {
              block_ = new global::Phorkus.Proto.Block();
            }
            input.ReadMessage(block_);
            break;
          }
        }
      }
    }

  }

  #endregion

}

#endregion Designer generated code
