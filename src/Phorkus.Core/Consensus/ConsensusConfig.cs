﻿using System.Collections.Generic;
using Newtonsoft.Json;

namespace Phorkus.Core.Consensus
{
    public class ConsensusConfig
    {
        [JsonProperty("validators")]
        public List<string> ValidatorsKeys { get; set; }

        [JsonProperty("privateKey")]
        public string PrivateKey { get; set; }
    }
}