using Azure.Data.Tables;
using AzureApp.Services;

var builder = WebApplication.CreateBuilder(args);

// Add MVC services
builder.Services.AddControllersWithViews();

// Add session support
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
    options.IdleTimeout = TimeSpan.FromHours(1);
});

// Read Azure Storage connection string
var azureConfig = builder.Configuration.GetSection("AzureStorage");
string connectionString = azureConfig.GetValue<string>("ConnectionString");

// Registered Azure Services
builder.Services.AddSingleton(sp => new TableServiceClient(connectionString));
builder.Services.AddSingleton<TableService>();      // TableClient for tables
builder.Services.AddSingleton<BlobService>();       // Blob storage
builder.Services.AddSingleton<QueueService>();      // Queue storage
builder.Services.AddSingleton<FileShareService>();  // File share storage

var app = builder.Build();

// Middleware
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

// Enable session
app.UseSession();

app.UseAuthorization();

// Routes
app.MapControllerRoute(
    name: "product-redirect",
    pattern: "Product",
    defaults: new { controller = "Products", action = "Index" }
);

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}"
);

app.Run();
