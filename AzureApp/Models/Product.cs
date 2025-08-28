using Azure;
using Azure.Data.Tables;
using Microsoft.AspNetCore.Http;
using System;

namespace AzureApp.Models
{
    public class Product : ITableEntity
    {
        public string PartitionKey { get; set; } = "Product";
        public string RowKey { get; set; } = Guid.NewGuid().ToString();

        public string ProductName { get; set; } = string.Empty;
        public decimal ProductPrice { get; set; }
        public int StockQuantity { get; set; }
        public string? Description { get; set; }

        public string? ImageUrl { get; set; }

        public int TimesBought { get; set; } = 0;

       
        [System.ComponentModel.DataAnnotations.Schema.NotMapped]
        public IFormFile? ImageFile { get; set; }

        public DateTimeOffset? Timestamp { get; set; }
        public ETag ETag { get; set; }
    }
}
