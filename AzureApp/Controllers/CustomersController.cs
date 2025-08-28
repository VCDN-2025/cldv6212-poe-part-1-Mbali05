using AzureApp.Models;
using AzureApp.Services;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using System.Linq;

namespace AzureApp.Controllers
{
    public class CustomersController : Controller
    {
        private readonly TableService _tableService;

        public CustomersController(TableService tableService)
        {
            _tableService = tableService;
        }

        // GET: Customers
        public IActionResult Index()
        {
            var customers = _tableService.GetCustomers();
            return View(customers);
        }

        // GET: Customers/Details/5
        public async Task<IActionResult> Details(string id)
        {
            var customer = await _tableService.GetCustomerAsync(id);
            if (customer == null) return NotFound();
            return View(customer);
        }

        // GET: Customers/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Customers/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Customer customer)
        {
            if (ModelState.IsValid)
            {
                customer.CartTotal = 0; 
                await _tableService.AddCustomerAsync(customer);
                TempData["SuccessMessage"] = "Customer created successfully!";
                return RedirectToAction(nameof(Index));
            }
            return View(customer);
        }

        // GET: Customers/Edit/5
        public async Task<IActionResult> Edit(string id)
        {
            var customer = await _tableService.GetCustomerAsync(id);
            if (customer == null) return NotFound();
            return View(customer);
        }

        // POST: Customers/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, Customer updatedCustomer)
        {
            var existingCustomer = await _tableService.GetCustomerAsync(id);
            if (existingCustomer == null) return NotFound();

            if (ModelState.IsValid)
            {
                // Update only editable fields
                existingCustomer.Name = updatedCustomer.Name;
                existingCustomer.Email = updatedCustomer.Email;
                existingCustomer.PhoneNumber = updatedCustomer.PhoneNumber;
                existingCustomer.Address = updatedCustomer.Address;
                // Preserve CartTotal

                await _tableService.UpdateCustomerAsync(existingCustomer);
                TempData["SuccessMessage"] = "Customer updated successfully!";
                return RedirectToAction(nameof(Index));
            }
            return View(updatedCustomer);
        }

        // GET: Customers/Delete/5
        public async Task<IActionResult> Delete(string id)
        {
            var customer = await _tableService.GetCustomerAsync(id);
            if (customer == null) return NotFound();
            return View(customer);
        }

        // POST: Customers/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            await _tableService.DeleteCustomerAsync(id);
            TempData["SuccessMessage"] = "Customer deleted successfully!";
            return RedirectToAction(nameof(Index));
        }
    }
}
