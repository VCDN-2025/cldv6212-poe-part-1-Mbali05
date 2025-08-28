using AzureApp.Models;
using AzureApp.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace AzureApp.Controllers
{
    public class ProductsController : Controller
    {
        private readonly TableService _tableService;
        private readonly BlobService _blobService;
        private readonly FileShareService _fileShareService;
        private readonly QueueService _queueService;

        public ProductsController(
            TableService tableService,
            BlobService blobService,
            FileShareService fileShareService,
            QueueService queueService)
        {
            _tableService = tableService;
            _blobService = blobService;
            _fileShareService = fileShareService;
            _queueService = queueService;
        }

        // --------------------------------------------------
        // ADMIN PART OF THE WEBSITE: Product Management
        // --------------------------------------------------

        public IActionResult Index()
        {
            var products = _tableService.GetProducts().ToList(); // keep as ProductEntity
            return View(products);
        }


        public IActionResult Create() => View(new Product());

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Product product, IFormFile? ImageFile)
        {
            if (!ModelState.IsValid) return View(product);

            if (ImageFile != null)
                product.ImageUrl = await _blobService.UploadFileAsync(ImageFile);

            var entity = new ProductEntity
            {
                PartitionKey = "Product",
                RowKey = System.Guid.NewGuid().ToString(),
                ProductName = product.ProductName,
                ProductPrice = product.ProductPrice,
                StockQuantity = product.StockQuantity,
                Description = product.Description,
                ImageUrl = product.ImageUrl,
                TimesBought = product.TimesBought
            };

            await _tableService.AddProductAsync(entity);
            TempData["SuccessMessage"] = "Product created successfully!";
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Edit(string id)
        {
            if (string.IsNullOrEmpty(id)) return NotFound();

            var entity = await _tableService.GetProductAsync(id);
            if (entity == null) return NotFound();

            return View(MapToProduct(entity));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, Product updatedProduct, IFormFile? ImageFile)
        {
            if (id != updatedProduct.RowKey) return BadRequest();
            if (!ModelState.IsValid) return View(updatedProduct);

            var existingEntity = await _tableService.GetProductAsync(id);
            if (existingEntity == null) return NotFound();

            existingEntity.ProductName = updatedProduct.ProductName;
            existingEntity.ProductPrice = updatedProduct.ProductPrice;
            existingEntity.StockQuantity = updatedProduct.StockQuantity;
            existingEntity.Description = updatedProduct.Description;

            if (ImageFile != null)
                existingEntity.ImageUrl = await _blobService.UploadFileAsync(ImageFile);

            await _tableService.UpdateProductAsync(existingEntity);
            TempData["SuccessMessage"] = "Product updated successfully!";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(string id)
        {
            if (string.IsNullOrEmpty(id)) return BadRequest();

            await _tableService.DeleteProductAsync(id);
            TempData["SuccessMessage"] = "Product deleted successfully!";
            return RedirectToAction(nameof(Index));
        }

        // -----------------------------------------------------------
        // CUSTOMER-FACING SHOP: USERS CAN USE THIS AS AN/1/4 ONLINE SHOP 
        // -----------------------------------------------------------

        public IActionResult Shop()
        {
            var productEntities = _tableService.GetProducts();
            var products = productEntities.Select(pe => MapToProduct(pe)).ToList();

            var customers = _tableService.GetCustomers();
            ViewData["Customers"] = customers;

            var selectedCustomer = HttpContext.Session.GetObjectFromJson<Customer>("SelectedCustomer");
            ViewBag.SelectedCustomer = selectedCustomer;

            return View(products);
        }

        [HttpPost]
        public async Task<IActionResult> SelectCustomer(string customerId)
        {
            if (string.IsNullOrEmpty(customerId)) return RedirectToAction("Shop");

            var customer = await _tableService.GetCustomerAsync(customerId);
            if (customer == null) return RedirectToAction("Shop");

            HttpContext.Session.SetObjectAsJson("SelectedCustomer", customer);
            return RedirectToAction("Shop");
        }

        [HttpPost]
        public IActionResult AddToCart(string productId)
        {
            var selectedCustomer = HttpContext.Session.GetObjectFromJson<Customer>("SelectedCustomer");
            if (selectedCustomer == null)
            {
                TempData["ErrorMessage"] = "Please select your customer profile first.";
                return RedirectToAction("Shop");
            }

            var productEntity = _tableService.GetProduct(productId);
            if (productEntity == null || productEntity.StockQuantity <= 0) return NotFound();

            var cartKey = $"Cart_{selectedCustomer.RowKey}";
            var cart = HttpContext.Session.GetObjectFromJson<List<CartItem>>(cartKey) ?? new List<CartItem>();

            var cartItem = cart.FirstOrDefault(c => c.ProductID == productEntity.RowKey);
            if (cartItem != null)
                cartItem.Quantity += 1;
            else
                cart.Add(new CartItem
                {
                    ProductID = productEntity.RowKey,
                    ProductName = productEntity.ProductName,
                    ProductPrice = productEntity.ProductPrice,
                    Quantity = 1,
                    ImageUrl = productEntity.ImageUrl
                });

            HttpContext.Session.SetObjectAsJson(cartKey, cart);
            return RedirectToAction("Shop");
        }

        public IActionResult Cart()
        {
            var selectedCustomer = HttpContext.Session.GetObjectFromJson<Customer>("SelectedCustomer");
            if (selectedCustomer == null)
            {
                TempData["ErrorMessage"] = "Please select your customer profile first.";
                return RedirectToAction("Shop");
            }

            var cartKey = $"Cart_{selectedCustomer.RowKey}";
            var cartItems = HttpContext.Session.GetObjectFromJson<List<CartItem>>(cartKey) ?? new List<CartItem>();
            return View(cartItems);
        }

        [HttpPost]
        public async Task<IActionResult> GenerateInvoice()
        {
            var customer = HttpContext.Session.GetObjectFromJson<Customer>("SelectedCustomer");
            if (customer == null)
            {
                TempData["ErrorMessage"] = "Please select a customer first.";
                return RedirectToAction("Shop");
            }

            var cartKey = $"Cart_{customer.RowKey}";
            var cartItems = HttpContext.Session.GetObjectFromJson<List<CartItem>>(cartKey) ?? new List<CartItem>();
            if (!cartItems.Any())
            {
                TempData["ErrorMessage"] = "Your cart is empty.";
                return RedirectToAction("Shop");
            }

            // This is to generate an invoice
            var invoiceText = new StringBuilder();
            invoiceText.AppendLine($"Invoice for {customer.Name} ({customer.Email})");
            invoiceText.AppendLine("----------------------------------------------------");
            decimal total = 0;

            foreach (var item in cartItems)
            {
                decimal subtotal = item.ProductPrice * item.Quantity;
                invoiceText.AppendLine($"{item.ProductName} x {item.Quantity} = R {subtotal:N2}");
                total += subtotal;

                // Update inventory
                var productEntity = await _tableService.GetProductAsync(item.ProductID);
                if (productEntity != null)
                {
                    productEntity.StockQuantity -= item.Quantity;
                    if (productEntity.StockQuantity < 0) productEntity.StockQuantity = 0;
                    await _tableService.UpdateProductAsync(productEntity);
                }
            }

            invoiceText.AppendLine("----------------------------------------------------");
            invoiceText.AppendLine($"Total: R {total:N2}");

            // Upload to Azure File Share
            using (var stream = new MemoryStream(Encoding.UTF8.GetBytes(invoiceText.ToString())))
            {
                string fileName = $"Invoice_{customer.RowKey}_{System.DateTime.Now:yyyyMMddHHmmss}.txt";
                await _fileShareService.UploadFileAsync(fileName, stream);
            }

            // Queue order message
            await _queueService.SendProcessingOrderAsync();

            // Clear cart
            HttpContext.Session.Remove(cartKey);

            TempData["SuccessMessage"] = "Invoice generated, stock updated, and order queued!";
            return RedirectToAction("Shop");
        }

        // ---------
        // Helper
        // ---------
        private Product MapToProduct(ProductEntity pe)
        {
            return new Product
            {
                PartitionKey = pe.PartitionKey,
                RowKey = pe.RowKey,
                ProductName = pe.ProductName,
                ProductPrice = pe.ProductPrice,
                StockQuantity = pe.StockQuantity,
                Description = pe.Description,
                ImageUrl = pe.ImageUrl,
                TimesBought = pe.TimesBought,
                Timestamp = pe.Timestamp,
                ETag = pe.ETag
            };
        }
    }
}
