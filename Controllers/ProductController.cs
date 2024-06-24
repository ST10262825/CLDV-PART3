using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using JamesCrafts.Models;
using System.Net.Http;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace JamesCrafts.Controllers
{
    //public class ProductController : Controller
    //{
    //    private readonly JamesContext _context;
    //    private readonly UserManager<IdentityUser> _userManager;
    //    private readonly IHttpClientFactory _httpClientFactory;

    //    public ProductController(JamesContext context, UserManager<IdentityUser> userManager, IHttpClientFactory httpClientFactory)
    //    {
    //        _context = context;
    //        _userManager = userManager;
    //        _httpClientFactory = httpClientFactory;
    //    }
        public class ProductController : Controller
        {
            private readonly JamesContext _context;
            private readonly UserManager<IdentityUser> _userManager;


            public ProductController(JamesContext context, UserManager<IdentityUser> userManager)
            {
                _context = context;
                _userManager = userManager;

            }


            // Display the list of products
            [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Index()
        {
            var products = await _context.Products.ToListAsync();
            return View(products);
        }

        // Add to cart
        [HttpPost]
        [Authorize]
        public async Task<IActionResult> AddToCart(int productId, int quantity = 1)
        {
            var user = await _userManager.GetUserAsync(User);

            if (user == null)
            {
                return Unauthorized(); // Ensure the user is logged in
            }

            var cart = await _context.Carts
                .Include(c => c.CartItems)
                .FirstOrDefaultAsync(c => c.UserId == user.Id);

            if (cart == null)
            {
                cart = new Cart
                {
                    UserId = user.Id,
                    CartItems = new List<CartItem>()
                };
                _context.Carts.Add(cart);
            }

            var cartItem = cart.CartItems.FirstOrDefault(ci => ci.ProductId == productId);
            if (cartItem == null)
            {
                cartItem = new CartItem
                {
                    ProductId = productId,
                    Quantity = quantity,
                    CartId = cart.CartId
                };
                cart.CartItems.Add(cartItem);
            }
            else
            {
                cartItem.Quantity += quantity;
            }

            await _context.SaveChangesAsync();

            return RedirectToAction("Index"); // Redirect to products list or cart view
        }

        // View cart for logged-in user
        [Authorize]
        public async Task<IActionResult> Cart()
        {
            var user = await _userManager.GetUserAsync(User);

            if (user == null)
            {
                return Unauthorized(); // Ensure the user is logged in
            }

            var cart = await _context.Carts
                .Include(c => c.CartItems)
                .ThenInclude(ci => ci.Product)
                .FirstOrDefaultAsync(c => c.UserId == user.Id);

            // Convert HashSet<CartItem> to List<CartItem>
            var cartItemsList = cart?.CartItems.ToList() ?? new List<CartItem>();

            return View(cartItemsList);
        }



        // Add action to update cart
        [HttpPost]
        [Authorize]
        public async Task<IActionResult> UpdateCart(int productId, int quantity)
        {
            var user = await _userManager.GetUserAsync(User);

            if (user == null)
            {
                return Unauthorized(); // Ensure the user is logged in
            }

            var cart = await _context.Carts
                .Include(c => c.CartItems)
                .FirstOrDefaultAsync(c => c.UserId == user.Id);

            if (cart == null)
            {
                return RedirectToAction("Cart"); // Redirect to cart if no cart found
            }

            var cartItem = cart.CartItems.FirstOrDefault(ci => ci.ProductId == productId);

            if (cartItem == null)
            {
                return RedirectToAction("Cart"); // Redirect to cart if product not found in cart
            }

            if (quantity <= 0)
            {
                // Remove item from cart if quantity is zero or less
                _context.CartItems.Remove(cartItem);
            }
            else
            {
                cartItem.Quantity = quantity;
            }

            await _context.SaveChangesAsync();

            return RedirectToAction("Cart"); // Redirect back to cart view
        }

        // Action to remove a product from cart
        [HttpPost]
        [Authorize]
        public async Task<IActionResult> RemoveFromCart(int productId)
        {
            var user = await _userManager.GetUserAsync(User);

            if (user == null)
            {
                return Unauthorized(); // Ensure the user is logged in
            }

            var cart = await _context.Carts
                .Include(c => c.CartItems)
                .FirstOrDefaultAsync(c => c.UserId == user.Id);

            if (cart == null)
            {
                return RedirectToAction("Cart"); // Redirect to cart if no cart found
            }

            var cartItem = cart.CartItems.FirstOrDefault(ci => ci.ProductId == productId);

            if (cartItem != null)
            {
                _context.CartItems.Remove(cartItem);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction("Cart"); // Redirect back to cart view
        }


        public async Task<IActionResult> MyWork(string category)
        {
            var products = from p in _context.Products
                           select p;

            if (!string.IsNullOrEmpty(category))
            {
                products = products.Where(p => p.Category == category);
            }

            var categories = await _context.Products
                                           .Select(p => p.Category)
                                           .Distinct()
                                           .ToListAsync();

            ViewBag.Categories = new SelectList(categories);
            ViewBag.Products = await products.ToListAsync();

            return View();
        }

        // Handle the creation of a new product
        [HttpPost]
        public async Task<IActionResult> CreateProduct(Product product)
        {
            if (ModelState.IsValid)
            {
                _context.Products.Add(product);
                await _context.SaveChangesAsync();
                return RedirectToAction("Index");
            }
            return View(product);
        }

        public IActionResult Checkout()
        {
            var viewModel = new CheckoutViewModel();
            return View(viewModel);
        }

        [HttpPost]
        public async Task<IActionResult> ProcessPayment(CheckoutViewModel viewModel)
        {
            // Process payment logic here, such as validating inputs, charging the card, etc.

            // Example: Assuming payment is successful, clear the cart
            var user = await _userManager.GetUserAsync(User);

            if (user == null)
            {
                return Unauthorized(); // Ensure the user is logged in
            }

            var cart = await _context.Carts
                .Include(c => c.CartItems)
                .ThenInclude(ci => ci.Product)
                .FirstOrDefaultAsync(c => c.UserId == user.Id);

            if (cart == null)
            {
                return RedirectToAction("PaymentConfirmation"); // Redirect to confirmation page if no cart found
            }

            // Assuming you iterate through cart items to create purchase history entries
            foreach (var cartItem in cart.CartItems)
            {
                var purchaseHistory = new PurchaseHistory
                {
                    UserId = user.Id,
                    PurchaseDate = DateTime.Now, // Or set to the appropriate purchase date
                    ProductName = cartItem.Product.Name,
                    ProductDescription = cartItem.Product.Description, // Ensure this is not null
                    ProductPrice = (decimal)cartItem.Product.Price,
                    Quantity = cartItem.Quantity,
                    IsConfirmed = false // Assuming you need to confirm orders later
                };

                _context.PurchaseHistories.Add(purchaseHistory);
            }

            // Clear cart items
            _context.CartItems.RemoveRange(cart.CartItems);

            await _context.SaveChangesAsync();

            // Redirect to a thank you or confirmation page
            return RedirectToAction("PaymentConfirmation");
        }

        public IActionResult PaymentConfirmation()
        {
            // Display a thank you or confirmation message
            return View();
        }

        [Authorize]
        public async Task<IActionResult> PurchaseHistory()
        {
            var user = await _userManager.GetUserAsync(User);

            if (user == null)
            {
                return Unauthorized(); // Ensure the user is logged in
            }

            var purchaseHistory = await _context.PurchaseHistories
                .Where(ph => ph.UserId == user.Id)
                .OrderByDescending(ph => ph.PurchaseDate)
                .ToListAsync();

            // Assuming PurchaseHistories model includes ProductName, ProductDescription, ProductPrice, Quantity, and IsConfirmed

            var purchaseHistoryViewModel = purchaseHistory.Select(ph => new PurchaseHistory
            {
                Id = ph.Id,
                PurchaseDate = ph.PurchaseDate,
                ProductName = ph.ProductName,
                ProductDescription = ph.ProductDescription,
                ProductPrice = ph.ProductPrice,
                Quantity = ph.Quantity,
                IsConfirmed = ph.IsConfirmed
            }).ToList();

            return View(purchaseHistoryViewModel);
        }

        // Admin: View all orders
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> AllOrders()
        {
            var orders = await _context.PurchaseHistories
                .Include(ph => ph.User) // Include User information
                .ToListAsync();
            return View(orders);
        }

        // Admin: Confirm an order
        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<IActionResult> ConfirmOrder(int orderId)
        {
            var order = await _context.PurchaseHistories.FindAsync(orderId);
            if (order != null)
            {
                order.IsConfirmed = true;
                await _context.SaveChangesAsync();
            }
            return RedirectToAction("AllOrders");
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> RemoveConfirmedOrder(int orderId)
        {
            var order = await _context.PurchaseHistories.FindAsync(orderId);

            if (order == null)
            {
                return NotFound(); // Handle case where order is not found
            }

            // Optionally, you may choose to perform some validation or additional checks here
            _context.PurchaseHistories.Remove(order);
            await _context.SaveChangesAsync();

            return RedirectToAction("AllOrders");
        }



        // GET: Product/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var product = await _context.Products
                .FirstOrDefaultAsync(m => m.ProductId == id);
            if (product == null)
            {
                return NotFound();
            }

            return View(product);
        }

        // GET: Product/Create 
        public IActionResult Create()
        {
            return View();
        }

        // POST: Product/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("ProductId,Name,Price,Category,Availability,Description,ImageUrl")] Product product)
        {
            if (ModelState.IsValid)
            {
                _context.Add(product);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(product);
        }

        // GET: Product/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var product = await _context.Products.FindAsync(id);
            if (product == null)
            {
                return NotFound();
            }
            return View(product);
        }

        // POST: Product/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("ProductId,Name,Price,Category,Availability,Description,ImageUrl")] Product product)
        {
            if (id != product.ProductId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(product);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ProductExists(product.ProductId))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(product);
        }

        // GET: Product/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var product = await _context.Products
                .FirstOrDefaultAsync(m => m.ProductId == id);
            if (product == null)
            {
                return NotFound();
            }

            return View(product);
        }

        // POST: Product/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var product = await _context.Products.FindAsync(id);
            if (product != null)
            {
                _context.Products.Remove(product);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool ProductExists(int id)
        {
            return _context.Products.Any(e => e.ProductId == id);
        }


        //[HttpPost]
        //public async Task<IActionResult> PlaceOrder(PurchaseHistory orderData)
        //{
        //    var client = _httpClientFactory.CreateClient();
        //    var functionUrl = "https://craftsfunctions.azurewebsites.net"; // Replace with your Durable Function URL
        //    var response = await client.PostAsJsonAsync(functionUrl, orderData);

        //    if (response.IsSuccessStatusCode)
        //    {
        //        return RedirectToAction("OrderSuccess");
        //    }
        //    else
        //    {
        //        return RedirectToAction("OrderFailure");
        //    }



        //}
    }
}
