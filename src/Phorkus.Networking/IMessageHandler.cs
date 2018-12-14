﻿using Phorkus.Proto;

namespace Phorkus.Networking
{
    public interface IMessageHandler
    {
         void GetBlocksByHashesRequest(MessageEnvelope envelope, GetBlocksByHashesRequest request);
         void GetBlocksByHashesReply(MessageEnvelope envelope, GetBlocksByHashesReply reply);
         void GetBlocksByHeightRangeRequest(MessageEnvelope envelope, GetBlocksByHeightRangeRequest request);
         void GetBlocksByHeightRangeReply(MessageEnvelope envelope, GetBlocksByHeightRangeReply reply);
         void GetTransactionsByHashesRequest(MessageEnvelope envelope, GetTransactionsByHashesRequest request);
         void GetTransactionsByHashesReply(MessageEnvelope envelope, GetTransactionsByHashesReply reply);
     }
 }