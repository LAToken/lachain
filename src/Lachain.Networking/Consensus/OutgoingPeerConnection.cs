using System;
using System.Collections.Generic;
using Google.Protobuf;
using Lachain.Logger;
using Lachain.Networking.Hub;
using Lachain.Networking.ZeroMQ;
using Lachain.Proto;
using Lachain.Utility.Benchmark;

namespace Lachain.Networking.Consensus
{
    public class OutgoingPeerConnection : IDisposable
    {
        private static readonly ILogger<OutgoingPeerConnection> Logger =
            LoggerFactory.GetLoggerForClass<OutgoingPeerConnection>();

        private readonly PeerAddress _address;
        private readonly IMessageFactory _messageFactory;
        private readonly ThroughputCalculator _throughputCalculator;
        private readonly LinkedList<NetworkMessage> _unsent = new LinkedList<NetworkMessage>();
        private ClientWorker? _client;

        public OutgoingPeerConnection(PeerAddress address, IMessageFactory messageFactory, Node localNode)
        {
            if (address.PublicKey is null) throw new Exception("Peer address must have public key");
            _address = address;
            _messageFactory = messageFactory;
            _throughputCalculator = new ThroughputCalculator(
                TimeSpan.FromSeconds(1),
                (speed, cnt) =>
                    Logger.LogTrace(
                        $"Outgoing bandwidth to peer {address}: {speed / 1024:0.00} KiB/s, {cnt} messages"
                    )
            );
            ClientWorker.SendOnce(
                address,
                messageFactory.MessagesBatch(new[] {messageFactory.HandshakeRequest(localNode)})
            );
        }

        public void InitConnection(PeerAddress address)
        {
            if (_client != null) return;
            Logger.LogDebug($"Initiating consensus connection to peer {address}");
            _client = new ClientWorker(address, address.PublicKey, _messageFactory);
            _client.Start();
            lock (_unsent)
            {
                Logger.LogTrace($"Sending {_unsent.Count} unsent messages to {address}");
                foreach (var msg in _unsent) Send(msg);
                _unsent.Clear();
            }
        }

        public void Send(NetworkMessage message)
        {
            if (_client is null)
            {
                lock (_unsent)
                {
                    _unsent.AddLast(message);
                    if (_unsent.Count > 10)
                    {
                        while (_unsent.Count > 10)
                        {
                            var payload = _unsent.First.Value.ToByteArray();
                            CommunicationHub.Send(
                                _messageFactory.GetPublicKey(),
                                _address.PublicKey!,
                                payload,
                                _messageFactory.SignCommunicationHubSend(_address.PublicKey!, payload)
                            );
                            _unsent.RemoveFirst();
                        }
                    }
                }
            }
            else
            {
                _throughputCalculator.RegisterMeasurement(message.CalculateSize());
                _client.Send(message);
            }
        }

        public void ReceiveAck(ulong messageId)
        {
            _client?.ReceiveAck(messageId);
        }

        public void Dispose()
        {
            _unsent.Clear();
            _client?.Dispose();
        }
    }
}