using AzureApp.Models;
using AzureApp.Services;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

namespace AzureApp.Controllers
{
    public class HomeController : Controller
    {
        private readonly TableService _tableService;

        public HomeController(TableService tableService)
        {
            _tableService = tableService;
        }

        // Welcome page
        public IActionResult Index()
        {
            return View();
        }

        // GET: SignIn form
        [HttpGet]
        public IActionResult SignIn()
        {
            return View(new Customer());
        }

        // POST: SignIn submission
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SignIn([Bind("Name,Email,Phone Number,Address")] Customer customer)
        {
            if (ModelState.IsValid)
            {
                customer.PartitionKey = "Customer";
                customer.RowKey = Guid.NewGuid().ToString();

                await _tableService.AddCustomerAsync(customer);

                return RedirectToAction("Index", "Customers");
            }

            return View(customer);
        }

        // Customer Details
        public async Task<IActionResult> Details(string id)
        {
            if (string.IsNullOrEmpty(id)) return NotFound();

            var customer = await _tableService.GetCustomerAsync(id);
            if (customer == null) return NotFound();

            return View(customer);
        }
    }
}
