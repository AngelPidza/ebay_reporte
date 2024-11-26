using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ebay.Models;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using System.Security.Claims;
using ebay.services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;


namespace ebay.Controllers
{
    public class AccountController : Controller
    {
        private readonly AuthService _authService;
        private readonly ILogger<AccountController> _logger;

        public AccountController(ILogger<AccountController> logger,AuthService authService)
        {
            _authService = authService;
            _logger = logger;
        }

        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(string email, string password)
        {
            _logger.LogInformation($"Attempting login for user: '{email}'");

            if (ModelState.IsValid)
            {
                var (succeeded, userData, errorMessage) = await _authService.LoginAsync(email, password);

                if (succeeded)
                {
                    if (userData == null || string.IsNullOrEmpty(userData.Username) || string.IsNullOrEmpty(userData.UserId.ToString()))
                    {
                        _logger.LogError("User data is incomplete or null.");
                        ModelState.AddModelError(string.Empty, "An error occurred while processing your login.");
                        return View();
                    }

                    _logger.LogInformation($"Password verified for user: {userData.Username}");

                    // Crear los Claims con el userId extraído del token
                    var claims = new List<Claim>
                    {
                        new Claim(ClaimTypes.Name, userData.Username),
                        new Claim(ClaimTypes.NameIdentifier, userData.UserId.ToString()) // Usar el userId obtenido
                    };

                    var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                    var authProperties = new AuthenticationProperties();

                    await HttpContext.SignInAsync(
                        CookieAuthenticationDefaults.AuthenticationScheme,
                        new ClaimsPrincipal(claimsIdentity),
                        authProperties);

                    _logger.LogInformation($"User {userData.Username} logged in successfully");
                    return RedirectToAction("Index", "Home");
                }
                else
                {
                    _logger.LogWarning($"Login failed for user: '{email}', Reason: {errorMessage}");
                    ModelState.AddModelError(string.Empty, errorMessage ?? "Invalid email or password.");
                }
            }
            else
            {
                _logger.LogWarning("Invalid model state in login attempt");
                foreach (var modelState in ModelState.Values)
                {
                    foreach (var error in modelState.Errors)
                    {
                        _logger.LogWarning($"Model Error: {error.ErrorMessage}");
                    }
                }
            }

            return View();
        }


        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Register(string username, string email, string password)
        {
            // Intenta registrar al usuario a través de la API externa
            var isRegistered = await _authService.RegisterAsync(username, email, password);

            if (isRegistered) return RedirectToAction("Index", "Home");
            
            ModelState.AddModelError(string.Empty, "Registration failed. Please try again.");
            
            return View();

            // Redirige al usuario a la página de login si se registra con éxito
        }

        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Index", "Home");
        }

        private bool VerifyPassword(string password, string hashedPassword)
        {
            // En una aplicación real, usarías una función de hash segura.
            // Este es solo un ejemplo simple.
            return hashedPassword == HashPassword(password);
        }

        private string HashPassword(string password)
        {
            // En una aplicación real, usarías una función de hash segura.
            // Este es solo un ejemplo simple.
            return Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(password));
        }
    }
    public class ProductsController : Controller
    {
        private readonly ProductService _productService;

        public ProductsController(ProductService productService)
        {
            _productService = productService;
        }

        // GET: Products
        public async Task<IActionResult> Index()
        {
            try
            {
                var products = await _productService.GetProductsAsync();
                return View(products);
            }
            catch (Exception ex)
            {
                // Manejo de errores
                return View("Error", new ErrorViewModel { RequestId = ex.Message });
            }
        }

        // GET: Products/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            try
            {
                var product = await _productService.GetProductDetailsAsync(id.Value);
                return View(product);
            }
            catch (HttpRequestException)
            {
                return NotFound();
            }
        }
        
        public async Task<IActionResult> ExportToExcel()
        {
            try
            {
                var excelData = await _productService.ExportProductsToExcelAsync();
                string fileName = $"Products_{DateTime.Now:yyyyMMdd}.xlsx";
                return File(excelData, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
            }
            catch (Exception ex)
            {
                return View("Error", new ErrorViewModel { RequestId = ex.Message });
            }
        }
        
        public async Task<IActionResult> Excel()
        {
            try
            {
                var products = await _productService.GetProductsAsync();
                return View(products);
            }
            catch (Exception ex)
            {
                return View("Error", new ErrorViewModel { RequestId = ex.Message });
            }
        }
    }

    public class OrdersController : Controller
    {
        private readonly GameWorldContext _context;

        public OrdersController(GameWorldContext context)
        {
            _context = context;
        }

        // GET: Orders
        public async Task<IActionResult> Index()
        {
            // Obtener el ID del usuario autenticado desde los claims
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (userId == null)
            {
                return Unauthorized(); // Si no está autenticado, redirigir o manejar de acuerdo a tus necesidades
            }

            var orders = await _context.Orders
                .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Product)
                .Where(o => o.UserId.ToString() == userId)
                .OrderByDescending(o => o.OrderDate)
                .ToListAsync();

            return View(orders);
        }

        // GET: Orders/Details/5
        public async Task<IActionResult> Details(String? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var order = await _context.Orders
                .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Product)
                .FirstOrDefaultAsync(m => m.OrderId == id);

            if (order == null)
            {
                return NotFound();
            }

            return View(order);
        }

        [HttpPost]
        [Authorize] // Asegura que el usuario esté autenticado
        public async Task<IActionResult> AddToCart(int productId)
        {
            // Obtener el nombre de usuario desde los claims
            var username = User.Identity?.Name;

            if (username == null)
            {
                return RedirectToAction("Login", "Account");
            }

            // Buscar el usuario en la base de datos por su nombre de usuario
            var user = await _context.Users.SingleOrDefaultAsync(u => u.Username == username);

            if (user == null)
            {
                return RedirectToAction("Login", "Account");
            }

            // Verificar si el producto existe en la base de datos
            var product = await _context.Products.SingleOrDefaultAsync(p => p.ProductId == productId);
            if (product == null)
            {
                return NotFound(); // El producto no existe
            }

            // Verificar si el usuario tiene una orden abierta (sin completar)
            DateTime dummyDate = DateTime.Now; // Fecha ficticia para órdenes sin completar
                                               // Verificar si el usuario tiene una orden abierta (sin completar)
            var order = await _context.Orders
                .Include(o => o.OrderItems)
                .FirstOrDefaultAsync(o => o.UserId == user.UserId && o.OrderDate == DateTime.Now);

            if (order == null)
            {
                // Si no hay una orden abierta, crear una nueva
                order = new Order
                {
                    UserId = user.UserId,
                    ShippingAddress = "Address placeholder",
                    TotalAmount = 0,
                };

                _context.Orders.Add(order);
                await _context.SaveChangesAsync();
            }

            // Verificar si el producto ya está en la orden
            var orderItem = order.OrderItems.FirstOrDefault(oi => oi.ProductId == product.ProductId);
            if (orderItem != null)
            {
                // Si el producto ya está en la orden, solo aumentamos la cantidad
                orderItem.Quantity++;
            }
            else
            {
                // Si el producto no está en la orden, agregar un nuevo item
                orderItem = new OrderItem
                {
                    OrderId = order.OrderId,
                    ProductId = product.ProductId,
                    Quantity = 1,
                    UnitPrice = product.Price,
                };
                _context.OrderItems.Add(orderItem);
            }

            // Actualizar el monto total de la orden
            order.TotalAmount = order.OrderItems.Sum(item => item.Quantity * item.UnitPrice) + product.Price;

            await _context.SaveChangesAsync();

            // Redirigir a la vista de órdenes o a la página del carrito
            return RedirectToAction("Index", "Orders");
        }
    }


    public class UsersController : Controller
    {
        private readonly GameWorldContext _context;

        public UsersController(GameWorldContext context)
        {
            _context = context;
        }

        // GET: Users
        public async Task<IActionResult> Index()
        {
            return View(await _context.Users.ToListAsync());
        }

        // GET: Users/Details/5
        public async Task<IActionResult> Details(string? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var user = await _context.Users
                .Include(u => u.Orders)
                .Include(u => u.WishLists)
                .FirstOrDefaultAsync(m => m.UserId == id);

            if (user == null)
            {
                return NotFound();
            }

            return View(user);
        }
    }
}