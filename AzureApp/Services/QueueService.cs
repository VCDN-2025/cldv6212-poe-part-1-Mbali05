using Azure.Storage.Queues;
using Microsoft.Extensions.Configuration;
using System;
using System.Text;
using System.Threading.Tasks;

namespace AzureApp.Services
{
    public class QueueService
    {
        private readonly QueueClient _queueClient;

        public QueueService(IConfiguration configuration)
        {
            var connectionString = configuration["AzureStorage:ConnectionString"];

            // I named Queue "neworders"
            var queueName = "neworders";

            _queueClient = new QueueClient(connectionString, queueName);

            // This is to make sure queue exists
            _queueClient.CreateIfNotExists();
        }

       
        public async Task SendProcessingOrderAsync()
        {
            string message = "Processing order";

            string base64Message = Convert.ToBase64String(Encoding.UTF8.GetBytes(message));
            await _queueClient.SendMessageAsync(base64Message);
        }

        public async Task<string[]> PeekMessagesAsync(int maxMessages = 10)
        {
            var messages = await _queueClient.PeekMessagesAsync(maxMessages);

            return Array.ConvertAll(messages.Value, m =>
                Encoding.UTF8.GetString(Convert.FromBase64String(m.MessageText)));
        }
    }
}
