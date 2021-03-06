﻿using System;
using System.Linq;
using Newtonsoft.Json;

namespace Lachain.Core.RPC
{
    [JsonObject]
    public class RpcConfig
    {
        public static readonly RpcConfig Default = new RpcConfig
        {
            Hosts = new[]
            {
                "localhost"
            },
            Port = 7070,
            MetricsPort = 7071,
            ApiKey = new string(Enumerable.Repeat("ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789", 16)
                .Select(s => s[new Random().Next(s.Length)]).ToArray())
        };
        
        [JsonProperty("hosts")]
        public string[]? Hosts { get; set; }

        [JsonProperty("port")]
        public ushort Port { get; set; }

        [JsonProperty("metricsPort")]
        public ushort MetricsPort { get; set; }

        [JsonProperty("apiKey")]
        public string? ApiKey { get; set; }
    }
}