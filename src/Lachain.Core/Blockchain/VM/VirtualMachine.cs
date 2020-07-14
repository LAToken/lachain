﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using Lachain.Core.Blockchain.Error;
using Lachain.Core.Blockchain.Interface;
using Lachain.Core.Blockchain.SystemContracts.ContractManager;
using Lachain.Core.Blockchain.VM.ExecutionFrame;
using Lachain.Crypto;
using Lachain.Logger;
using Lachain.Proto;
using WebAssembly.Runtime;

namespace Lachain.Core.Blockchain.VM
{
    public class VirtualMachine : IVirtualMachine
    {
        internal static Stack<IExecutionFrame> ExecutionFrames { get; } = new Stack<IExecutionFrame>();

        internal static IBlockchainInterface BlockchainInterface { get; } = new BlockchainInterface();

        internal static readonly ICrypto Crypto = CryptoProvider.GetCrypto();

        private static readonly ILogger<VirtualMachine> Logger = LoggerFactory.GetLoggerForClass<VirtualMachine>();

        [MethodImpl(MethodImplOptions.Synchronized)]
        public static bool VerifyContract(byte[] contractCode)
        {
            try
            {
                var config = new CompilerConfiguration();
                using var stream = new MemoryStream(contractCode);
                Compile.FromBinary<dynamic>(stream, config)(BlockchainInterface.GetFunctionImports());
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public static InvocationResult InvokeSystemContract(
            SystemContractCall systemContractCall, InvocationContext context, byte[] input, ulong gasLimit
        )
        {
            var status = FrameFactory.FromSystemContractCall(
                systemContractCall,
                context,
                input,
                out var rootFrame,
                gasLimit
            );
            return status == ExecutionStatus.Ok ? ExecuteFrame(rootFrame) : InvocationResult.WithStatus(status);
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        public static InvocationResult InvokeWasmContract(
            Contract contract, InvocationContext context, byte[] input, ulong gasLimit
        )
        {
            var status = FrameFactory.FromInvocation(
                contract.ByteCode.ToByteArray(),
                context,
                contract.ContractAddress,
                input,
                BlockchainInterface,
                out var rootFrame,
                gasLimit
            );
            return status == ExecutionStatus.Ok ? ExecuteFrame(rootFrame) : InvocationResult.WithStatus(status);
        }

        private static InvocationResult ExecuteFrame(IExecutionFrame frame)
        {
            var result = new InvocationResult();
            ExecutionFrames.Push(frame);
            try
            {
                result.Status = frame.Execute();
                if (result.Status == ExecutionStatus.Ok)
                    result.ReturnValue = frame.ReturnValue;
                result.GasUsed = frame.GasUsed;
            }
            catch (OutOfGasException e)
            {
                result.GasUsed = e.GasUsed;
                result.Status = ExecutionStatus.GasOverflow;
            }
            catch (HaltException e)
            {
                result.Status = ExecutionStatus.ExecutionHalted;
                result.ReturnValue = new[] {(byte) e.HaltCode};
                result.GasUsed = frame.GasUsed;
            }
            catch (Exception e)
            {
                Logger.LogError($"Unknown exception in VM: {e}");
                result.Status = ExecutionStatus.UnknownError;
                result.GasUsed = frame.GasUsed;
            }

            ExecutionFrames.Pop();
            return result;
        }
    }
}