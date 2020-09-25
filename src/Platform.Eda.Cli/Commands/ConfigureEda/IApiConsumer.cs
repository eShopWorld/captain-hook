using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CaptainHook.Api.Client;
using Platform.Eda.Cli.Commands.ConfigureEda.Models;

namespace Platform.Eda.Cli.Commands.ConfigureEda
{
    public interface IApiConsumer
    {
        IAsyncEnumerable<ApiOperationResult> CallApiAsync(IEnumerable<PutSubscriberFile> files);
    }
}
