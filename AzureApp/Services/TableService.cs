using Azure;
using Azure.Data.Tables;
using AzureApp.Models;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AzureApp.Services
{
    public class TableService
    {
        private readonly TableClient _customersTable;
        private readonly TableClient _productsTable;
        private readonly TableClient _ordersTable;

        public TableService(IConfiguration configuration)
        {
            var connectionString = configuration["AzureStorage:ConnectionString"];

            _customersTable = new TableClient(connectionString, "Customers");
            _customersTable.CreateIfNotExists();

            _productsTable = new TableClient(connectionString, "Products");
            _productsTable.CreateIfNotExists();

            _ordersTable = new TableClient(connectionString, "Orders");
            _ordersTable.CreateIfNotExists();
        }

        // ==== Products ====
        public async Task AddProductAsync(ProductEntity product)
            => await _productsTable.AddEntityAsync(product);

        public List<ProductEntity> GetProducts()
            => _productsTable.Query<ProductEntity>().ToList();

        public async Task<ProductEntity> GetProductAsync(string rowKey)
            => await _productsTable.GetEntityAsync<ProductEntity>("Product", rowKey);

        public async Task UpdateProductAsync(ProductEntity product)
        {
            
            await _productsTable.UpdateEntityAsync(product, ETag.All, TableUpdateMode.Replace);
        }

        public async Task DeleteProductAsync(string rowKey)
            => await _productsTable.DeleteEntityAsync("Product", rowKey);

        
        public ProductEntity? GetProduct(string rowKey)
        {
            try
            {
                return _productsTable.GetEntity<ProductEntity>("Product", rowKey).Value;
            }
            catch (RequestFailedException)
            {
                return null;
            }
        }

        // ==== Customers ====
        public async Task AddCustomerAsync(Customer customer)
            => await _customersTable.AddEntityAsync(customer);

        public List<Customer> GetCustomers() => _customersTable.Query<Customer>().ToList();

        public async Task<Customer> GetCustomerAsync(string id)
            => await _customersTable.GetEntityAsync<Customer>("Customer", id);

        public async Task UpdateCustomerAsync(Customer customer)
            => await _customersTable.UpdateEntityAsync(customer, ETag.All, TableUpdateMode.Replace);

        public async Task DeleteCustomerAsync(string id)
            => await _customersTable.DeleteEntityAsync("Customer", id);

        // ==== Orders ====
        public async Task AddOrderAsync(Orders order)
            => await _ordersTable.AddEntityAsync(order);

        public List<Orders> GetOrders() => _ordersTable.Query<Orders>().ToList();

        public async Task<Orders> GetOrderAsync(string rowKey)
            => await _ordersTable.GetEntityAsync<Orders>("Order", rowKey);

        public async Task UpdateOrderAsync(Orders order)
            => await _ordersTable.UpdateEntityAsync(order, ETag.All, TableUpdateMode.Replace);

        public async Task DeleteOrderAsync(string rowKey)
            => await _ordersTable.DeleteEntityAsync("Order", rowKey);


    }
}
