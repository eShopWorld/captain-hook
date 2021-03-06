﻿using CaptainHook.Domain.Results;

namespace Platform.Eda.Cli.Common
{
    public class CliExecutionError : ErrorBase
    {
        public CliExecutionError(string message, params IFailure[] failures) : base(message, failures)
        {
        }
    }
}