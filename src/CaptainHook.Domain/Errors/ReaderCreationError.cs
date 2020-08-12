﻿using CaptainHook.Domain.Entities;
using CaptainHook.Domain.Results;

namespace CaptainHook.Domain.Errors
{
    public class ReaderCreateError : ErrorBase
    {
        public ReaderCreateError(SubscriberEntity subscriber)
            : base($"Can't create Reader Service for Event {subscriber?.ParentEvent?.Name} and Subscriber {subscriber?.Name}")
        {
        }
    }
}