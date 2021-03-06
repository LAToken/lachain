﻿using System;

namespace Lachain.Core.Blockchain.SystemContracts.ContractManager.Attributes
{
    [AttributeUsage(AttributeTargets.Method)]
    public class ContractMethodAttribute : Attribute
    {
        public string Method { get; }
        
        public ContractMethodAttribute(string method)
        {
            Method = method;
        }
    }
}