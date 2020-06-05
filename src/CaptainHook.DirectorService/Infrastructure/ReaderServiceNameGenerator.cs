using CaptainHook.Common;
using CaptainHook.Common.Configuration;
using CaptainHook.DirectorService.Infrastructure.Interfaces;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace CaptainHook.DirectorService.Infrastructure
{
    public class ReaderServiceNameGenerator: IReaderServiceNameGenerator
    {
        private IDateTimeProvider _dateTimeProvider;        

        public ReaderServiceNameGenerator(IDateTimeProvider dateTimeProvider)
        {
            _dateTimeProvider = dateTimeProvider;
        }

        public string GenerateNewName(SubscriberNaming naming)
        {
            var ticksPerMs = 10_000;
            // generates a different id every millisecond
            var newSuffix = (_dateTimeProvider.UtcNow.Ticks / ticksPerMs).ToString();
            var readerServiceNameUri = ServiceNaming.EventReaderServiceFullUri(naming.EventType, naming.SubscriberName, naming.IsDlqMode);
            return $"{readerServiceNameUri}-{newSuffix}";
        }

        public IList<string> FindOldNames(SubscriberNaming naming, IList<string> serviceList)
        {
            var readerServiceNameUri = ServiceNaming.EventReaderServiceFullUri(naming.EventType, naming.SubscriberName, naming.IsDlqMode);
            var pattern = $@"^{Regex.Escape(readerServiceNameUri)}\b(|-a|-b|-\d{{14}})\b$";
            return serviceList.Where(x => Regex.IsMatch(x, pattern)).ToList();
        }
    }
}
