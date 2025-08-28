using Azure;
using Azure.Data.Tables;
using System;

namespace AzureApp.Models
{
    public class OrderItem : ITableEntity
    {
        public string PartitionKey { get; set; } = "OrderItem";
        public string RowKey { get; set; } = Guid.NewGuid().ToString();

        public string OrderId { get; set; }      
        public string ProductId { get; set; }   
        public int Quantity { get; set; }
        public decimal Price { get; set; }

        // Required by ITableEntity
        public DateTimeOffset? Timestamp { get; set; }
        public ETag ETag { get; set; }
    }
}
