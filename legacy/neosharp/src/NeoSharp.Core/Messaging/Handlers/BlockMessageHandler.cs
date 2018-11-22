using System;
using System.Threading.Tasks;
using NeoSharp.Core.Blockchain.Processing;
using NeoSharp.Core.Blockchain.Processing.BlockProcessing;
using NeoSharp.Core.Logging;
using NeoSharp.Core.Messaging.Messages;
using NeoSharp.Core.Models.OperationManager;
using NeoSharp.Core.Network;
using Newtonsoft.Json;

namespace NeoSharp.Core.Messaging.Handlers
{
    public class BlockMessageHandler : MessageHandler<BlockMessage>
    {
        #region Private Fields 

        private readonly IBlockProcessor _blockProcessor;
        private readonly IBlockOperationsManager _blockOperationsManager;
        private readonly IBroadcaster _broadcaster;
        private readonly ILogger<BlockMessageHandler> _logger;

        #endregion

        #region Constructor 

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="blockProcessor">Block Pool</param>
        /// <param name="blockOperationsManager">The block operations mananger.</param>
        /// <param name="broadcaster">Broadcaster</param>
        /// <param name="logger">Logger</param>
        public BlockMessageHandler(
            IBlockProcessor blockProcessor,
            IBlockOperationsManager blockOperationsManager,
            IBroadcaster broadcaster,
            ILogger<BlockMessageHandler> logger)
        {
            _blockProcessor = blockProcessor ?? throw new ArgumentNullException(nameof(blockProcessor));
            _blockOperationsManager = blockOperationsManager ?? throw new ArgumentNullException(nameof(blockOperationsManager));
            _broadcaster = broadcaster ?? throw new ArgumentNullException(nameof(broadcaster));
            _logger = logger;
        }

        #endregion

        #region MessageHandler override methods 

        /// <inheritdoc />
        public override bool CanHandle(Message message) => message is BlockMessage;

        /// <inheritdoc />
        public override async Task Handle(BlockMessage message, IPeer sender)
        {
            var block = message.Payload;
            if (block.Hash == null)
                _blockOperationsManager.Sign(block);
            
            if (!_blockOperationsManager.Verify(block))
            {
                _logger.LogWarning($"Block {block.Hash} with Index {block.Index} verification fail.");
                return;
            }
            
//            var json = JsonConvert.SerializeObject(block, Formatting.Indented);
            Console.WriteLine("new block (height: " + block.Index + ", txs: " + block.Transactions.Length + "): " + block.Hash);
//            Console.WriteLine("--------------------------------");
//            Console.WriteLine(json);
//            Console.WriteLine("--------------------------------");

            //_logger.LogInformation($"Broadcasting block {block.Hash} with Index {block.Index}.");
            //_broadcaster.Broadcast(message, sender);
            
            // TODO: if we are consensus node, ignore this
            await _blockProcessor.AddBlock(block);
        }

        #endregion
    }
}