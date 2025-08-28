using Azure;
using Azure.Storage.Files.Shares;
using Azure.Storage.Files.Shares.Models;
using Microsoft.Extensions.Configuration;
using System.IO;
using System.Threading.Tasks;

namespace AzureApp.Services
{
    public class FileShareService
    {
        private readonly ShareClient _shareClient;
        private readonly string _directoryName;

        public FileShareService(IConfiguration configuration)
        {
            var connectionString = configuration["AzureStorage:ConnectionString"];
            var shareName = configuration["AzureStorage:FileShareName"] ?? "contracts";
            _directoryName = configuration["AzureStorage:FileDirectoryName"] ?? "";

            _shareClient = new ShareClient(connectionString, shareName);
            _shareClient.CreateIfNotExists();
        }

        public async Task UploadFileAsync(string fileName, Stream stream)
        {
            var directory = _shareClient.GetRootDirectoryClient();
            if (!string.IsNullOrEmpty(_directoryName))
            {
                directory = directory.GetSubdirectoryClient(_directoryName);
                await directory.CreateIfNotExistsAsync();
            }

            var fileClient = directory.GetFileClient(fileName);
            await fileClient.CreateAsync(stream.Length);
            await fileClient.UploadRangeAsync(
                new HttpRange(0, stream.Length),
                stream);
        }
    }
}
