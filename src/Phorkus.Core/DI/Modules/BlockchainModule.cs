using Phorkus.Core.Blockchain;
using Phorkus.Core.Blockchain.Genesis;
using Phorkus.Core.Blockchain.OperationManager;
using Phorkus.Core.Blockchain.OperationManager.BlockManager;
using Phorkus.Core.Blockchain.OperationManager.TransactionManager;
using Phorkus.Core.Blockchain.Pool;
using Phorkus.Core.Config;
using Phorkus.Core.Consensus;
using IValidatorManager = Phorkus.Core.Blockchain.IValidatorManager;

namespace Phorkus.Core.DI.Modules
{
    public class BlockchainModule : IModule
    {
        public void Register(IContainerBuilder containerBuilder, IConfigManager configManager)
        {
            /* global */
            containerBuilder.RegisterSingleton<IBlockchainManager, BlockchainManager>();
            containerBuilder.RegisterSingleton<ITransactionVerifier, TransactionVerifier>();
            containerBuilder.RegisterSingleton<ITransactionFactory, TransactionFactory>();
            containerBuilder.RegisterSingleton<IValidatorManager, ValidatorManager>();
            /* consensus */
//            containerBuilder.RegisterSingleton<IConsensusManager, ConsensusManager>();
            /* gensis */
            containerBuilder.RegisterSingleton<IGenesisAssetsBuilder, GenesisAssetsBuilder>();
            containerBuilder.RegisterSingleton<IGenesisBuilder, GenesisBuilder>();
            /* operation manager */
            containerBuilder.RegisterSingleton<ITransactionManager, TransactionManager>();
            containerBuilder.RegisterSingleton<IBlockManager, BlockManager>();
            /* pool */
            containerBuilder.RegisterSingleton<ITransactionPool, TransactionPool>();
        }
    }
}