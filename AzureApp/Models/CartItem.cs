namespace AzureApp.Models
{
    public class CartItem
    {
        public string ProductID { get; set; } = string.Empty;  
        public string ProductName { get; set; } = string.Empty;
        public decimal ProductPrice { get; set; }
        public int Quantity { get; set; } = 1;
        public string? ImageUrl { get; set; }
    }
}
