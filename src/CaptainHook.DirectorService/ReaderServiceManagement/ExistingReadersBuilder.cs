using System;
using System.Collections.Generic;
using System.Linq;
using CaptainHook.Common;

namespace CaptainHook.DirectorService.ReaderServiceManagement
{
    public class ExistingReadersBuilder
    {
        public static IEnumerable<ExistingReaderDefinition> FromNames(IEnumerable<string> deployedServicesNames)
        {
            return deployedServicesNames
                .Where(s => s.StartsWith(
                    $"fabric:/{Constants.CaptainHookApplication.ApplicationName}/{ServiceNaming.EventReaderServiceShortName}",
                    StringComparison.CurrentCultureIgnoreCase))
                .Select(s => new ExistingReaderDefinition(s));
        }
    }
}