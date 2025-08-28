namespace AzureApp.Models
{
    public class Shopping
    {
        public string ProductId { get; set; }    // Table RowKey
        public string CustomerId { get; set; }   // Table RowKey
        public int Quantity { get; set; }
        public decimal TotalPrice { get; set; }

        // Optional: lightweight reference
        public string ProductName { get; set; }
        public decimal ProductPrice { get; set; }
    }
}
