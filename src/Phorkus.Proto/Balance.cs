// <auto-generated>
//     Generated by the protocol buffer compiler.  DO NOT EDIT!
//     source: balance.proto
// </auto-generated>
#pragma warning disable 1591, 0612, 3021
#region Designer generated code

using pb = global::Google.Protobuf;
using pbc = global::Google.Protobuf.Collections;
using pbr = global::Google.Protobuf.Reflection;
using scg = global::System.Collections.Generic;
namespace Phorkus.Proto {

  /// <summary>Holder for reflection information generated from balance.proto</summary>
  public static partial class BalanceReflection {

    #region Descriptor
    /// <summary>File descriptor for balance.proto</summary>
    public static pbr::FileDescriptor Descriptor {
      get { return descriptor; }
    }
    private static pbr::FileDescriptor descriptor;

    static BalanceReflection() {
      byte[] descriptorData = global::System.Convert.FromBase64String(
          string.Concat(
            "Cg1iYWxhbmNlLnByb3RvGg1kZWZhdWx0LnByb3RvInkKB0JhbGFuY2USGQoH",
            "YWRkcmVzcxgBIAEoCzIILlVJbnQxNjASFwoFYXNzZXQYAiABKAsyCC5VSW50",
            "MTYwEhsKCWF2YWlsYWJsZRgDIAEoCzIILlVJbnQyNTYSHQoLd2l0aGRyYXdp",
            "bmcYBCABKAsyCC5VSW50MjU2QiMKEWNvbS5sYXRva2VuLnByb3RvqgINUGhv",
            "cmt1cy5Qcm90b2IGcHJvdG8z"));
      descriptor = pbr::FileDescriptor.FromGeneratedCode(descriptorData,
          new pbr::FileDescriptor[] { global::Phorkus.Proto.DefaultReflection.Descriptor, },
          new pbr::GeneratedClrTypeInfo(null, new pbr::GeneratedClrTypeInfo[] {
            new pbr::GeneratedClrTypeInfo(typeof(global::Phorkus.Proto.Balance), global::Phorkus.Proto.Balance.Parser, new[]{ "Address", "Asset", "Available", "Withdrawing" }, null, null, null)
          }));
    }
    #endregion

  }
  #region Messages
  public sealed partial class Balance : pb::IMessage<Balance> {
    private static readonly pb::MessageParser<Balance> _parser = new pb::MessageParser<Balance>(() => new Balance());
    private pb::UnknownFieldSet _unknownFields;
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public static pb::MessageParser<Balance> Parser { get { return _parser; } }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public static pbr::MessageDescriptor Descriptor {
      get { return global::Phorkus.Proto.BalanceReflection.Descriptor.MessageTypes[0]; }
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    pbr::MessageDescriptor pb::IMessage.Descriptor {
      get { return Descriptor; }
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public Balance() {
      OnConstruction();
    }

    partial void OnConstruction();

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public Balance(Balance other) : this() {
      address_ = other.address_ != null ? other.address_.Clone() : null;
      asset_ = other.asset_ != null ? other.asset_.Clone() : null;
      available_ = other.available_ != null ? other.available_.Clone() : null;
      withdrawing_ = other.withdrawing_ != null ? other.withdrawing_.Clone() : null;
      _unknownFields = pb::UnknownFieldSet.Clone(other._unknownFields);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public Balance Clone() {
      return new Balance(this);
    }

    /// <summary>Field number for the "address" field.</summary>
    public const int AddressFieldNumber = 1;
    private global::Phorkus.Proto.UInt160 address_;
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public global::Phorkus.Proto.UInt160 Address {
      get { return address_; }
      set {
        address_ = value;
      }
    }

    /// <summary>Field number for the "asset" field.</summary>
    public const int AssetFieldNumber = 2;
    private global::Phorkus.Proto.UInt160 asset_;
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public global::Phorkus.Proto.UInt160 Asset {
      get { return asset_; }
      set {
        asset_ = value;
      }
    }

    /// <summary>Field number for the "available" field.</summary>
    public const int AvailableFieldNumber = 3;
    private global::Phorkus.Proto.UInt256 available_;
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public global::Phorkus.Proto.UInt256 Available {
      get { return available_; }
      set {
        available_ = value;
      }
    }

    /// <summary>Field number for the "withdrawing" field.</summary>
    public const int WithdrawingFieldNumber = 4;
    private global::Phorkus.Proto.UInt256 withdrawing_;
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public global::Phorkus.Proto.UInt256 Withdrawing {
      get { return withdrawing_; }
      set {
        withdrawing_ = value;
      }
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public override bool Equals(object other) {
      return Equals(other as Balance);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public bool Equals(Balance other) {
      if (ReferenceEquals(other, null)) {
        return false;
      }
      if (ReferenceEquals(other, this)) {
        return true;
      }
      if (!object.Equals(Address, other.Address)) return false;
      if (!object.Equals(Asset, other.Asset)) return false;
      if (!object.Equals(Available, other.Available)) return false;
      if (!object.Equals(Withdrawing, other.Withdrawing)) return false;
      return Equals(_unknownFields, other._unknownFields);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public override int GetHashCode() {
      int hash = 1;
      if (address_ != null) hash ^= Address.GetHashCode();
      if (asset_ != null) hash ^= Asset.GetHashCode();
      if (available_ != null) hash ^= Available.GetHashCode();
      if (withdrawing_ != null) hash ^= Withdrawing.GetHashCode();
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
      if (address_ != null) {
        output.WriteRawTag(10);
        output.WriteMessage(Address);
      }
      if (asset_ != null) {
        output.WriteRawTag(18);
        output.WriteMessage(Asset);
      }
      if (available_ != null) {
        output.WriteRawTag(26);
        output.WriteMessage(Available);
      }
      if (withdrawing_ != null) {
        output.WriteRawTag(34);
        output.WriteMessage(Withdrawing);
      }
      if (_unknownFields != null) {
        _unknownFields.WriteTo(output);
      }
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public int CalculateSize() {
      int size = 0;
      if (address_ != null) {
        size += 1 + pb::CodedOutputStream.ComputeMessageSize(Address);
      }
      if (asset_ != null) {
        size += 1 + pb::CodedOutputStream.ComputeMessageSize(Asset);
      }
      if (available_ != null) {
        size += 1 + pb::CodedOutputStream.ComputeMessageSize(Available);
      }
      if (withdrawing_ != null) {
        size += 1 + pb::CodedOutputStream.ComputeMessageSize(Withdrawing);
      }
      if (_unknownFields != null) {
        size += _unknownFields.CalculateSize();
      }
      return size;
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public void MergeFrom(Balance other) {
      if (other == null) {
        return;
      }
      if (other.address_ != null) {
        if (address_ == null) {
          address_ = new global::Phorkus.Proto.UInt160();
        }
        Address.MergeFrom(other.Address);
      }
      if (other.asset_ != null) {
        if (asset_ == null) {
          asset_ = new global::Phorkus.Proto.UInt160();
        }
        Asset.MergeFrom(other.Asset);
      }
      if (other.available_ != null) {
        if (available_ == null) {
          available_ = new global::Phorkus.Proto.UInt256();
        }
        Available.MergeFrom(other.Available);
      }
      if (other.withdrawing_ != null) {
        if (withdrawing_ == null) {
          withdrawing_ = new global::Phorkus.Proto.UInt256();
        }
        Withdrawing.MergeFrom(other.Withdrawing);
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
            if (address_ == null) {
              address_ = new global::Phorkus.Proto.UInt160();
            }
            input.ReadMessage(address_);
            break;
          }
          case 18: {
            if (asset_ == null) {
              asset_ = new global::Phorkus.Proto.UInt160();
            }
            input.ReadMessage(asset_);
            break;
          }
          case 26: {
            if (available_ == null) {
              available_ = new global::Phorkus.Proto.UInt256();
            }
            input.ReadMessage(available_);
            break;
          }
          case 34: {
            if (withdrawing_ == null) {
              withdrawing_ = new global::Phorkus.Proto.UInt256();
            }
            input.ReadMessage(withdrawing_);
            break;
          }
        }
      }
    }

  }

  #endregion

}

#endregion Designer generated code
