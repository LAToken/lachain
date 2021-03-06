using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Lachain.Logger;
using Lachain.Core.Blockchain.Interface;
using Lachain.Core.Blockchain.Pool;
using Lachain.Core.ValidatorStatus;
using Lachain.Crypto.ECDSA;
using Lachain.Storage.State;

namespace Lachain.Core.CLI
{
    public class ConsoleManager : IConsoleManager
    {
        private static readonly ILogger<ConsoleManager> Logger = LoggerFactory.GetLoggerForClass<ConsoleManager>();

        private readonly ITransactionPool _transactionPool;
        private readonly ITransactionBuilder _transactionBuilder;
        private readonly ITransactionManager _transactionManager;
        private readonly ITransactionSigner _transactionSigner;
        private readonly IBlockManager _blockManager;
        private readonly IStateManager _stateManager;
        private readonly ISystemContractReader _systemContractReader;
        private readonly IValidatorStatusManager _validatorStatusManager;
        private IConsoleCommands? _consoleCommands;

        public bool IsWorking { get; set; }

        public ConsoleManager(
            ITransactionBuilder transactionBuilder,
            ITransactionPool transactionPool,
            ITransactionManager transactionManager,
            ITransactionSigner transactionSigner,
            IBlockManager blockManager,
            IStateManager stateManager,
            ISystemContractReader systemContractReader,
            IValidatorStatusManager validatorStatusManager
        )
        {
            _blockManager = blockManager;
            _transactionBuilder = transactionBuilder;
            _transactionPool = transactionPool;
            _transactionManager = transactionManager;
            _transactionSigner = transactionSigner;
            _stateManager = stateManager;
            _systemContractReader = systemContractReader;
            _validatorStatusManager = validatorStatusManager;
        }

        private void _Worker(EcdsaKeyPair keyPair)
        {
            _consoleCommands = new ConsoleCommands(
                _transactionBuilder, _transactionPool, _transactionManager, _transactionSigner,
                _blockManager, _stateManager, _systemContractReader, _validatorStatusManager, 
                keyPair
            );
            try
            {
                Console.Write(" > ");
                var command = Console.ReadLine();
                if (string.IsNullOrWhiteSpace(command))
                    return;
                var argumentsTrash = command.Split(' ');
                var arguments = argumentsTrash.Where(argument => argument != " ").ToList();
                if (arguments.Count == 0)
                    return;
                var theCommand = _consoleCommands.GetType()
                    .GetMethods().FirstOrDefault(method => method.Name.ToLower().Contains(arguments[0].ToLower()));
                if (theCommand == null)
                    return;
                try
                {
                    var result = theCommand.Invoke(_consoleCommands, new object[] {arguments.ToArray()});
                    if (result == null)
                    {
                        Logger.LogError("Wrong arguments!\n");
                        Console.Out.Write("null\n");
                        return;
                    }

                    Console.Out.Write(result.ToString() + '\n');
                }
                catch (Exception e)
                {
                    Console.Error.WriteLine(e);
                    Logger.LogError("Incorrect cli method call!\n");
                }
            }
            catch (Exception e)
            {
                Console.Error.WriteLine(e);
                Logger.LogError("Incorrect cli method call!\n");
            }
        }

        public void Start(EcdsaKeyPair keyPair)
        {
            Task.Factory.StartNew(() =>
            {
                try
                {
                    var thread = Thread.CurrentThread;
                    while (thread.IsAlive)
                    {
                        _Worker(keyPair);
                    }
                }
                catch (Exception e)
                {
                    Console.Error.WriteLine(e);
                    Logger.LogError(e.Message);
                }
            }, TaskCreationOptions.LongRunning);
        }

        public void Stop()
        {
            IsWorking = false;
        }
    }
}