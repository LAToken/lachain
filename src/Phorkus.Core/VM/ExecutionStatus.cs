﻿namespace Phorkus.Core.VM
{
    public enum ExecutionStatus
    {
        Ok = 0,
        IncompatibleCode = 1,
        CompilationFailure = 2,
        MissingEntrypoint = 3,
        IncorrectCall = 5,
        UnknownError = -1,
        VmCorruption = -2,
    }
}