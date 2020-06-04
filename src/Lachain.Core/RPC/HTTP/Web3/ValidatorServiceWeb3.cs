﻿using AustinHarris.JsonRpc;
using Lachain.Core.ValidatorStatus;
using Lachain.Logger;

namespace Lachain.Core.RPC.HTTP.Web3
{
    public class ValidatorServiceWeb3 : JsonRpcService
    {
        private readonly IValidatorStatusManager _validatorStatusManager;
        private readonly ILogger<TransactionServiceWeb3> _logger = LoggerFactory.GetLoggerForClass<TransactionServiceWeb3>();
        
        public ValidatorServiceWeb3(
            IValidatorStatusManager validatorStatusManager
            )
        {
            _validatorStatusManager = validatorStatusManager;
        }

        [JsonRpcMethod("validator_start")]
        private string StartValidator()
        {
            _logger.LogDebug("validator start command received");
            if (!_validatorStatusManager.IsStarted())
            {
                _validatorStatusManager.Start(false);
            }

            return "0x1";
        }

        [JsonRpcMethod("validator_status")]
        private string GetValidatorStatus()
        {
            
            return _validatorStatusManager.IsStarted() ? "0x1" : "0x0";
        }

        [JsonRpcMethod("validator_stop")]
        private string StopValidator()
        {
            _logger.LogDebug("validator stop command received");
            _validatorStatusManager.WithdrawStakeAndStop();
            return "0x1";
        }
    }
}