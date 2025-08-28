using Azure;
using Azure.Data.Tables;
using System;

namespace AzureApp.Models
{
    public class Orders : ITableEntity
    {
        // Azure Table properties
        public string PartitionKey { get; set; } = "Order";
        public string RowKey { get; set; } = Guid.NewGuid().ToString();
        public DateTimeOffset? Timestamp { get; set; }
        public ETag ETag { get; set; }

        // Order properties
        public required string CustomerId { get; set; }
        public string? ProductId { get; set; }
        public int Quantity { get; set; } = 1;
        public DateTime OrderDate { get; set; } = DateTime.UtcNow;
        public string OrderStatus { get; set; } = "Pending";
        public decimal TotalPrice { get; set; }

        // Optional navigation properties
        public string? ProductName { get; set; }

        // Blob image URL for product
        public string? ImageUrl { get; set; }

        // Navigation for views (optional)
        public Customer? Customer { get; set; }
        public Product? Product { get; set; }

        // RowKey as OrderID
        public string OrderID => RowKey;
    }
}
