using System;
using System.Collections.Generic;
using System.Net;
using CaptainHook.Domain.Results;

namespace CaptainHook.Cli.Commands.ExecuteApi
{
    public class ApiConsumer
    {
        public OperationResult<HttpWebResponse> CallApi(IEnumerable<PutSubscriberRequest> requests)
        {
            throw new NotImplementedException();
        }
    }
}