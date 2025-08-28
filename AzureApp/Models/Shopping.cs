namespace AzureApp.Models
{
    public class Shopping
    {
        public string ProductId { get; set; }    
        public string CustomerId { get; set; }   
        public int Quantity { get; set; }
        public decimal TotalPrice { get; set; }

     
        public string ProductName { get; set; }
        public decimal ProductPrice { get; set; }
    }
}
