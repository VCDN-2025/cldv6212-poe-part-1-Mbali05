using AzureApp.Models;
using AzureApp.Services;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace AzureApp.Controllers
{
    public class ShoppingController : Controller
    {
        private readonly TableService _tableService;
        private readonly QueueService _queueService;

        public ShoppingController(TableService tableService, QueueService queueService)
        {
            _tableService = tableService;
            _queueService = queueService;
        }

        // Displays all products
        public IActionResult Index()
        {
            var products = _tableService.GetProducts();
            return View(products);
        }

        // This is to add product to cart ( and is stored in session)
        public IActionResult AddToCart(string id)
        {
            var product = _tableService.GetProduct(id);
            if (product == null || product.StockQuantity <= 0) return NotFound();

            var cart = HttpContext.Session.GetObjectFromJson<List<CartItem>>("Cart")
                       ?? new List<CartItem>();

            var cartItem = cart.FirstOrDefault(c => c.ProductID == product.RowKey);
            if (cartItem != null)
            {
                cartItem.Quantity += 1;
            }
            else
            {
                cart.Add(new CartItem
                {
                    ProductID = product.RowKey,
                    ProductName = product.ProductName,
                    ProductPrice = product.ProductPrice,
                    Quantity = 1,
                    ImageUrl = product.ImageUrl
                });
            }

            HttpContext.Session.SetObjectAsJson("Cart", cart);
            return RedirectToAction("Cart");
        }

        // Shows the cart
        public IActionResult Cart()
        {
            var cartItems = HttpContext.Session.GetObjectFromJson<List<CartItem>>("Cart")
                            ?? new List<CartItem>();
            return View(cartItems);
        }

        // Checkout: Reduce the stock when added to cart and send inventory messages
        public async Task<IActionResult> Checkout()
        {
            var cartItems = HttpContext.Session.GetObjectFromJson<List<CartItem>>("Cart")
                            ?? new List<CartItem>();
            if (!cartItems.Any()) return RedirectToAction("Cart");

            foreach (var item in cartItems)
            {
                var product = _tableService.GetProduct(item.ProductID);
                if (product != null)
                {
                    product.StockQuantity -= item.Quantity;
                    await _tableService.UpdateProductAsync(product);

                    // Send inventory update message
                    var inventoryMessage = new InventoryMessage
                    {
                        ProductId = product.RowKey,
                        Quantity = item.Quantity,
                        Action = "ProcessOrder"
                    };
                    await _queueService.SendProcessingOrderAsync();

                }
            }

            HttpContext.Session.Remove("Cart");
            TempData["SuccessMessage"] = "Order placed successfully!";
            return RedirectToAction("Index");
        }
    }

    // Extension methods for session serialization
    public static class SessionExtensions
    {
        public static void SetObjectAsJson(this ISession session, string key, object value)
            => session.SetString(key, JsonSerializer.Serialize(value));

        public static T? GetObjectFromJson<T>(this ISession session, string key)
        {
            var value = session.GetString(key);
            return value == null ? default : JsonSerializer.Deserialize<T>(value);
        }
    }
}
