﻿using CaptainHook.Domain.Entities;
using CaptainHook.Domain.Results;

namespace CaptainHook.Domain.Errors
{
    public class ReaderDeletionError : ErrorBase
    {
        public ReaderDeletionError(SubscriberEntity subscriber)
            : base($"Can't delete Reader Service for Event {subscriber?.ParentEvent?.Name} and Subscriber {subscriber?.Name}")
        {
        }
    }
}