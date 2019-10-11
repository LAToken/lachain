using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using NUnit.Framework;
using Org.BouncyCastle.Crypto.Engines;
using Phorkus.Consensus;
using Phorkus.Consensus.Messages;
using Phorkus.Proto;

namespace Phorkus.ConsensusTest
{
    public class DeliverySerivce
    {
        public readonly ISet<int> _mutedPlayers = new HashSet<int>();
        private readonly IDictionary<int, IConsensusBroadcaster> _broadcasters = new Dictionary<int, IConsensusBroadcaster>();
        private readonly RandomSamplingQueue<Tuple<int, ConsensusMessage>> _queue = new RandomSamplingQueue<Tuple<int, ConsensusMessage>>();
        private readonly object _queueLock = new object();
        public double RepeatProbability
        {
            get => _queue.RepeatProbability;
            set => _queue.RepeatProbability = value;
        }

        private bool Terminated { get; set; }

        private readonly Thread _thread;
        private bool _stopped;
        public DeliveryServiceMode Mode { get; set; }

        public DeliverySerivce()
        {
            Mode = DeliveryServiceMode.TAKE_FIRST;
            _thread = new Thread(Start) {IsBackground = true};
            _thread.Start();
        }
        
        public void AddPlayer(int index, IConsensusBroadcaster player)
        {
            _broadcasters.Add(index, player);
        }

        public void MutePlayer(int index)
        {
            _mutedPlayers.Add(index);
        }
        
        public void WaitFinish()
        {
            Terminated = true;
            lock (_queueLock)
            {
                Monitor.Pulse(_queueLock);
                Console.Error.WriteLine("_queueLock pulsed.");
            }
            _thread.Join();
        }

        private void Start()
        {
            while (!Terminated || !_queue.IsEmpty)
            {
                lock (_queueLock)
                {
                    while (_queue.IsEmpty && !Terminated)
                    {
                        Monitor.Wait(_queueLock);
                    }

                    if (!_queue.IsEmpty)
                    {
                        Tuple<int, ConsensusMessage> tuple;
                        var success = Mode switch
                        {
                            DeliveryServiceMode.TAKE_FIRST => _queue.TryDequeue(out tuple),
                            DeliveryServiceMode.TAKE_LAST => _queue.TryTakeLast(out tuple),
                            DeliveryServiceMode.TAKE_RANDOM => _queue.TrySample(out tuple),
                            _ => throw new NotImplementedException($"Unknown mode {Mode}")
                        };

                        if (!success)
                        {
                            throw new Exception("Can't sample from queue!");
                        }

                        Console.Error.WriteLine($"remaining in queue: {_queue.Count}");
                        
                        var index = tuple.Item1;
                        var message = tuple.Item2;
                        
                        if (_mutedPlayers.Contains(index)) continue;
                        
                        try
                        {
                            _broadcasters[index].Dispatch(message);
                        }
                        catch (Exception e)
                        {
                            Console.Error.WriteLine(e);
                            Terminated = true;
                            break;
                        }
                    }
                    else
                    {
                        if (Terminated) return;
                    }
                }

                
            }
        }

        private void ReceiveMessage(int index, ConsensusMessage message)
        {
            if (Terminated) return;
            lock (_queueLock)
            {
                _queue.Enqueue(new Tuple<int, ConsensusMessage>(index, message));
                Monitor.Pulse(_queueLock);
            }
        }

        public void BroadcastMessage(ConsensusMessage consensusMessage)
        {
            for (var i = 0; i < _broadcasters.Count; ++i)
            {
                ReceiveMessage(i, consensusMessage);
            }
        }

        public void SendToPlayer(int index, ConsensusMessage consensusMessage)
        {
            ReceiveMessage(index, consensusMessage);
        }
    }
}