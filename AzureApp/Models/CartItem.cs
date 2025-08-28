namespace AzureApp.Models
{
    public class CartItem
    {
        public string ProductID { get; set; } = string.Empty;  // match Product.RowKey
        public string ProductName { get; set; } = string.Empty;
        public decimal ProductPrice { get; set; }
        public int Quantity { get; set; } = 1;

        // Optional: store Blob URL for image
        public string? ImageUrl { get; set; }
    }
}
