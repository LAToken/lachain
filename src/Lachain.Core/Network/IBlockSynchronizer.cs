﻿using System;
using System.Collections.Generic;
using Lachain.Networking;
using Lachain.Proto;

namespace Lachain.Core.Network
{
    public interface IBlockSynchronizer
    {
        uint WaitForTransactions(IEnumerable<UInt256> transactionHashes, TimeSpan timeout);

        uint HandleTransactionsFromPeer(IEnumerable<TransactionReceipt> transactions, IRemotePeer remotePeer);
        
        void HandleBlockFromPeer(Block block, IRemotePeer remotePeer, TimeSpan timeout);

        void HandlePeerHasBlocks(ulong blockHeight, IRemotePeer remotePeer);

        bool IsSynchronizingWith(IEnumerable<ECDSAPublicKey> peers);

        PeerAddress[] GetConnectedPeers();

        ulong? GetHighestBlock();

        void SynchronizeWith(IEnumerable<ECDSAPublicKey> peers);

        void Start();
    }
}