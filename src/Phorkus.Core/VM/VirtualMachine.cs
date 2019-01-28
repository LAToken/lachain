﻿using System;
using System.Collections.Generic;
using System.IO;
using Grpc.Core;
using Phorkus.Crypto;
using Phorkus.Proto;
using Phorkus.Storage.State;
using Phorkus.WebAssembly;

namespace Phorkus.Core.VM
{
    public class VirtualMachine : IVirtualMachine
    {
        public static Stack<ExecutionFrame> ExecutionFrames { get; } = new Stack<ExecutionFrame>();
        private static IStateManager StateManager { get; set; }
        public static IBlockchainSnapshot BlockchainSnapshot => StateManager.LastApprovedSnapshot;
        public static IBlockchainInterface BlockchainInterface { get; } = new DefaultBlockchainInterface();
        public static ICrypto Crypto { get; } = new BouncyCastle();

        public VirtualMachine(IStateManager stateManager)
        {
            StateManager = stateManager;
        }

        // TODO: protection from multiple instantiation

        public bool VerifyContract(Contract contract)
        {
            var contractCode = contract.Wasm.ToByteArray();
            try
            {
                using (var stream = new MemoryStream(contractCode))
                    Compile.FromBinary<dynamic>(stream);
                return true;
            }
            catch (Exception e)
            {
                Console.Error.WriteLine(e);
            }

            return false;
        }

        public ExecutionStatus InvokeContract(Contract contract, Invocation invocation)
        {
            try
            {
                return _InvokeContractUnsafe(contract, invocation);
            }
            catch (Exception e)
            {
                Console.Error.WriteLine(e);
                return ExecutionStatus.UnknownError;
            }
        }

        private static ExecutionStatus _InvokeContractUnsafe(Contract contract, Invocation invocation)
        {
            if (ExecutionFrames.Count != 0)
                return ExecutionStatus.VmCorruption;
            if (contract.Version != ContractVersion.Wasm)
                return ExecutionStatus.IncompatibleCode;
            
            var status = ExecutionFrame.FromInvocation(contract.Wasm.ToByteArray(), invocation, BlockchainInterface, out var rootFrame);
            if (status != ExecutionStatus.Ok) return status;
            ExecutionFrames.Push(rootFrame);
            return rootFrame.Execute();
        }
    }
}