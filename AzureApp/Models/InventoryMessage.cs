namespace AzureApp.Models
{
    public class InventoryMessage
    {
        public string OrderId { get; set; } = string.Empty;
        public string ProductId { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public string Action { get; set; } = "ProcessOrder";
    }
}
