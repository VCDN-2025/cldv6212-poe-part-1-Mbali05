using AzureApp.Models;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace AzureApp.Controllers
{
    public class CheckoutController : Controller
    {
        // GET: Checkout
        public IActionResult Index()
        {
            var cart = HttpContext.Session.GetObjectFromJson<List<Product>>("Cart")
                       ?? new List<Product>();
            return View(cart);
        }

        [HttpPost]
        public IActionResult AddToCheckout(string id)
        {
            // Get current cart
            var cart = HttpContext.Session.GetObjectFromJson<List<Product>>("Cart")
                       ?? new List<Product>();

            // In a real app you would fetch product from DB (we'll fake here)
            var product = new Product
            {
                RowKey = id,
                ProductName = "Sample Product " + id,
                ProductPrice = 20,
                StockQuantity = 10
            };

            cart.Add(product);

            HttpContext.Session.SetObjectAsJson("Cart", cart);

            return RedirectToAction("Index");
        }

        [HttpPost]
        public IActionResult PlaceOrder()
        {
            var cart = HttpContext.Session.GetObjectFromJson<List<Product>>("Cart")
                       ?? new List<Product>();

            if (!cart.Any())
            {
                TempData["ErrorMessage"] = "Your cart is empty!";
                return RedirectToAction("Index");
            }

            // Create new order for customer
            var order = new Orders
            {
                CustomerId = "GuestCustomer", 
                TotalPrice = cart.Sum(p => p.ProductPrice),
                Quantity = cart.Count,
                OrderDate = DateTime.UtcNow,
                OrderStatus = "Confirmed"
            };

            // Generate invoice text
            var sb = new StringBuilder();
            sb.AppendLine("========= INVOICE =========");
            sb.AppendLine($"Order ID: {order.RowKey}");
            sb.AppendLine($"Date: {order.OrderDate}");
            sb.AppendLine($"Status: {order.OrderStatus}");
            sb.AppendLine();
            sb.AppendLine("Items:");
            foreach (var item in cart)
            {
                sb.AppendLine($"{item.ProductName} - {item.ProductPrice:C}");
            }
            sb.AppendLine();
            sb.AppendLine($"TOTAL: {order.TotalPrice:C}");
            sb.AppendLine("===========================");

            // Save invoice to file
            string folderPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "invoices");
            if (!Directory.Exists(folderPath))
                Directory.CreateDirectory(folderPath);

            string filePath = Path.Combine(folderPath, $"Invoice_{order.RowKey}.txt");
            System.IO.File.WriteAllText(filePath, sb.ToString());

            // Clear cart
            HttpContext.Session.Remove("Cart");

            TempData["SuccessMessage"] = $"Order placed! Invoice saved to {filePath}";
            return RedirectToAction("Index");
        }
    }
}
