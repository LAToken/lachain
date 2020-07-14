using System;
using System.IO;
using System.Security.Cryptography;
using Lachain.Core.Blockchain.Operations;
using Lachain.Core.Config;
using Lachain.Core.DI.Modules;
using Lachain.Core.DI.SimpleInjector;
using Lachain.Crypto;
using Lachain.Crypto.ECDSA;
using Lachain.Proto;
using Lachain.Utility;
using Lachain.Utility.Utils;
using Nethereum.Util;

namespace Lachain.CoreTest
{
    public class TestUtils
    {
        public static SimpleInjectorContainerBuilder GetContainerBuilder(string configPath)
        {
            var configManager = new ConfigManager(configPath, (s, s1) => null);
            var containerBuilder = new SimpleInjectorContainerBuilder(configManager);

            containerBuilder.RegisterModule<StorageModule>();
            containerBuilder.Register<IConfigManager>(configManager);
            containerBuilder.RegisterModule<NetworkModule>();
            containerBuilder.RegisterModule<BlockchainModule>();
            return containerBuilder;
        }

        public static TransactionReceipt GetRandomTransaction()
        {
            var signer = new TransactionSigner();
            byte[] random = new byte[32];
            RNGCryptoServiceProvider rng = new RNGCryptoServiceProvider();
            rng.GetBytes(random);
            var keyPair = new EcdsaKeyPair(random.ToPrivateKey());
            var randomValue = new Random().Next(1, 100);
            var tx = new Transaction
            {
                To = random.Slice(0, 20).ToUInt160(),
                From = keyPair.PublicKey.GetAddress(),
                GasPrice = (ulong) Money.Parse("0.0000001").ToWei(),
                GasLimit = 100000000,
                Nonce = 0,
                Value = Money.Parse($"{randomValue}.0").ToUInt256()
            };
            return signer.Sign(tx, keyPair);
        }

        public static void DeleteTestChainData()
        {
            if (Directory.Exists("ChainTest"))
            {
                Directory.Delete("ChainTest", true);
            }
            if (Directory.Exists("ChainTest2"))
            {
                Directory.Delete("ChainTest2", true);
            }
        }
    }
}