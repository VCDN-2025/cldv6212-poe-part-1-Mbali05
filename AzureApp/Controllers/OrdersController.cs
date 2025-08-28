using AzureApp.Models;
using AzureApp.Services;
using Microsoft.AspNetCore.Mvc;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace AzureApp.Controllers
{
    public class OrdersController : Controller
    {
        private readonly TableService _tableService;
        private readonly QueueService _queueService;
        private readonly BlobService _blobService;
        private readonly FileShareService _fileService;

        public OrdersController(
            TableService tableService,
            QueueService queueService,
            BlobService blobService,
            FileShareService fileService)
        {
            _tableService = tableService;
            _queueService = queueService;
            _blobService = blobService;
            _fileService = fileService;
        }

        public IActionResult Index()
        {
            var orders = _tableService.GetOrders(); 
            return View(orders);
        }

        public IActionResult Create()
        {
            ViewData["Customers"] = _tableService.GetCustomers();
            ViewData["Products"] = _tableService.GetProducts();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Orders order, IFormFile? productImage, IFormFile? invoiceFile)
        {
            if (!ModelState.IsValid)
                return View(order);

            // Upload product image to Blob
            if (productImage != null)
            {
                order.ImageUrl = await _blobService.UploadFileAsync(productImage);
            }

            // Save order to Table Storage
            order.PartitionKey = "Order";
            order.RowKey = Guid.NewGuid().ToString();
            await _tableService.AddOrderAsync(order); 

            // Upload invoice to Azure Files
            if (invoiceFile != null)
            {
                using var ms = new MemoryStream();
                await invoiceFile.CopyToAsync(ms);
                ms.Position = 0;
                await _fileService.UploadFileAsync(invoiceFile.FileName, ms);
            }

            // Send inventory message
            var inventoryMessage = new InventoryMessage
            {
                OrderId = order.RowKey,
                ProductId = order.ProductId ?? string.Empty,
                Quantity = order.Quantity
            };
            await _queueService.SendProcessingOrderAsync();


            TempData["SuccessMessage"] = "Order created successfully!";
            return RedirectToAction(nameof(Index));
        }
    }
}
