﻿namespace CaptainHook.Common.Configuration
{
    /// <summary>
    /// Default service setting used by the director to create instances of a service with the fabric client
    /// </summary>
    public class DefaultServiceSettings
    {
        public int DefaultTargetReplicaSetSize { get; set; }

        public int DefaultMinReplicaSetSize { get; set; }

        public int DefaultPartitionCount { get; set; }
    }
}