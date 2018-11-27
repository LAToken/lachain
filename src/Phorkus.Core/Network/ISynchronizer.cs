﻿using System;
using System.Collections.Generic;
using Phorkus.Proto;

namespace Phorkus.Core.Network
{
    public interface ISynchronizer
    {
        uint HandleTransactionsFromPeer(IEnumerable<SignedTransaction> transactions, IRemotePeer remotePeer);
        
        uint WaitForTransactions(IEnumerable<UInt256> transactionHashes, TimeSpan timeout);
        
        void HandleBlockFromPeer(Block block, IRemotePeer remotePeer);
        
        void Start();
    }
}