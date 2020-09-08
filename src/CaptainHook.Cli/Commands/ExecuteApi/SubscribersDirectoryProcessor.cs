using System;
using System.IO;
using System.IO.Abstractions;
using System.Net;
using System.Threading.Tasks;
using CaptainHook.Api.Client;
using CaptainHook.Api.Client.Models;
using CaptainHook.Cli.Commands.GeneratePowerShell;
using Newtonsoft.Json;

namespace CaptainHook.Cli.Commands.ExecuteApi
{
    public class SubscribersDirectoryProcessor
    {
        private readonly IFileSystem _fileSystem;
        private readonly ICaptainHookClient _captainHookClient;

        public SubscribersDirectoryProcessor(IFileSystem fileSystem, ICaptainHookClient captainHookClient)
        {
            _fileSystem = fileSystem;
            _captainHookClient = captainHookClient;
        }

        public async Task<Result> ProcessDirectory(string inputFolderPath, string environmentName)
        {
            var sourceFolderPath = Path.GetFullPath(inputFolderPath);
            if (!_fileSystem.Directory.Exists(sourceFolderPath))
            {
                return new Result($"Cannot open {inputFolderPath}");
            }

            try
            {
                var subscriberFiles = _fileSystem.Directory.GetFiles(sourceFolderPath, "*.json");
                foreach (var fileName in subscriberFiles)
                {
                    // deserialize
                    var content = _fileSystem.File.ReadAllText(fileName);
                    var subscriberDto = JsonConvert.DeserializeObject<CaptainHookContractSubscriberDto>(content);

                    // call the API
                    var response = await _captainHookClient.PutSuscriberWithHttpMessagesAsync("", "", subscriberDto);
                    if (response.Response.StatusCode != HttpStatusCode.Accepted && response.Response.StatusCode != HttpStatusCode.Created)
                    {
                        return new Result(response.Response.Content.ToString());
                    }
                }
            }
            catch (Exception e)
            {
                return new Result(e.ToString());
            }

            return Result.Valid;
        }
    }
}