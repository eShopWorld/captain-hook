﻿using System.IO;
using Newtonsoft.Json;

namespace CaptainHook.Common.ServiceModels
{
    public class EventReaderInitData
    {
        public string EventType { get; set; }
        public string SuscriberName { get; set; }

        public static string GetReaderInitDataAsString(string @event, string sub)
        {
            using (var sw = new StringWriter())
            {
                using (var writer = new JsonTextWriter(sw))
                {
                    JsonSerializer.CreateDefault().Serialize(writer, new EventReaderInitData
                    { SuscriberName = sub, EventType = @event });
                    writer.Flush();
                    return sw.ToString();
                }
            }
        }
    }
}