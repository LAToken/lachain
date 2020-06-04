using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using Google.Protobuf;
using Lachain.Consensus.Messages;
using Lachain.Crypto;
using Lachain.Crypto.TPKE;
using Lachain.Logger;
using Lachain.Proto;
using Lachain.Utility.Serialization;
using Lachain.Utility.Utils;

namespace Lachain.Consensus.ReliableBroadcast
{
    public class ReliableBroadcast : AbstractProtocol
    {
        private static readonly ILogger<ReliableBroadcast>
            Logger = LoggerFactory.GetLoggerForClass<ReliableBroadcast>();

        private readonly ReliableBroadcastId _broadcastId;

        private ResultStatus _requested;

        private readonly ECHOMessage?[] _echoMessages;
        private readonly ReadyMessage?[] _readyMessages;
        private readonly bool[] _sentValMessage;
        private readonly int _merkleTreeSize;
        private bool _readySent;

        public ReliableBroadcast(
            ReliableBroadcastId broadcastId, IPublicConsensusKeySet wallet, IConsensusBroadcaster broadcaster) :
            base(wallet, broadcastId, broadcaster)
        {
            _broadcastId = broadcastId;
            _echoMessages = new ECHOMessage?[N];
            _readyMessages = new ReadyMessage?[N];
            _sentValMessage = new bool[N];
            _requested = ResultStatus.NotRequested;
            _merkleTreeSize = N;
            while ((_merkleTreeSize & (_merkleTreeSize - 1)) != 0)
                _merkleTreeSize++; // increment while not power of two
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        public override void ProcessMessage(MessageEnvelope envelope)
        {
            if (envelope.External)
            {
                var message = envelope.ExternalMessage ?? throw new InvalidOperationException();
                // Logger.LogError(
                //     $"{Id} processing message:x {message.PayloadCase}, thread = {Thread.CurrentThread.ManagedThreadId}");

                switch (message.PayloadCase)
                {
                    case ConsensusMessage.PayloadOneofCase.ValMessage:
                        HandleValMessage(message.ValMessage, envelope.ValidatorIndex);
                        break;
                    case ConsensusMessage.PayloadOneofCase.EchoMessage:
                        HandleEchoMessage(message.EchoMessage, envelope.ValidatorIndex);
                        break;
                    case ConsensusMessage.PayloadOneofCase.ReadyMessage:
                        HandleReadyMessage(message.ReadyMessage, envelope.ValidatorIndex);
                        break;
                    default:
                        throw new ArgumentException(
                            $"consensus message of type {message.PayloadCase} routed to ReliableBroadcast protocol"
                        );
                }
            }
            else
            {
                var message = envelope.InternalMessage ?? throw new InvalidOperationException();
                // Logger.LogError(
                //     $"{Id} processing message: {message.GetType()}, thread = {Thread.CurrentThread.ManagedThreadId}");
                switch (message)
                {
                    case ProtocolRequest<ReliableBroadcastId, EncryptedShare> broadcastRequested:
                        HandleInputMessage(broadcastRequested);
                        break;
                    case ProtocolResult<ReliableBroadcastId, EncryptedShare> _:
                        Terminate();
                        break;
                    default:
                        throw new InvalidOperationException(
                            $"RBC protocol does not handle message of type {message.GetType()}");
                }
            }
        }

        private void HandleInputMessage(ProtocolRequest<ReliableBroadcastId, EncryptedShare> broadcastRequested)
        {
            Logger.LogInformation($"{Id}: got request with input, empty={broadcastRequested.Input == null}");
            _requested = ResultStatus.Requested;
            if (N == 1)
            {
                if (broadcastRequested.Input is null) throw new InvalidOperationException();
                Broadcaster.InternalResponse(
                    new ProtocolResult<ReliableBroadcastId, EncryptedShare>(_broadcastId, broadcastRequested.Input)
                );
                return;
            }

            if (broadcastRequested.Input == null)
            {
                CheckResult();
                return;
            }

            var input = broadcastRequested.Input.ToBytes().ToList();
            AugmentInput(input);
            foreach (var (valMessage, i) in ConstructValMessages(input).WithIndex())
                Broadcaster.SendToValidator(new ConsensusMessage {ValMessage = valMessage}, i);

            // Logger.LogDebug($"{Id} finished sending VAL messages");
            CheckResult();
        }

        private void HandleValMessage(ValMessage val, int validator)
        {
            if (_sentValMessage[validator])
            {
                Logger.LogWarning($"{Id}: validator {validator} tried to send VAL message twice");
                return;
            }

            _sentValMessage[validator] = true;
            Broadcaster.Broadcast(CreateEchoMessage(val));
            // Logger.LogDebug($"{Id}, player {GetMyId()} broadcast ECHO");
        }

        private void HandleEchoMessage(ECHOMessage echo, int validator)
        {
            // Logger.LogDebug($"{Id} got ECHO from {validator}");
            if (_echoMessages[validator] != null)
            {
                Logger.LogWarning($"{Id} already received correct echo from {validator}");
                return;
            }

            if (!CheckEchoMessage(echo, validator))
            {
                Logger.LogWarning($"{Id}: validator {validator} sent incorrect ECHO");
                return;
            }

            _echoMessages[validator] = echo;
            TrySendReadyMessageFromEchos();
            CheckResult();
        }

        private void HandleReadyMessage(ReadyMessage readyMessage, int validator)
        {
            if (_readyMessages[validator] != null)
            {
                Logger.LogWarning($"{Id} received duplicate ready from validator {validator}");
                return;
            }

            _readyMessages[validator] = readyMessage;
            TrySendReadyMessageFromReady();
            CheckResult();
            // Logger.LogDebug($"{Id}: got ready message from {validator}");
        }

        private void TrySendReadyMessageFromEchos()
        {
            if (_readySent) return;
            var (bestRootCnt, bestRoot) = _echoMessages
                .Where(x => x != null)
                .GroupBy(x => x!.MerkleTreeRoot)
                .Select(m => (cnt: m.Count(), key: m.Key))
                .OrderByDescending(x => x.cnt)
                .First();
            if (bestRootCnt != N - F) return;
            // Logger.LogDebug($"{Id} got {N - F} ECHO messages, interpolating result to send READY");
            var interpolationData = _echoMessages
                .WithIndex()
                .Where(x => bestRoot.Equals(x.item?.MerkleTreeRoot))
                .Select(t => (echo: t.item!, t.index))
                .Take(N - 2 * F)
                .ToArray();
            var restoredData = DecodeFromEchos(interpolationData);
            var restoredMerkleTree = ConstructMerkleTree(
                restoredData
                    .Batch(interpolationData.First().echo.Data.Length)
                    .Select(x => x.ToArray())
                    .ToArray()
            );
            if (!restoredMerkleTree[1].Equals(bestRoot))
            {
                Logger.LogError($"{Id}: Interpolated result merkle root does not match!");
                Abort();
                return;
            }

            Broadcaster.Broadcast(CreateReadyMessage(bestRoot));
            _readySent = true;
            // Logger.LogDebug($"{Id} broadcast ReadyMessage from HandleEchoMessage()");
        }

        private void TrySendReadyMessageFromReady()
        {
            var (bestRootCnt, bestRoot) = _readyMessages
                .Where(x => x != null)
                .GroupBy(x => x!.MerkleTreeRoot)
                .Select(m => (cnt: m.Count(), key: m.Key))
                .OrderByDescending(x => x.cnt)
                .First();
            if (bestRootCnt != F + 1) return;
            if (_readySent) return;
            Broadcaster.Broadcast(CreateReadyMessage(bestRoot));
            _readySent = true;
            // Logger.LogDebug($"{Id} broadcast ReadyMessage from HandleReadyMessage()");
        }

        private void CheckResult()
        {
            if (_requested != ResultStatus.Requested) return;
            var (bestRootCnt, bestRoot) = _readyMessages
                .Where(x => x != null)
                .GroupBy(x => x!.MerkleTreeRoot)
                .Select(m => (cnt: m.Count(), key: m.Key))
                .OrderByDescending(x => x.cnt)
                .FirstOrDefault();
            // Logger.LogDebug($"{Id}: checking READY messages, best root has {bestRootCnt} votes");
            if (bestRootCnt < 2 * F + 1) return;
            // Logger.LogDebug($"{Id}: got {2 * F + 1} READY, trying to get {N - 2 * F} ECHOs and finish");
            var matchingEchos = _echoMessages
                .WithIndex()
                .Where(t => bestRoot.Equals(t.item?.MerkleTreeRoot))
                .Select(t => (echo: t.item!, t.index))
                .Take(N - 2 * F)
                .ToArray();
            if (matchingEchos.Length < N - 2 * F) return;
            // Logger.LogDebug($"{Id}: got {N - 2 * F} ECHOs, returning result");

            var restored = DecodeFromEchos(matchingEchos);
            var len = restored.AsReadOnlySpan().Slice(0, 4).ToInt32();
            var result = EncryptedShare.FromBytes(restored.AsMemory().Slice(4, len));

            // Logger.LogDebug($"{Id} returned result");
            _requested = ResultStatus.Sent;
            Broadcaster.InternalResponse(new ProtocolResult<ReliableBroadcastId, EncryptedShare>(_broadcastId, result));
        }

        public byte[] DecodeFromEchos(IReadOnlyCollection<(ECHOMessage echo, int from)> echos)
        {
            var erasureCoding = new ErasureCoding();
            Debug.Assert(echos.Count == N - 2 * F);
            Debug.Assert(echos.Select(t => t.echo.Data.Length).Distinct().Count() == 1);
            var shardLength = echos.First().echo.Data.Length;
            var result = new byte[shardLength * N];
            foreach (var (echo, i) in echos)
                Buffer.BlockCopy(echo.Data.ToArray(), 0, result, i * shardLength, shardLength);
            for (var i = 0; i < shardLength; ++i)
            {
                var codeword = new int[N];
                for (var j = 0; j < N; ++j) codeword[j] = result[i + j * shardLength];
                var erasurePlaces = Enumerable.Range(0, N)
                    .Where(z => !echos.Select(t => t.from).Contains(z))
                    .ToArray();
                Debug.Assert(erasurePlaces.Length == 2 * F);
                erasureCoding.Decode(codeword, 2 * F, erasurePlaces);
                for (var j = 0; j < N; ++j) result[i + j * shardLength] = checked((byte) codeword[j]);
            }

            return result;
        }

        private void Abort()
        {
            Logger.LogError($"{Id} was aborted!");
            Terminate();
        }

        private bool CheckEchoMessage(ECHOMessage msg, int from)
        {
            UInt256 value = msg.Data.Keccak();
            for (int i = from + _merkleTreeSize, j = 0; i > 1; i /= 2, ++j)
            {
                value = (i & 1) == 0
                    ? value.ToBytes().Concat(msg.MerkleProof[j].ToBytes()).Keccak() // we are left sibling
                    : msg.MerkleProof[j].ToBytes().Concat(value.ToBytes()).Keccak(); // we are right sibling
            }

            return msg.MerkleTreeRoot.Equals(value);
        }

        private void AugmentInput(List<byte> input)
        {
            var sz = input.Count;
            input.InsertRange(0, sz.ToBytes());
            var dataShards = N - 2 * F;
            var shardSize = (input.Count + dataShards - 1) / dataShards;
            input.AddRange(Enumerable.Repeat<byte>(0, dataShards * shardSize - input.Count));
            Debug.Assert(input.Count == dataShards * shardSize);
        }

        private ValMessage[] ConstructValMessages(IReadOnlyList<byte> input)
        {
            var shards = ErasureCodingShards(input, N, 2 * F);
            var merkleTree = ConstructMerkleTree(shards);
            var result = new ValMessage[N];
            for (var i = 0; i < N; ++i)
            {
                result[i] = new ValMessage
                {
                    SenderId = _broadcastId.SenderId,
                    MerkleTreeRoot = merkleTree[1],
                    MerkleProof = {MerkleTreeBranch(merkleTree, i)},
                    Data = ByteString.CopyFrom(shards[i]),
                };
            }

            return result;
        }

        private ConsensusMessage CreateEchoMessage(ValMessage valMessage)
        {
            return new ConsensusMessage
            {
                EchoMessage = new ECHOMessage
                {
                    SenderId = _broadcastId.SenderId,
                    MerkleTreeRoot = valMessage.MerkleTreeRoot,
                    Data = valMessage.Data,
                    MerkleProof = {valMessage.MerkleProof}
                }
            };
        }

        private ConsensusMessage CreateReadyMessage(UInt256 merkleRoot)
        {
            return new ConsensusMessage
            {
                ReadyMessage = new ReadyMessage
                {
                    SenderId = _broadcastId.SenderId,
                    MerkleTreeRoot = merkleRoot,
                }
            };
        }

        private static List<UInt256> MerkleTreeBranch(IReadOnlyList<UInt256> tree, int i)
        {
            var n = tree.Count / 2;
            var result = new List<UInt256>();
            for (i += n; i > 1; i /= 2) // go up in Merkle tree, div 2 means go to parent
                result.Add(tree[i ^ 1]); // xor 1 means take sibling
            return result;
        }

        private UInt256[] ConstructMerkleTree(IReadOnlyCollection<IReadOnlyCollection<byte>> shards)
        {
            Debug.Assert(shards.Count == N);
            var result = new UInt256[_merkleTreeSize * 2];
            foreach (var (shard, i) in shards.WithIndex())
                result[_merkleTreeSize + i] = shard.Keccak();
            for (var i = shards.Count; i < _merkleTreeSize; ++i)
                result[_merkleTreeSize + i] = UInt256Utils.Zero;
            for (var i = _merkleTreeSize - 1; i >= 1; --i)
                result[i] = result[2 * i].ToBytes().Concat(result[2 * i + 1].ToBytes()).Keccak();
            return result;
        }

        /**
         * Split arbitrary array into specified number of shards adding some parity shards
         * After this operation whole array can be recovered if specified number of shards is lost
         * Array length is assumed to be divisible by (shards - erasures)
         */
        public static byte[][] ErasureCodingShards(IReadOnlyList<byte> input, int shards, int erasures)
        {
            var erasureCoding = new ErasureCoding();
            var dataShards = shards - erasures;
            if (input.Count % dataShards != 0) throw new InvalidOperationException();
            var shardSize = input.Count / dataShards;
            var result = new byte[shards][];
            foreach (var (shard, i) in input
                .Batch(shardSize)
                .WithIndex()
            ) result[i] = shard.ToArray();

            for (var i = dataShards; i < shards; ++i) result[i] = new byte[shardSize];

            for (var i = 0; i < shardSize; ++i)
            {
                var codeword = new int[shards];
                for (var j = 0; j < dataShards; j++)
                    codeword[j] = input[i + j * shardSize];
                erasureCoding.Encode(codeword, erasures);
                for (var j = dataShards; j < shards; ++j)
                    result[j][i] = checked((byte) codeword[j]);
            }

            return result;
        }
    }
}