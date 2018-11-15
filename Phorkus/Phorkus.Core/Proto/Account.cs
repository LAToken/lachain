// <auto-generated>
//     Generated by the protocol buffer compiler.  DO NOT EDIT!
//     source: account.proto
// </auto-generated>
#pragma warning disable 1591, 0612, 3021
#region Designer generated code

using pb = global::Google.Protobuf;
using pbc = global::Google.Protobuf.Collections;
using pbr = global::Google.Protobuf.Reflection;
using scg = global::System.Collections.Generic;
namespace Phorkus.Core.Proto {

  /// <summary>Holder for reflection information generated from account.proto</summary>
  public static partial class AccountReflection {

    #region Descriptor
    /// <summary>File descriptor for account.proto</summary>
    public static pbr::FileDescriptor Descriptor {
      get { return descriptor; }
    }
    private static pbr::FileDescriptor descriptor;

    static AccountReflection() {
      byte[] descriptorData = global::System.Convert.FromBase64String(
          string.Concat(
            "Cg1hY2NvdW50LnByb3RvGg1kZWZhdWx0LnByb3RvIkIKB0FjY291bnQSGQoH",
            "YWRkcmVzcxgBIAEoCzIILlVJbnQxNjASHAoFc3RhdGUYAiABKA4yDS5BY2Nv",
            "dW50U3RhdGUqQgoMQWNjb3VudFN0YXRlEhgKFEFDQ09VTlRfU1RBVEVfQUNU",
            "SVZFEAASGAoUQUNDT1VOVF9TVEFURV9GUk9aRU4QAUIVqgISUGhvcmt1cy5D",
            "b3JlLlByb3RvYgZwcm90bzM="));
      descriptor = pbr::FileDescriptor.FromGeneratedCode(descriptorData,
          new pbr::FileDescriptor[] { global::Phorkus.Core.Proto.DefaultReflection.Descriptor, },
          new pbr::GeneratedClrTypeInfo(new[] {typeof(global::Phorkus.Core.Proto.AccountState), }, new pbr::GeneratedClrTypeInfo[] {
            new pbr::GeneratedClrTypeInfo(typeof(global::Phorkus.Core.Proto.Account), global::Phorkus.Core.Proto.Account.Parser, new[]{ "Address", "State" }, null, null, null)
          }));
    }
    #endregion

  }
  #region Enums
  public enum AccountState {
    [pbr::OriginalName("ACCOUNT_STATE_ACTIVE")] Active = 0,
    [pbr::OriginalName("ACCOUNT_STATE_FROZEN")] Frozen = 1,
  }

  #endregion

  #region Messages
  public sealed partial class Account : pb::IMessage<Account> {
    private static readonly pb::MessageParser<Account> _parser = new pb::MessageParser<Account>(() => new Account());
    private pb::UnknownFieldSet _unknownFields;
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public static pb::MessageParser<Account> Parser { get { return _parser; } }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public static pbr::MessageDescriptor Descriptor {
      get { return global::Phorkus.Core.Proto.AccountReflection.Descriptor.MessageTypes[0]; }
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    pbr::MessageDescriptor pb::IMessage.Descriptor {
      get { return Descriptor; }
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public Account() {
      OnConstruction();
    }

    partial void OnConstruction();

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public Account(Account other) : this() {
      address_ = other.address_ != null ? other.address_.Clone() : null;
      state_ = other.state_;
      _unknownFields = pb::UnknownFieldSet.Clone(other._unknownFields);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public Account Clone() {
      return new Account(this);
    }

    /// <summary>Field number for the "address" field.</summary>
    public const int AddressFieldNumber = 1;
    private global::Phorkus.Core.Proto.UInt160 address_;
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public global::Phorkus.Core.Proto.UInt160 Address {
      get { return address_; }
      set {
        address_ = value;
      }
    }

    /// <summary>Field number for the "state" field.</summary>
    public const int StateFieldNumber = 2;
    private global::Phorkus.Core.Proto.AccountState state_ = 0;
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public global::Phorkus.Core.Proto.AccountState State {
      get { return state_; }
      set {
        state_ = value;
      }
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public override bool Equals(object other) {
      return Equals(other as Account);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public bool Equals(Account other) {
      if (ReferenceEquals(other, null)) {
        return false;
      }
      if (ReferenceEquals(other, this)) {
        return true;
      }
      if (!object.Equals(Address, other.Address)) return false;
      if (State != other.State) return false;
      return Equals(_unknownFields, other._unknownFields);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public override int GetHashCode() {
      int hash = 1;
      if (address_ != null) hash ^= Address.GetHashCode();
      if (State != 0) hash ^= State.GetHashCode();
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
      if (State != 0) {
        output.WriteRawTag(16);
        output.WriteEnum((int) State);
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
      if (State != 0) {
        size += 1 + pb::CodedOutputStream.ComputeEnumSize((int) State);
      }
      if (_unknownFields != null) {
        size += _unknownFields.CalculateSize();
      }
      return size;
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public void MergeFrom(Account other) {
      if (other == null) {
        return;
      }
      if (other.address_ != null) {
        if (address_ == null) {
          address_ = new global::Phorkus.Core.Proto.UInt160();
        }
        Address.MergeFrom(other.Address);
      }
      if (other.State != 0) {
        State = other.State;
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
              address_ = new global::Phorkus.Core.Proto.UInt160();
            }
            input.ReadMessage(address_);
            break;
          }
          case 16: {
            state_ = (global::Phorkus.Core.Proto.AccountState) input.ReadEnum();
            break;
          }
        }
      }
    }

  }

  #endregion

}

#endregion Designer generated code