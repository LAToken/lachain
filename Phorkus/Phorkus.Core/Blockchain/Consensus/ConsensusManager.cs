﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using Google.Protobuf;
using Google.Protobuf.WellKnownTypes;
using Org.BouncyCastle.Security;
using Phorkus.Core.Blockchain.OperationManager;
using Phorkus.Core.Blockchain.Pool;
using Phorkus.Core.Logging;
using Phorkus.Core.Network;
using Phorkus.Core.Network.Proto;
using Phorkus.Core.Proto;
using Phorkus.Core.Utils;

namespace Phorkus.Core.Blockchain.Consensus
{
    using Timer = System.Timers.Timer;

    public class ConsensusManager : IConsensusManager, IDisposable
    {
        private readonly IBlockManager _blockManager;
        private readonly ITransactionManager _transactionManager;

        private readonly IBlockchainContext _blockchainContext;

//        private readonly ITransactionCrawler _transactionCrawler;
        private readonly ITransactionPool _transactionPool;

        private readonly IBroadcaster _broadcaster;

        private readonly ILogger<ConsensusManager> _logger;
        private readonly ConsensusContext _context;
        private readonly object _allTransactionVerified = new object();
        private readonly object _quorumSignaturesAcquired = new object();
        private readonly object _prepareRequestReceived = new object();
        private readonly object _changeViewApproved = new object();
        private readonly object _timeToProduceBlock = new object();
        private Timer _timer;
        private bool _stopped;
        private readonly SecureRandom _random;

        private readonly TimeSpan _timePerBlock = TimeSpan.FromSeconds(15);

        public ConsensusManager(
            IBlockManager blockManager, ITransactionManager transactionManager,
            IBlockchainContext blockchainContext,
            ITransactionPool transactionPool, IBroadcaster broadcaster,
            ILogger<ConsensusManager> logger
            //ConsensusConfig configuration
        )
        {
            _blockManager = blockManager ?? throw new ArgumentNullException(nameof(blockManager));
            _transactionManager = transactionManager;
            _blockchainContext = blockchainContext ?? throw new ArgumentNullException(nameof(blockchainContext));
            //_transactionCrawler = transactionCrawler ?? throw new ArgumentNullException(nameof(transactionCrawler));
            _transactionPool = transactionPool ?? throw new ArgumentNullException(nameof(transactionPool));
            _broadcaster = broadcaster;
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            //_context = new ConsensusContext(configuration.KeyPair, configuration.ValidatorsKeys);
            _context = new ConsensusContext(null, null);
            _random = new SecureRandom();

            (transactionManager ?? throw new ArgumentNullException(nameof(transactionManager)))
                .OnTransactionPersisted += OnTransactionVerified;
        }

        public void Stop()
        {
            _stopped = true;
        }

        private void _TaskWorker()
        {
            InitializeConsensus(0);
            _context.Timestamp = _blockchainContext.CurrentBlock.BlockHeader.Timestamp;
            Thread.Sleep(1000);
            while (!_stopped)
            {
                // If were are waiting for view change, just wait
                if (_context.State.HasFlag(ConsensusState.ViewChanging))
                {
                    lock (_changeViewApproved)
                    {
                        // TODO: manage timeouts
                        var timeToWait = TimeUtils.Multiply(_timePerBlock, 1 + _context.MyState.ExpectedViewNumber);
                        if (!Monitor.Wait(_changeViewApproved, timeToWait))
                        {
                            RequestChangeView();
                            continue;
                        }

                        InitializeConsensus(_context.ViewNumber);
                    }
                }

                if (_context.Role.HasFlag(ConsensusState.Primary))
                {
                    // if we are primary, wait until block must be produced
                    lock (_timeToProduceBlock)
                    {
                        if (DateTime.UtcNow - _context.LastBlockRecieved < _timePerBlock)
                            Monitor.Wait(_timeToProduceBlock);
                    }

                    // TODO: produce block
                    var blockBuilder = new BlockBuilder(
                        _transactionPool, _blockchainContext.CurrentBlockHeader.Hash,
                        _blockchainContext.CurrentBlockHeader.BlockHeader.Index
                    );
                    var blockWithTransactions = blockBuilder.Build((ulong) _random.Next());
                    _logger.LogInformation($"Produced block with hash {blockWithTransactions.Block.Hash}");
                    _context.UpdateCurrentProposal(blockWithTransactions);
                    _context.State |= ConsensusState.RequestSent;

                    if (!_context.State.HasFlag(ConsensusState.SignatureSent))
                    {
                        var result = _blockManager.Sign(blockWithTransactions.Block, _context.PrivateKey);
                        _context.MyState.BlockSignature = result;
                        _context.MyState.BlockSignature = new Signature
                        {
                            Buffer = ByteString.CopyFromUtf8("BADCABLE!!!!")
                        };
                    }

                    SignAndBroadcast(
                        _context.MakePrepareRequest(blockWithTransactions, _context.MyState.BlockSignature));
                    _logger.LogInformation("Sent prepare request");
                }
                else
                {
                    // if we are backup, wait unitl someone sends prepare, or change view
                    lock (_prepareRequestReceived)
                    {
                        // TODO: manage timeouts
                        var timeToWait = TimeUtils.Multiply(_timePerBlock, 1 + _context.MyState.ExpectedViewNumber);
                        if (!Monitor.Wait(_prepareRequestReceived, timeToWait))
                        {
                            RequestChangeView();
                            continue;
                        }
                    }
                }

//                TODO: we should get and verify all transactions here
//                _context.CurrentProposal.TransactionHashes.ForEach(hash =>
//                {
//                    var transaction = _transactionPool.FindByHash(hash);
//                    if (transaction != null) _context.CurrentProposal.Transactions[hash] = transaction;
//                    else _transactionCrawler.AddTransactionHash(hash);
//                });
                lock (_allTransactionVerified)
                {
                    if (!Monitor.Wait(_allTransactionVerified, _timePerBlock)) // TODO: manage timeouts
                    {
                        _logger.LogWarning("Cannot retrieve all transactions in time, aborting");
                        RequestChangeView();
                        continue;
                    }
                }

                _logger.LogInformation("Send prepare response");
                _context.State |= ConsensusState.SignatureSent;
                var mySignature = new Signature
                {
                    Buffer = ByteString.CopyFromUtf8("BADCABLE!!!")
                };
                _context.MyState.BlockSignature = mySignature;
                //_context.Validators[_context.MyIndex].BlockSignature = _context.GetProposedHeader().Sign(context.KeyPair);
                SignAndBroadcast(_context.MakePrepareResponse(_context.MyState.BlockSignature));
                OnSignatureAcquired(_context.MyIndex, mySignature);
                lock (_quorumSignaturesAcquired)
                {
                    // TODO: manage timeouts
                    var timeToWait = TimeUtils.Multiply(_timePerBlock, 1 + _context.MyState.ExpectedViewNumber);
                    if (!Monitor.Wait(_quorumSignaturesAcquired, timeToWait))
                    {
                        _logger.LogWarning("Cannot retrieve all signatures in time, aborting");
                        RequestChangeView();
                        continue;
                    }
                }

                _logger.LogInformation(
                    $"Collected sinatures={_context.SignaturesAcquired}, quorum={_context.Quorum}"
                );

                var block = _context.GetProposedBlock();
                /* TODO: multisig
                ContractParametersContext sc = new ContractParametersContext(block);
                for (int i = 0, j = 0; i < context.Validators.Length && j < context.M; i++)
                    if (context.Signatures[i] != null)
                    {
                        sc.AddSignature(contract, context.Validators[i], context.Signatures[i]);
                        j++;
                    }
                sc.Verifiable.Witnesses = sc.GetWitnesses();
                */
                _logger.LogInformation($"Block approved by consensus: {block.Hash}");

                _context.State |= ConsensusState.BlockSent;
                // TODO: persist block
//                _blockManager.AddBlock(block).Start(); // ??
//                _blockManager.WaitUntilBlockProcessed(block.Index);

                _logger.LogInformation($"Block persist completed: {block.Hash}");
                _context.LastBlockRecieved = DateTime.UtcNow;
                InitializeConsensus(0);
            }
        }

        public void Start()
        {
            _logger.LogInformation("Starting consensus");
            Task.Factory.StartNew(_TaskWorker);
        }

        private void OnTimer(object sender, ElapsedEventArgs e)
        {
            lock (_timeToProduceBlock)
            {
                Monitor.PulseAll(_timeToProduceBlock);
            }
        }

        private void InitializeConsensus(byte viewNumber)
        {
            if (viewNumber == 0)
                _context.ResetState(_blockchainContext.CurrentBlock.Hash,
                    _blockchainContext.CurrentBlock.BlockHeader.Index);
            else
                _context.ChangeView(viewNumber);
            if (_context.MyIndex < 0) return;
            _logger.LogInformation(
                $"Initialized consensus: height={_context.BlockIndex} view={viewNumber} " +
                $"my_index={_context.MyIndex} role={_context.Role}"
            );

            if (!_context.Role.HasFlag(ConsensusState.Primary))
            {
                _context.State |= ConsensusState.Backup;
                return;
            }

            _context.State |= ConsensusState.Primary;
            var span = DateTime.UtcNow - _context.LastBlockRecieved;
            if (span >= _timePerBlock) OnTimer(null, null);
            else
            {
                _timer = new Timer((_timePerBlock - span).TotalMilliseconds);
                _timer.Elapsed += OnTimer;
            }
        }

        private void OnPrepareRequestReceived(ConsensusPayload payload)
        {
            if (_context.State.HasFlag(ConsensusState.ViewChanging))
            {
                _logger.LogDebug(
                    $"Ignoring prepare request from validator={payload.ValidatorIndex}: we are changing view"
                );
                return;
            }

            if (_context.State.HasFlag(ConsensusState.RequestReceived))
            {
                _logger.LogDebug(
                    $"Ignoring prepare request from validator={payload.ValidatorIndex}: we are already prepared"
                );
                return;
            }

            if (payload.ValidatorIndex != _context.PrimaryIndex)
            {
                _logger.LogDebug(
                    $"Ignoring prepare request from validator={payload.ValidatorIndex}: validator is not primary"
                );
                return;
            }

            var prepareRequest = payload.PrepareRequest;
            _logger.LogInformation(
                $"{nameof(OnPrepareRequestReceived)}: height={payload.BlockIndex} view={payload.ViewNumber} " +
                $"index={payload.ValidatorIndex} tx={prepareRequest.TransactionHashes.Count}"
            );
            if (!_context.State.HasFlag(ConsensusState.Backup))
            {
                _logger.LogDebug(
                    $"Ignoring prepare request from validator={payload.ValidatorIndex}: were are primary"
                );
                return;
            }

            if (payload.Timestamp <= _blockchainContext.CurrentBlockHeader.BlockHeader.Timestamp ||
                payload.Timestamp > (ulong) DateTime.UtcNow.AddMinutes(10).ToTimestamp().Seconds)
            {
                _logger.LogDebug(
                    $"Ignoring prepare request from validator={payload.ValidatorIndex}: " +
                    $"timestamp incorrect: theirs={payload.Timestamp} ours={_context.Timestamp} " +
                    $"last_block={_blockchainContext.CurrentBlockHeader.BlockHeader.Timestamp}"
                );
                return;
            }

            _context.State |= ConsensusState.RequestReceived;
            _context.Timestamp = payload.Timestamp;
            _context.Nonce = prepareRequest.Nonce; // TODO: we are blindly accepting their nonce!
            _context.CurrentProposal = new ConsensusProposal
            {
                TransactionHashes = prepareRequest.TransactionHashes.ToArray(),
                Transactions = new Dictionary<UInt256, SignedTransaction>()
            };

            /* TODO: check signature
            byte[] hashData = BinarySerializer.Default.Serialize(_context.GetProposedHeader().Hash);
             if (!Crypto.Default.VerifySignature(hashData, request.Signature,
                 _context.Validators[message.ValidatorIndex].PublicKey.DecodedData))
            {
                return;
            }
            for (int i = 0; i < context.Signatures.Length; i++)
                if (context.Signatures[i] != null)
                    if (!Crypto.Default.VerifySignature(hashData, context.Signatures[i],
                        context.Validators[i].EncodePoint(false)))
                        context.Signatures[i] = null;
            */
            OnSignatureAcquired(payload.ValidatorIndex, prepareRequest.Signature);
            _logger.LogInformation(
                $"Prepare request from validator={payload.ValidatorIndex} accepted, requesting missing transactions"
            );
        }

        public void HandleConsensusMessage(ConsensusMessage message)
        {
            var body = message.Payload;
            if (_context.State.HasFlag(ConsensusState.BlockSent)) return;
            if (body.ValidatorIndex == _context.MyIndex) return;
            if (body.Version != ConsensusContext.Version) return;
            // TODO: check payload signature
            if (!body.PrevHash.Equals(_context.PreviousBlockHash) || body.BlockIndex != _context.BlockIndex)
            {
                _logger.LogWarning(
                    $"Cannot handle consensus payload at height={body.BlockIndex}, " +
                    $"local height={_blockchainContext.CurrentBlockHeader.BlockHeader.Index}"
                );
                if (_blockchainContext.CurrentBlockHeader.BlockHeader.Index + 1 < body.BlockIndex)
                {
                    return;
                }

                _logger.LogWarning("Rejected consensus payload because of prev hash mismatch");
                return;
            }

            if (body.ValidatorIndex >= _context.ValidatorCount) return;

            if (body.ViewNumber != _context.ViewNumber &&
                body.Type != ConsensusPayload.Types.ConsensusPayloadType.ChangeView)
            {
                _logger.LogWarning(
                    $"Rejected consensus payload of type {body.Type} because view does not match, " +
                    $"my={_context.ViewNumber} theirs={body.ViewNumber}"
                );
                return;
            }

            _logger.LogInformation($"Received consensus payload of type {body.Type}");
            switch (body.Type)
            {
                case ConsensusPayload.Types.ConsensusPayloadType.ChangeView:
                    OnChangeViewReceived(body);
                    break;
                case ConsensusPayload.Types.ConsensusPayloadType.PrepareRequest:
                    OnPrepareRequestReceived(body);
                    break;
                case ConsensusPayload.Types.ConsensusPayloadType.PrepareResponse:
                    OnPrepareResponseReceived(body);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private void OnPrepareResponseReceived(ConsensusPayload message)
        {
            if (_context.Validators[message.ValidatorIndex].BlockSignature != null) return;
            _logger.LogInformation(
                $"{nameof(OnPrepareResponseReceived)}: height={message.BlockIndex} view={message.ViewNumber} " +
                $"index={message.ValidatorIndex}"
            );
            OnSignatureAcquired(message.ValidatorIndex, message.PrepareResponse.Signature);
        }

        private bool OnSignatureAcquired(long validatorIndex, Signature signature)
        {
            if (_context.Validators[validatorIndex].BlockSignature != null) return false;
            // TODO: verify signature
            //byte[] hashData = _context.GetProposedHeader()?.GetHashData();
            //if (Crypto.Default.VerifySignature(hashData, message.Signature,
            //    context.Validators[payload.ValidatorIndex].EncodePoint(false))) ...
            _context.Validators[validatorIndex].BlockSignature = signature;
            _context.SignaturesAcquired++;
            if (_context.SignaturesAcquired >= _context.Quorum)
            {
                lock (_quorumSignaturesAcquired)
                {
                    Monitor.PulseAll(_quorumSignaturesAcquired);
                }
            }

            return true;
        }

        private void OnTransactionVerified(object sender, SignedTransaction e)
        {
            if (_context.CurrentProposal.Transactions.ContainsKey(e.Hash)) return;
            _context.CurrentProposal.Transactions[e.Hash] = e;
            if (!_context.CurrentProposal.IsComplete) return;
            lock (_allTransactionVerified)
            {
                Monitor.PulseAll(_allTransactionVerified);
            }
        }

        private void RequestChangeView()
        {
            _context.State |= ConsensusState.ViewChanging;
            _context.MyState.ExpectedViewNumber++;
            _logger.LogInformation(
                $"request change view: height={_context.BlockIndex} view={_context.ViewNumber} " +
                $"nv={_context.MyState.ExpectedViewNumber} state={_context.State}"
            );
            SignAndBroadcast(_context.MakeChangeView());
            CheckExpectedView(_context.MyState.ExpectedViewNumber);
        }

        private void OnChangeViewReceived(ConsensusPayload payload)
        {
            var changeView = payload.ChangeView;
            if (changeView.NewViewNumber <= _context.Validators[payload.ValidatorIndex].ExpectedViewNumber)
                return;
            _logger.LogInformation(
                $"{nameof(OnChangeViewReceived)}: height={payload.BlockIndex} view={payload.ViewNumber} " +
                $"index={payload.ValidatorIndex} nv={changeView.NewViewNumber}"
            );
            _context.Validators[payload.ValidatorIndex].ExpectedViewNumber = (byte) changeView.NewViewNumber;
            CheckExpectedView((byte) changeView.NewViewNumber);
        }

        private void CheckExpectedView(byte viewNumber)
        {
            if (_context.ViewNumber == viewNumber) return;
            if (_context.Validators.Select(v => v.ExpectedViewNumber).Count(p => p == viewNumber) <
                _context.Quorum) return;
            lock (_changeViewApproved)
            {
                _context.ViewNumber = viewNumber;
                Monitor.PulseAll(_changeViewApproved);
            }
        }

        private void SignAndBroadcast(ConsensusPayload payload)
        {
            // TODO: sign
            var message = new ConsensusMessage
            {
                Payload = payload,
                Signature = new Signature
                {
                    Buffer = ByteString.CopyFromUtf8("BADCABLE!!!")
                }
            };
            _broadcaster.Broadcast(
                new Message
                {
                    Type = MessageType.ConsensusMessage,
                    ConsensusMessage = message
                }
            );
        }

        public void Dispose()
        {
            _timer?.Dispose();
        }
    }
}