using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using AzureApp.Models;

namespace AzureApp.Data
{
    public class AzureAppContext : DbContext
    {
        public AzureAppContext (DbContextOptions<AzureAppContext> options)
            : base(options)
        {
        }

        public DbSet<AzureApp.Models.Customer> Customer { get; set; } = default!;
        public DbSet<AzureApp.Models.Images> Images { get; set; } = default!;
        public DbSet<AzureApp.Models.Product> Products { get; set; } = default!;
        public DbSet<AzureApp.Models.Orders> Orders { get; set; } = default!;
        public DbSet<AzureApp.Models.Shopping> Shopping { get; set; } = default!;
    }
}
