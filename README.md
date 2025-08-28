# Azure E-Commerce Web App

This is a simple **ASP.NET Core MVC application** that demonstrates an end-to-end e-commerce workflow powered by **Azure Storage services**.  
The project includes product management, customer shopping, checkout with invoice generation, inventory updates, and order queuing.

---

## Features

### Admin
- Product Management:
  - Add new products with images
  - Edit existing products
  - Delete products  
- Product images are uploaded to **Azure Blob Storage**  
- Product details are stored in **Azure Table Storage**

### Customers
- Shop – Browse products and add them to your cart  
- Cart – View cart contents and adjust quantities  
- Checkout – Generate an invoice and complete the order  

When an order is placed:
1. An invoice (`.txt` file) is created and uploaded to **Azure File Share**
2. Product stock levels are reduced automatically
3. An order message (`"Processing order"`) is pushed to the **Azure Queue** (`neworders`)
4. The shopping cart is cleared

---

## Tech Stack

- ASP.NET Core MVC – web application framework  
- Azure Table Storage – product & customer data  
- Azure Blob Storage – product images  
- Azure File Share – invoices  
- Azure Queue Storage – order processing messages  
- Session State – cart management per customer  

---

## How it works (Flow)

1. Admin adds products → stored in Table + Blob  
2. Customer logs in / selects profile  
3. Customer adds products to cart (Session-based)  
4. Checkout:
   - Invoice saved to File Share
   - Inventory updated in Table
   - `"Processing order"` message sent to Queue
   - Cart cleared  
5. Background service (optional) can listen on the queue and process orders further

---

## Project Structure (simplified)


---

## Azure Integration

- **Blob Storage** – used for images (uploaded when creating/editing products)  
- **Table Storage** – used to store product + customer data with PartitionKey/RowKey  
- **File Share** – used to store invoices as text files  
- **Queue Storage** – used to signal that an order has been placed (message: `"Processing order"`)  

---

## Demo Links

- Unlisted YouTube  Walkthrough Video: https://youtu.be/ZToAlsZ5h5g
- Live App (MVC Published): https://st10445933-e9fuhpdug6fzdrc7.southafricanorth-01.azurewebsites.net/

---

## Notes

- This project is designed as a **learning/demo application** to show how Azure Storage services can be integrated into a real-world scenario.  
- It's a foundation that can be extended with:
  - Payment integration
  - Order history for customers
  - Background workers for queue processing  
