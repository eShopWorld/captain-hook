﻿namespace CaptainHook.Domain.Results
{
    public class Failure : IFailure
    {
        public string Code { get; }
        public string Message { get; }

        public Failure(string code, string message)
        {
            Code = code;
            Message = message;
        }
    }
}