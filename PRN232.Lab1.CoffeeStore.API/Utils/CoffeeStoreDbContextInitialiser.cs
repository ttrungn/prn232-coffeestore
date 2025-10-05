using Microsoft.AspNetCore.Identity;
using PRN232.Lab1.CoffeeStore.Repositories.Constants;
using PRN232.Lab1.CoffeeStore.Repositories.Models;

namespace PRN232.Lab1.CoffeeStore.API.Utils;

public static class InitializerExtensions
{
    public static async Task InitialiseDatabaseAsync(this WebApplication app)
    {
        using var scope = app.Services.CreateScope();

        var initialiser = scope.ServiceProvider.GetRequiredService<ApplicationDbContextInitializer>();

        await initialiser.InitialiseAsync();
        await initialiser.SeedAsync();
    }
}

public class ApplicationDbContextInitializer
{
    private readonly ILogger<ApplicationDbContextInitializer> _logger;
    private readonly CoffeeStoreDbContext _context;
    private readonly RoleManager<IdentityRole> _roleManager;
    private readonly UserManager<User> _userManager;
    
    public ApplicationDbContextInitializer(
        ILogger<ApplicationDbContextInitializer> logger, 
        CoffeeStoreDbContext coffeeStoreDbContext, 
        RoleManager<IdentityRole> roleManager,
        UserManager<User> userManager)
    {
        _logger = logger;
        _context = coffeeStoreDbContext;
        _roleManager = roleManager;
        _userManager = userManager;
    }

    public async Task InitialiseAsync()
    {
        try
        {
            await _context.Database.EnsureDeletedAsync();
            await _context.Database.EnsureCreatedAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while initialising the database.");
            throw;
        }
    }

    public async Task SeedAsync()
    {
        try
        {
            await TrySeedAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while seeding the database.");
            throw;
        }
    }

    private async Task TrySeedAsync()
    {
        // Seed Roles
        var customerRole = new IdentityRole(Roles.Customer);
        var adminRole = new IdentityRole(Roles.Admin);

        if (_roleManager.Roles.All(r => r.Name != customerRole.Name))
        {
            await _roleManager.CreateAsync(customerRole);
        }

        if (_roleManager.Roles.All(r => r.Name != adminRole.Name))
        {
            await _roleManager.CreateAsync(adminRole);
        }

        // Default users
        var customer = new User()
        {
            Id = "8bb58df8-a28f-47eb-9036-9e102a8134f8", 
            UserName = "customer",
            Email = "customer@coffeestore.localhost",
            FirstName = "Test",
            LastName = "Customer",
            EmailConfirmed = true
        };

        if (_userManager.Users.All(u => u.UserName != customer.UserName))
        {
            await _userManager.CreateAsync(customer, "12345aA#");
            if (!string.IsNullOrWhiteSpace(customerRole.Name))
            {
                await _userManager.AddToRolesAsync(customer, [customerRole.Name]);
            }
        }

        var admin = new User()
        {
            Id = "a11a11a1-a11a-11a1-a11a-11a1a11a11a1",
            UserName = "admin",
            Email = "admin@coffeestore.localhost",
            FirstName = "System",
            LastName = "Administrator",
            EmailConfirmed = true
        };

        if (_userManager.Users.All(u => u.UserName != admin.UserName))
        {
            await _userManager.CreateAsync(admin, "Admin123!@#");
            if (!string.IsNullOrWhiteSpace(adminRole.Name))
            {
                await _userManager.AddToRolesAsync(admin, [adminRole.Name]);
            }
        }
        
        // Seed Categories
        var categories = new List<Category>
        {
            new Category
            {
                Id = Guid.Parse("11111111-1111-1111-1111-111111111111"),
                Name = "Hot Coffee",
                Description = "Traditional hot coffee beverages",
                IsActive = true
            },
            new Category
            {
                Id = Guid.Parse("22222222-2222-2222-2222-222222222222"),
                Name = "Cold Coffee",
                Description = "Refreshing iced and cold coffee drinks",
                IsActive = true
            },
            new Category
            {
                Id = Guid.Parse("33333333-3333-3333-3333-333333333333"),
                Name = "Tea",
                Description = "Various tea options",
                IsActive = true
            },
            new Category
            {
                Id = Guid.Parse("44444444-4444-4444-4444-444444444444"),
                Name = "Pastries",
                Description = "Fresh baked goods and pastries",
                IsActive = true
            },
            new Category
            {
                Id = Guid.Parse("55555555-5555-5555-5555-555555555555"),
                Name = "Smoothies & Juices",
                Description = "Fresh fruit smoothies and natural juices",
                IsActive = true
            },
            new Category
            {
                Id = Guid.Parse("66666666-6666-6666-6666-666666666666"),
                Name = "Sandwiches",
                Description = "Freshly made sandwiches and wraps",
                IsActive = true
            },
            new Category
            {
                Id = Guid.Parse("77777777-7777-7777-7777-777777777777"),
                Name = "Desserts",
                Description = "Sweet treats and desserts"
            }
        };

        // Seed Products (Much more comprehensive data)
        var products = new List<Product>
        {
            // Hot Coffee Products (20 items)
            new Product
            {
                Id = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"),
                Name = "Espresso",
                Price = 123000,
                Description = "Rich and bold single shot of espresso",
                CategoryId = Guid.Parse("11111111-1111-1111-1111-111111111111"),
                IsActive = true
            },
            new Product
            {
                Id = Guid.Parse("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb"),
                Name = "Double Espresso",
                Price = 123000,
                Description = "Two shots of rich espresso for extra strength",
                CategoryId = Guid.Parse("11111111-1111-1111-1111-111111111111"),
                IsActive = true
            },
            new Product
            {
                Id = Guid.Parse("cccccccc-cccc-cccc-cccc-cccccccccccc"),
                Name = "Americano",
                Price = 123000,
                Description = "Espresso with hot water, smooth and strong",
                CategoryId = Guid.Parse("11111111-1111-1111-1111-111111111111"),
                IsActive = true
            },
            new Product
            {
                Id = Guid.Parse("dddddddd-dddd-dddd-dddd-dddddddddddd"),
                Name = "Cappuccino",
                Price = 123000,
                Description = "Espresso with steamed milk and foam",
                CategoryId = Guid.Parse("11111111-1111-1111-1111-111111111111"),
                IsActive = true
            },
            new Product
            {
                Id = Guid.Parse("eeeeeeee-eeee-eeee-eeee-eeeeeeeeeeee"),
                Name = "Latte",
                Price = 123000,
                Description = "Espresso with steamed milk and light foam",
                CategoryId = Guid.Parse("11111111-1111-1111-1111-111111111111"),
                IsActive = true
            },
            new Product
            {
                Id = Guid.Parse("ffffffff-ffff-ffff-ffff-ffffffffffff"),
                Name = "Macchiato",
                Price = 123000,
                Description = "Espresso with a dollop of steamed milk foam",
                CategoryId = Guid.Parse("11111111-1111-1111-1111-111111111111"),
                IsActive = true
            },
            new Product
            {
                Id = Guid.Parse("a1a1a1a1-a1a1-a1a1-a1a1-a1a1a1a1a1a1"),
                Name = "Mocha",
                Price = 123000,
                Description = "Espresso with chocolate syrup and steamed milk",
                CategoryId = Guid.Parse("11111111-1111-1111-1111-111111111111"),
                IsActive = true
            },
            new Product
            {
                Id = Guid.Parse("b2b2b2b2-b2b2-b2b2-b2b2-b2b2b2b2b2b2"),
                Name = "Flat White",
                Price = 123000,
                Description = "Double espresso with steamed milk, no foam",
                CategoryId = Guid.Parse("11111111-1111-1111-1111-111111111111"),
                IsActive = true
            },
            new Product
            {
                Id = Guid.Parse("c3c3c3c3-c3c3-c3c3-c3c3-c3c3c3c3c3c3"),
                Name = "Cortado",
                Price = 123000,
                Description = "Equal parts espresso and warm milk",
                CategoryId = Guid.Parse("11111111-1111-1111-1111-111111111111"),
                IsActive = true
            },
            new Product
            {
                Id = Guid.Parse("d4d4d4d4-d4d4-d4d4-d4d4-d4d4d4d4d4d4"),
                Name = "Gibraltar",
                Price = 123000,
                Description = "Double espresso with steamed milk in small glass",
                CategoryId = Guid.Parse("11111111-1111-1111-1111-111111111111"),
                IsActive = true
            },
            new Product
            {
                Id = Guid.Parse("e5e5e5e5-e5e5-e5e5-e5e5-e5e5e5e5e5e5"),
                Name = "Breve",
                Price = 123000,
                Description = "Espresso with steamed half-and-half",
                CategoryId = Guid.Parse("11111111-1111-1111-1111-111111111111"),
                IsActive = true
            },
            new Product
            {
                Id = Guid.Parse("f6f6f6f6-f6f6-f6f6-f6f6-f6f6f6f6f6f6"),
                Name = "Turkish Coffee",
                Price = 123000,
                Description = "Traditional finely ground coffee brewed in pot",
                CategoryId = Guid.Parse("11111111-1111-1111-1111-111111111111"),
                IsActive = true
            },
            new Product
            {
                Id = Guid.Parse("a7a7a7a7-a7a7-a7a7-a7a7-a7a7a7a7a7a7"),
                Name = "French Press",
                Price = 123000,
                Description = "Coarse ground coffee steeped in hot water",
                CategoryId = Guid.Parse("11111111-1111-1111-1111-111111111111"),
                IsActive = true
            },
            new Product
            {
                Id = Guid.Parse("b8b8b8b8-b8b8-b8b8-b8b8-b8b8b8b8b8b8"),
                Name = "Pour Over",
                Price = 123000,
                Description = "Hand-poured coffee with filter method",
                CategoryId = Guid.Parse("11111111-1111-1111-1111-111111111111"),
                IsActive = true
            },
            new Product
            {
                Id = Guid.Parse("c9c9c9c9-c9c9-c9c9-c9c9-c9c9c9c9c9c9"),
                Name = "Caramel Latte",
                Price = 123000,
                Description = "Latte with caramel syrup and whipped cream",
                CategoryId = Guid.Parse("11111111-1111-1111-1111-111111111111"),
                IsActive = true
            },
            new Product
            {
                Id = Guid.Parse("d0d0d0d0-d0d0-d0d0-d0d0-d0d0d0d0d0d0"),
                Name = "Vanilla Latte",
                Price = 123000,
                Description = "Latte with vanilla syrup and steamed milk",
                CategoryId = Guid.Parse("11111111-1111-1111-1111-111111111111"),
                IsActive = true
            },
            new Product
            {
                Id = Guid.Parse("e1e1e1e1-e1e1-e1e1-e1e1-e1e1e1e1e1e1"),
                Name = "Hazelnut Cappuccino",
                Price = 123000,
                Description = "Cappuccino with hazelnut syrup",
                CategoryId = Guid.Parse("11111111-1111-1111-1111-111111111111"),
                IsActive = true
            },
            new Product
            {
                Id = Guid.Parse("f2f2f2f2-f2f2-f2f2-f2f2-f2f2f2f2f2f2"),
                Name = "Cinnamon Dolce Latte",
                Price = 123000,
                Description = "Latte with cinnamon dolce syrup and whipped cream",
                CategoryId = Guid.Parse("11111111-1111-1111-1111-111111111111"),
                IsActive = true
            },
            new Product
            {
                Id = Guid.Parse("a3a3a3a3-a3a3-a3a3-a3a3-a3a3a3a3a3a3"),
                Name = "Pumpkin Spice Latte",
                Price = 123000,
                Description = "Seasonal latte with pumpkin and spice flavors",
                CategoryId = Guid.Parse("11111111-1111-1111-1111-111111111111"),
                IsActive = true
            },
            new Product
            {
                Id = Guid.Parse("b4b4b4b4-b4b4-b4b4-b4b4-b4b4b4b4b4b4"),
                Name = "White Chocolate Mocha",
                Price = 123000,
                Description = "Mocha with white chocolate instead of dark",
                CategoryId = Guid.Parse("11111111-1111-1111-1111-111111111111"),
                IsActive = true
            },

            // Cold Coffee Products (15 items) - Fixed invalid GUIDs
            new Product
            {
                Id = Guid.Parse("10101010-1010-1010-1010-101010101010"),
                Name = "Iced Americano",
                Price = 123000,
                Description = "Espresso with cold water over ice",
                CategoryId = Guid.Parse("22222222-2222-2222-2222-222222222222"),
                IsActive = true
            },
            new Product
            {
                Id = Guid.Parse("20202020-2020-2020-2020-202020202020"),
                Name = "Iced Latte",
                Price = 123000,
                Description = "Espresso with cold milk over ice",
                CategoryId = Guid.Parse("22222222-2222-2222-2222-222222222222"),
                IsActive = true
            },
            new Product
            {
                Id = Guid.Parse("30303030-3030-3030-3030-303030303030"),
                Name = "Cold Brew",
                Price = 123000,
                Description = "Smooth cold-brewed coffee concentrate",
                CategoryId = Guid.Parse("22222222-2222-2222-2222-222222222222"),
                IsActive = true
            },
            new Product
            {
                Id = Guid.Parse("40404040-4040-4040-4040-404040404040"),
                Name = "Iced Cappuccino",
                Price = 123000,
                Description = "Cold version of cappuccino with ice",
                CategoryId = Guid.Parse("22222222-2222-2222-2222-222222222222"),
                IsActive = true
            },
            new Product
            {
                Id = Guid.Parse("50505050-5050-5050-5050-505050505050"),
                Name = "Iced Mocha",
                Price = 123000,
                Description = "Cold chocolate coffee drink with whipped cream",
                CategoryId = Guid.Parse("22222222-2222-2222-2222-222222222222"),
                IsActive = true
            },
            new Product
            {
                Id = Guid.Parse("c5c5c5c5-c5c5-c5c5-c5c5-c5c5c5c5c5c5"),
                Name = "Iced Caramel Macchiato",
                Price = 123000,
                Description = "Iced espresso with caramel and milk",
                CategoryId = Guid.Parse("22222222-2222-2222-2222-222222222222"),
                IsActive = true
            },
            new Product
            {
                Id = Guid.Parse("d6d6d6d6-d6d6-d6d6-d6d6-d6d6d6d6d6d6"),
                Name = "Nitro Cold Brew",
                Price = 123000,
                Description = "Cold brew coffee infused with nitrogen",
                CategoryId = Guid.Parse("22222222-2222-2222-2222-222222222222"),
                IsActive = true
            },
            new Product
            {
                Id = Guid.Parse("e7e7e7e7-e7e7-e7e7-e7e7-e7e7e7e7e7e7"),
                Name = "Iced Vanilla Latte",
                Price = 123000,
                Description = "Iced latte with vanilla syrup",
                CategoryId = Guid.Parse("22222222-2222-2222-2222-222222222222"),
                IsActive = true
            },
            new Product
            {
                Id = Guid.Parse("f8f8f8f8-f8f8-f8f8-f8f8-f8f8f8f8f8f8"),
                Name = "Frappuccino",
                Price = 123000,
                Description = "Blended coffee drink with ice and whipped cream",
                CategoryId = Guid.Parse("22222222-2222-2222-2222-222222222222"),
                IsActive = true
            },
            new Product
            {
                Id = Guid.Parse("a9a9a9a9-a9a9-a9a9-a9a9-a9a9a9a9a9a9"),
                Name = "Affogato",
                Price = 123000,
                Description = "Vanilla ice cream drowned in hot espresso",
                CategoryId = Guid.Parse("22222222-2222-2222-2222-222222222222"),
                IsActive = true
            },
            new Product
            {
                Id = Guid.Parse("b0b0b0b0-b0b0-b0b0-b0b0-b0b0b0b0b0b0"),
                Name = "Iced Cortado",
                Price = 123000,
                Description = "Equal parts espresso and cold milk over ice",
                CategoryId = Guid.Parse("22222222-2222-2222-2222-222222222222"),
                IsActive = true
            },
            new Product
            {
                Id = Guid.Parse("c1c1c1c1-c1c1-c1c1-c1c1-c1c1c1c1c1c1"),
                Name = "Cold Brew Float",
                Price = 123000,
                Description = "Cold brew with vanilla ice cream float",
                CategoryId = Guid.Parse("22222222-2222-2222-2222-222222222222"),
                IsActive = true
            },
            new Product
            {
                Id = Guid.Parse("d2d2d2d2-d2d2-d2d2-d2d2-d2d2d2d2d2d2"),
                Name = "Iced Flat White",
                Price = 123000,
                Description = "Double espresso with cold milk, no foam",
                CategoryId = Guid.Parse("22222222-2222-2222-2222-222222222222"),
                IsActive = true
            },
            new Product
            {
                Id = Guid.Parse("e3e3e3e3-e3e3-e3e3-e3e3-e3e3e3e3e3e3"),
                Name = "Vietnamese Iced Coffee",
                Price = 123000,
                Description = "Strong coffee with sweetened condensed milk",
                CategoryId = Guid.Parse("22222222-2222-2222-2222-222222222222"),
                IsActive = true
            },
            new Product
            {
                Id = Guid.Parse("f4f4f4f4-f4f4-f4f4-f4f4-f4f4f4f4f4f4"),
                Name = "Iced Pumpkin Spice Latte",
                Price = 123000,
                Description = "Seasonal iced latte with pumpkin spice",
                CategoryId = Guid.Parse("22222222-2222-2222-2222-222222222222"),
                IsActive = true
            },

            // Tea Products (12 items) - Fixed invalid GUIDs
            new Product
            {
                Id = Guid.Parse("60606060-6060-6060-6060-606060606060"),
                Name = "Green Tea",
                Price = 123000,
                Description = "Classic green tea with antioxidants",
                CategoryId = Guid.Parse("33333333-3333-3333-3333-333333333333"),
                IsActive = true
            },
            new Product
            {
                Id = Guid.Parse("70707070-7070-7070-7070-707070707070"),
                Name = "Earl Grey",
                Price = 123000,
                Description = "Black tea with bergamot oil",
                CategoryId = Guid.Parse("33333333-3333-3333-3333-333333333333"),
                IsActive = true
            },
            new Product
            {
                Id = Guid.Parse("80808080-8080-8080-8080-808080808080"),
                Name = "Chamomile Tea",
                Price = 123000,
                Description = "Relaxing herbal tea for evening",
                CategoryId = Guid.Parse("33333333-3333-3333-3333-333333333333"),
                IsActive = true
            },
            new Product
            {
                Id = Guid.Parse("90909090-9090-9090-9090-909090909090"),
                Name = "Jasmine Tea",
                Price = 123000,
                Description = "Fragrant green tea with jasmine flowers",
                CategoryId = Guid.Parse("33333333-3333-3333-3333-333333333333"),
                IsActive = true
            },
            new Product
            {
                Id = Guid.Parse("a0a0a0a0-a0a0-a0a0-a0a0-a0a0a0a0a0a0"),
                Name = "Oolong Tea",
                Price = 123000,
                Description = "Traditional Chinese partially fermented tea",
                CategoryId = Guid.Parse("33333333-3333-3333-3333-333333333333"),
                IsActive = true
            },
            new Product
            {
                Id = Guid.Parse("b1b1b1b1-b1b1-b1b1-b1b1-b1b1b1b1b1b1"),
                Name = "Peppermint Tea",
                Price = 123000,
                Description = "Refreshing herbal tea with mint leaves",
                CategoryId = Guid.Parse("33333333-3333-3333-3333-333333333333"),
                IsActive = true
            },
            new Product
            {
                Id = Guid.Parse("c2c2c2c2-c2c2-c2c2-c2c2-c2c2c2c2c2c2"),
                Name = "Chai Latte",
                Price = 123000,
                Description = "Spiced tea with steamed milk",
                CategoryId = Guid.Parse("33333333-3333-3333-3333-333333333333"),
                IsActive = true
            },
            new Product
            {
                Id = Guid.Parse("d3d3d3d3-d3d3-d3d3-d3d3-d3d3d3d3d3d3"),
                Name = "Matcha Latte",
                Price = 123000,
                Description = "Japanese green tea powder with steamed milk",
                CategoryId = Guid.Parse("33333333-3333-3333-3333-333333333333"),
                IsActive = true
            },
            new Product
            {
                Id = Guid.Parse("e4e4e4e4-e4e4-e4e4-e4e4-e4e4e4e4e4e4"),
                Name = "English Breakfast",
                Price = 123000,
                Description = "Classic strong black tea blend",
                CategoryId = Guid.Parse("33333333-3333-3333-3333-333333333333"),
                IsActive = true
            },
            new Product
            {
                Id = Guid.Parse("f5f5f5f5-f5f5-f5f5-f5f5-f5f5f5f5f5f5"),
                Name = "White Tea",
                Price = 123000,
                Description = "Delicate tea with subtle flavor",
                CategoryId = Guid.Parse("33333333-3333-3333-3333-333333333333"),
                IsActive = true
            },
            new Product
            {
                Id = Guid.Parse("a6a6a6a6-a6a6-a6a6-a6a6-a6a6a6a6a6a6"),
                Name = "Rooibos Tea",
                Price = 123000,
                Description = "Caffeine-free South African red bush tea",
                CategoryId = Guid.Parse("33333333-3333-3333-3333-333333333333"),
                IsActive = true
            },
            new Product
            {
                Id = Guid.Parse("b7b7b7b7-b7b7-b7b7-b7b7-b7b7b7b7b7b7"),
                Name = "Iced Green Tea",
                Price = 123000,
                Description = "Refreshing cold green tea",
                CategoryId = Guid.Parse("33333333-3333-3333-3333-333333333333"),
                IsActive = true
            },

            // Pastry Products (15 items) - Fixed invalid GUIDs
            new Product
            {
                Id = Guid.Parse("c8c8c8c8-c8c8-c8c8-c8c8-c8c8c8c8c8c8"),
                Name = "Croissant",
                Price = 123000,
                Description = "Buttery, flaky French pastry",
                CategoryId = Guid.Parse("44444444-4444-4444-4444-444444444444"),
                IsActive = true
            },
            new Product
            {
                Id = Guid.Parse("d9d9d9d9-d9d9-d9d9-d9d9-d9d9d9d9d9d9"),
                Name = "Pain au Chocolat",
                Price = 123000,
                Description = "Croissant with dark chocolate filling",
                CategoryId = Guid.Parse("44444444-4444-4444-4444-444444444444"),
                IsActive = true
            },
            new Product
            {
                Id = Guid.Parse("eaeaeaea-eaea-eaea-eaea-eaeaeaeaeaea"),
                Name = "Blueberry Muffin",
                Price = 123000,
                Description = "Fresh baked muffin with blueberries",
                CategoryId = Guid.Parse("44444444-4444-4444-4444-444444444444"),
                IsActive = true
            },
            new Product
            {
                Id = Guid.Parse("a5a5a5a5-a5a5-a5a5-a5a5-a5a5a5a5a5a5"),
                Name = "Chocolate Chip Muffin",
                Price = 123000,
                Description = "Muffin loaded with chocolate chips",
                CategoryId = Guid.Parse("44444444-4444-4444-4444-444444444444"),
                IsActive = true
            },
            new Product
            {
                Id = Guid.Parse("b6b6b6b6-b6b6-b6b6-b6b6-b6b6b6b6b6b6"),
                Name = "Danish Pastry",
                Price = 123000,
                Description = "Sweet pastry with fruit or cheese filling",
                CategoryId = Guid.Parse("44444444-4444-4444-4444-444444444444"),
                IsActive = true
            },
            new Product
            {
                Id = Guid.Parse("c7c7c7c7-c7c7-c7c7-c7c7-c7c7c7c7c7c7"),
                Name = "Scone",
                Price = 123000,
                Description = "Traditional British baked good",
                CategoryId = Guid.Parse("44444444-4444-4444-4444-444444444444"),
                IsActive = true
            },
            new Product
            {
                Id = Guid.Parse("d8d8d8d8-d8d8-d8d8-d8d8-d8d8d8d8d8d8"),
                Name = "Bagel",
                Price = 123000,
                Description = "Traditional ring-shaped bread",
                CategoryId = Guid.Parse("44444444-4444-4444-4444-444444444444"),
                IsActive = true
            },
            new Product
            {
                Id = Guid.Parse("e9e9e9e9-e9e9-e9e9-e9e9-e9e9e9e9e9e9"),
                Name = "Cinnamon Roll",
                Price = 123000,
                Description = "Sweet roll with cinnamon and icing",
                CategoryId = Guid.Parse("44444444-4444-4444-4444-444444444444"),
                IsActive = true
            },
            new Product
            {
                Id = Guid.Parse("f0f0f0f0-f0f0-f0f0-f0f0-f0f0f0f0f0f0"),
                Name = "Donut",
                Price = 123000,
                Description = "Classic glazed or filled donut",
                CategoryId = Guid.Parse("44444444-4444-4444-4444-444444444444"),
                IsActive = true
            },
            new Product
            {
                Id = Guid.Parse("a1b1a1b1-a1b1-a1b1-a1b1-a1b1a1b1a1b1"),
                Name = "Biscotti",
                Price = 123000,
                Description = "Italian twice-baked cookies",
                CategoryId = Guid.Parse("44444444-4444-4444-4444-444444444444"),
                IsActive = true
            },
            new Product
            {
                Id = Guid.Parse("b2c2b2c2-b2c2-b2c2-b2c2-b2c2b2c2b2c2"),
                Name = "Banana Bread",
                Price = 123000,
                Description = "Moist bread made with ripe bananas",
                CategoryId = Guid.Parse("44444444-4444-4444-4444-444444444444"),
                IsActive = true
            },
            new Product
            {
                Id = Guid.Parse("c3d3c3d3-c3d3-c3d3-c3d3-c3d3c3d3c3d3"),
                Name = "Lemon Loaf",
                Price = 123000,
                Description = "Sweet lemon flavored cake",
                CategoryId = Guid.Parse("44444444-4444-4444-4444-444444444444"),
                IsActive = true
            },
            new Product
            {
                Id = Guid.Parse("d4e4d4e4-d4e4-d4e4-d4e4-d4e4d4e4d4e4"),
                Name = "Quiche",
                Price = 123000,
                Description = "Savory egg custard tart",
                CategoryId = Guid.Parse("44444444-4444-4444-4444-444444444444"),
                IsActive = true
            },
            new Product
            {
                Id = Guid.Parse("e5f5e5f5-e5f5-e5f5-e5f5-e5f5e5f5e5f5"),
                Name = "Pretzel",
                Price = 123000,
                Description = "Twisted bread with coarse salt",
                CategoryId = Guid.Parse("44444444-4444-4444-4444-444444444444"),
                IsActive = true
            },
            new Product
            {
                Id = Guid.Parse("f6a6f6a6-f6a6-f6a6-f6a6-f6a6f6a6f6a6"),
                Name = "Coffee Cake",
                Price = 123000,
                Description = "Sweet cake perfect with coffee",
                CategoryId = Guid.Parse("44444444-4444-4444-4444-444444444444"),
                IsActive = true
            },

            // Smoothies & Juices (10 items)
            new Product
            {
                Id = Guid.Parse("11223344-1122-3344-5566-778899aabbcc"),
                Name = "Strawberry Banana Smoothie",
                Price = 123000,
                Description = "Fresh strawberries and bananas blended",
                CategoryId = Guid.Parse("55555555-5555-5555-5555-555555555555"),
                IsActive = true
            },
            new Product
            {
                Id = Guid.Parse("22334455-2233-4455-6677-8899aabbccdd"),
                Name = "Mango Passion Smoothie",
                Price = 123000,
                Description = "Tropical mango and passion fruit",
                CategoryId = Guid.Parse("55555555-5555-5555-5555-555555555555"),
                IsActive = true
            },
            new Product
            {
                Id = Guid.Parse("33445566-3344-5566-7788-99aabbccddee"),
                Name = "Green Smoothie",
                Price = 123000,
                Description = "Spinach, kale, apple, and banana",
                CategoryId = Guid.Parse("55555555-5555-5555-5555-555555555555"),
                IsActive = true
            },
            new Product
            {
                Id = Guid.Parse("44556677-4455-6677-8899-aabbccddeeff"),
                Name = "Berry Blast Smoothie",
                Price = 123000,
                Description = "Mixed berries with yogurt",
                CategoryId = Guid.Parse("55555555-5555-5555-5555-555555555555"),
                IsActive = true
            },
            new Product
            {
                Id = Guid.Parse("55667788-5566-7788-99aa-bbccddeeff00"),
                Name = "Orange Juice",
                Price = 123000,
                Description = "Fresh squeezed orange juice",
                CategoryId = Guid.Parse("55555555-5555-5555-5555-555555555555"),
                IsActive = true
            },
            new Product
            {
                Id = Guid.Parse("66778899-6677-8899-aabb-ccddeeff0011"),
                Name = "Apple Juice",
                Price = 123000,
                Description = "Pure apple juice from fresh apples",
                CategoryId = Guid.Parse("55555555-5555-5555-5555-555555555555"),
                IsActive = true
            },
            new Product
            {
                Id = Guid.Parse("778899aa-7788-99aa-bbcc-ddeeff001122"),
                Name = "Carrot Ginger Juice",
                Price = 123000,
                Description = "Fresh carrot juice with ginger",
                CategoryId = Guid.Parse("55555555-5555-5555-5555-555555555555"),
                IsActive = true
            },
            new Product
            {
                Id = Guid.Parse("8899aabb-8899-aabb-ccdd-eeff00112233"),
                Name = "Pineapple Juice",
                Price = 123000,
                Description = "Sweet tropical pineapple juice",
                CategoryId = Guid.Parse("55555555-5555-5555-5555-555555555555"),
                IsActive = true
            },
            new Product
            {
                Id = Guid.Parse("99aabbcc-99aa-bbcc-ddee-ff0011223344"),
                Name = "Acai Bowl Smoothie",
                Price = 123000,
                Description = "Acai berries with granola toppings",
                CategoryId = Guid.Parse("55555555-5555-5555-5555-555555555555"),
                IsActive = true
            },
            new Product
            {
                Id = Guid.Parse("aabbccdd-aabb-ccdd-eeff-001122334455"),
                Name = "Protein Smoothie",
                Price = 123000,
                Description = "Protein powder with banana and peanut butter",
                CategoryId = Guid.Parse("55555555-5555-5555-5555-555555555555"),
                IsActive = true
            },

            // Sandwiches (12 items)
            new Product
            {
                Id = Guid.Parse("bbccddee-bbcc-ddee-ff00-112233445566"),
                Name = "BLT Sandwich",
                Price = 123000,
                Description = "Bacon, lettuce, and tomato on toasted bread",
                CategoryId = Guid.Parse("66666666-6666-6666-6666-666666666666"),
                IsActive = true
            },
            new Product
            {
                Id = Guid.Parse("ccddeeff-ccdd-eeff-0011-223344556677"),
                Name = "Club Sandwich",
                Price = 123000,
                Description = "Turkey, bacon, lettuce, and tomato triple decker",
                CategoryId = Guid.Parse("66666666-6666-6666-6666-666666666666"),
                IsActive = true
            },
            new Product
            {
                Id = Guid.Parse("ddeeff00-ddee-ff00-1122-334455667788"),
                Name = "Grilled Cheese",
                Price = 123000,
                Description = "Classic grilled cheese on sourdough",
                CategoryId = Guid.Parse("66666666-6666-6666-6666-666666666666"),
                IsActive = true
            },
            new Product
            {
                Id = Guid.Parse("eeff0011-eeff-0011-2233-445566778899"),
                Name = "Turkey Avocado Wrap",
                Price = 123000,
                Description = "Turkey, avocado, and veggies in tortilla",
                CategoryId = Guid.Parse("66666666-6666-6666-6666-666666666666"),
                IsActive = true
            },
            new Product
            {
                Id = Guid.Parse("ff001122-ff00-1122-3344-55667788aabb"),
                Name = "Veggie Sandwich",
                Price = 123000,
                Description = "Fresh vegetables with hummus spread",
                CategoryId = Guid.Parse("66666666-6666-6666-6666-666666666666"),
                IsActive = true
            },
            new Product
            {
                Id = Guid.Parse("00112233-0011-2233-4455-667788aabbcc"),
                Name = "Ham and Swiss",
                Price = 123000,
                Description = "Sliced ham with Swiss cheese",
                CategoryId = Guid.Parse("66666666-6666-6666-6666-666666666666"),
                IsActive = true
            },
            new Product
            {
                Id = Guid.Parse("11223344-1122-3344-5566-778899ccddee"),
                Name = "Chicken Caesar Wrap",
                Price = 123000,
                Description = "Grilled chicken with caesar dressing",
                CategoryId = Guid.Parse("66666666-6666-6666-6666-666666666666"),
                IsActive = true
            },
            new Product
            {
                Id = Guid.Parse("22334455-2233-4455-6677-8899aaddbbcc"),
                Name = "Tuna Salad Sandwich",
                Price = 123000,
                Description = "Fresh tuna salad on whole grain bread",
                CategoryId = Guid.Parse("66666666-6666-6666-6666-666666666666"),
                IsActive = true
            },
            new Product
            {
                Id = Guid.Parse("33445566-3344-5566-7788-99aabbddeeff"),
                Name = "Panini",
                Price = 123000,
                Description = "Pressed Italian sandwich with various fillings",
                CategoryId = Guid.Parse("66666666-6666-6666-6666-666666666666"),
                IsActive = true
            },
            new Product
            {
                Id = Guid.Parse("44556677-4455-6677-8899-aabbccee0011"),
                Name = "Reuben Sandwich",
                Price = 123000,
                Description = "Corned beef, sauerkraut, Swiss on rye",
                CategoryId = Guid.Parse("66666666-6666-6666-6666-666666666666"),
                IsActive = true
            },
            new Product
            {
                Id = Guid.Parse("55667788-5566-7788-99aa-bbccddff1122"),
                Name = "Caprese Sandwich",
                Price = 123000,
                Description = "Fresh mozzarella, tomato, and basil",
                CategoryId = Guid.Parse("66666666-6666-6666-6666-666666666666"),
                IsActive = true
            },
            new Product
            {
                Id = Guid.Parse("66778899-6677-8899-aabb-ccddeeff2233"),
                Name = "Philly Cheesesteak",
                Price = 123000,
                Description = "Sliced steak with peppers and cheese",
                CategoryId = Guid.Parse("66666666-6666-6666-6666-666666666666"),
                IsActive = true
            },

            // Desserts (10 items)
            new Product
            {
                Id = Guid.Parse("778899aa-7788-99aa-bbcc-ddeeff334455"),
                Name = "Chocolate Brownie",
                Price = 123000,
                Description = "Rich, fudgy chocolate brownie",
                CategoryId = Guid.Parse("77777777-7777-7777-7777-777777777777"),
                IsActive = true
            },
            new Product
            {
                Id = Guid.Parse("8899aabb-8899-aabb-ccdd-eeff00445566"),
                Name = "Cheesecake Slice",
                Price = 123000,
                Description = "Creamy New York style cheesecake",
                CategoryId = Guid.Parse("77777777-7777-7777-7777-777777777777"),
                IsActive = true
            },
            new Product
            {
                Id = Guid.Parse("99aabbcc-99aa-bbcc-ddee-ff0011556677"),
                Name = "Tiramisu",
                Price = 123000,
                Description = "Italian coffee-flavored dessert",
                CategoryId = Guid.Parse("77777777-7777-7777-7777-777777777777"),
                IsActive = true
            },
            new Product
            {
                Id = Guid.Parse("aabbccdd-aabb-ccdd-eeff-001122667788"),
                Name = "Chocolate Chip Cookie",
                Price = 123000,
                Description = "Classic homemade chocolate chip cookie",
                CategoryId = Guid.Parse("77777777-7777-7777-7777-777777777777"),
                IsActive = true
            },
            new Product
            {
                Id = Guid.Parse("bbccddee-bbcc-ddee-ff00-112233778899"),
                Name = "Macarons",
                Price = 123000,
                Description = "Delicate French sandwich cookies",
                CategoryId = Guid.Parse("77777777-7777-7777-7777-777777777777"),
                IsActive = true
            },
            new Product
            {
                Id = Guid.Parse("ccddeeff-ccdd-eeff-0011-2233448899aa"),
                Name = "Ice Cream Sundae",
                Price = 123000,
                Description = "Vanilla ice cream with toppings",
                CategoryId = Guid.Parse("77777777-7777-7777-7777-777777777777"),
                IsActive = true
            },
            new Product
            {
                Id = Guid.Parse("ddeeff00-ddee-ff00-1122-334455aabbcc"),
                Name = "Apple Pie Slice",
                Price = 123000,
                Description = "Traditional apple pie with crust",
                CategoryId = Guid.Parse("77777777-7777-7777-7777-777777777777"),
                IsActive = true
            },
            new Product
            {
                Id = Guid.Parse("eeff0011-eeff-0011-2233-445566bbccdd"),
                Name = "Chocolate Mousse",
                Price = 123000,
                Description = "Light and airy chocolate dessert",
                CategoryId = Guid.Parse("77777777-7777-7777-7777-777777777777"),
                IsActive = true
            },
            new Product
            {
                Id = Guid.Parse("ff001122-ff00-1122-3344-556677ccddee"),
                Name = "Crème Brûlée",
                Price = 123000,
                Description = "Vanilla custard with caramelized sugar",
                CategoryId = Guid.Parse("77777777-7777-7777-7777-777777777777"),
                IsActive = true
            },
            new Product
            {
                Id = Guid.Parse("00112233-0011-2233-4455-667788ddeeff"),
                Name = "Gelato",
                Price = 123000,
                Description = "Italian-style dense ice cream",
                CategoryId = Guid.Parse("77777777-7777-7777-7777-777777777777")
            }
        };

        // Seed Menus
        var menus = new List<Menu>
        {
            new Menu
            {
                Id = Guid.Parse("99999999-9999-9999-9999-999999999999"),
                Name = "Morning Menu",
                FromDate = new DateTime(2024, 1, 1),
                ToDate = new DateTime(2024, 12, 31),
                IsActive = true
            },
            new Menu
            {
                Id = Guid.Parse("88888888-8888-8888-8888-888888888888"),
                Name = "Summer Special",
                FromDate = new DateTime(2024, 6, 1),
                ToDate = new DateTime(2024, 8, 31),
                IsActive = true
            },
            new Menu
            {
                Id = Guid.Parse("11111122-3333-4444-5555-666677778888"),
                Name = "All Day Menu",
                FromDate = new DateTime(2024, 1, 1),
                ToDate = new DateTime(2024, 12, 31),
                IsActive = true
            },
            new Menu
            {
                Id = Guid.Parse("22222233-4444-5555-6666-777788889999"),
                Name = "Holiday Special",
                FromDate = new DateTime(2024, 11, 1),
                ToDate = new DateTime(2024, 12, 31),
                IsActive = true
            },
            new Menu
            {
                Id = Guid.Parse("33333344-5555-6666-7777-888899990000"),
                Name = "Lunch Menu",
                FromDate = new DateTime(2024, 1, 1),
                ToDate = new DateTime(2024, 12, 31)
            }
        };

        // Seed ProductInMenu (fixed GUIDs)
        var productInMenus = new List<ProductInMenu>
        {
            // Morning Menu Products (10 items)
            new ProductInMenu
            {
                Id = Guid.Parse("11aa11aa-11aa-11aa-11aa-11aa11aa11aa"),
                ProductId = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"), // Espresso
                MenuId = Guid.Parse("99999999-9999-9999-9999-999999999999"),
                Quantity = 50
            },
            new ProductInMenu
            {
                Id = Guid.Parse("22bb22bb-22bb-22bb-22bb-22bb22bb22bb"),
                ProductId = Guid.Parse("cccccccc-cccc-cccc-cccc-cccccccccccc"), // Americano
                MenuId = Guid.Parse("99999999-9999-9999-9999-999999999999"),
                Quantity = 40
            },
            new ProductInMenu
            {
                Id = Guid.Parse("33cc33cc-33cc-33cc-33cc-33cc33cc33cc"),
                ProductId = Guid.Parse("dddddddd-dddd-dddd-dddd-dddddddddddd"), // Cappuccino
                MenuId = Guid.Parse("99999999-9999-9999-9999-999999999999"),
                Quantity = 35
            },
            new ProductInMenu
            {
                Id = Guid.Parse("44dd44dd-44dd-44dd-44dd-44dd44dd44dd"),
                ProductId = Guid.Parse("eeeeeeee-eeee-eeee-eeee-eeeeeeeeeeee"), // Latte
                MenuId = Guid.Parse("99999999-9999-9999-9999-999999999999"),
                Quantity = 45
            },
            new ProductInMenu
            {
                Id = Guid.Parse("55ee55ee-55ee-55ee-55ee-55ee55ee55ee"),
                ProductId = Guid.Parse("c8c8c8c8-c8c8-c8c8-c8c8-c8c8c8c8c8c8"), // Croissant
                MenuId = Guid.Parse("99999999-9999-9999-9999-999999999999"),
                Quantity = 25
            },
            new ProductInMenu
            {
                Id = Guid.Parse("66ff66ff-66ff-66ff-66ff-66ff66ff66ff"),
                ProductId = Guid.Parse("eaeaeaea-eaea-eaea-eaea-eaeaeaeaeaea"), // Blueberry Muffin
                MenuId = Guid.Parse("99999999-9999-9999-9999-999999999999"),
                Quantity = 20
            },
            new ProductInMenu
            {
                Id = Guid.Parse("77aa77aa-77aa-77aa-77aa-77aa77aa77aa"),
                ProductId = Guid.Parse("d8d8d8d8-d8d8-d8d8-d8d8-d8d8d8d8d8d8"), // Bagel
                MenuId = Guid.Parse("99999999-9999-9999-9999-999999999999"),
                Quantity = 30
            },
            new ProductInMenu
            {
                Id = Guid.Parse("88bb88bb-88bb-88bb-88bb-88bb88bb88bb"),
                ProductId = Guid.Parse("e4e4e4e4-e4e4-e4e4-e4e4-e4e4e4e4e4e4"), // English Breakfast Tea
                MenuId = Guid.Parse("99999999-9999-9999-9999-999999999999"),
                Quantity = 15
            },
            new ProductInMenu
            {
                Id = Guid.Parse("99cc99cc-99cc-99cc-99cc-99cc99cc99cc"),
                ProductId = Guid.Parse("55667788-5566-7788-99aa-bbccddeeff00"), // Orange Juice
                MenuId = Guid.Parse("99999999-9999-9999-9999-999999999999"),
                Quantity = 20
            },
            new ProductInMenu
            {
                Id = Guid.Parse("aaddaadd-aadd-aadd-aadd-aaddaaddaadd"),
                ProductId = Guid.Parse("e9e9e9e9-e9e9-e9e9-e9e9-e9e9e9e9e9e9"), // Cinnamon Roll
                MenuId = Guid.Parse("99999999-9999-9999-9999-999999999999"),
                Quantity = 18
            },

            // Summer Special items - Fixed ProductIds to match new GUIDs
            new ProductInMenu
            {
                Id = Guid.Parse("bbeebbee-bbee-bbee-bbee-bbeebbeebbee"),
                ProductId = Guid.Parse("10101010-1010-1010-1010-101010101010"), // Iced Americano
                MenuId = Guid.Parse("88888888-8888-8888-8888-888888888888"),
                Quantity = 50
            },
            new ProductInMenu
            {
                Id = Guid.Parse("ccffccff-ccff-ccff-ccff-ccffccffccff"),
                ProductId = Guid.Parse("20202020-2020-2020-2020-202020202020"), // Iced Latte
                MenuId = Guid.Parse("88888888-8888-8888-8888-888888888888"),
                Quantity = 45
            },
            new ProductInMenu
            {
                Id = Guid.Parse("dd00dd00-dd00-dd00-dd00-dd00dd00dd00"),
                ProductId = Guid.Parse("30303030-3030-3030-3030-303030303030"), // Cold Brew
                MenuId = Guid.Parse("88888888-8888-8888-8888-888888888888"),
                Quantity = 40
            },

            // All Day Menu items
            new ProductInMenu
            {
                Id = Guid.Parse("ee11ee11-ee11-ee11-ee11-ee11ee11ee11"),
                ProductId = Guid.Parse("bbccddee-bbcc-ddee-ff00-112233445566"), // BLT Sandwich
                MenuId = Guid.Parse("11111122-3333-4444-5555-666677778888"),
                Quantity = 20
            },
            new ProductInMenu
            {
                Id = Guid.Parse("ff22ff22-ff22-ff22-ff22-ff22ff22ff22"),
                ProductId = Guid.Parse("ccddeeff-ccdd-eeff-0011-223344556677"), // Club Sandwich
                MenuId = Guid.Parse("11111122-3333-4444-5555-666677778888"),
                Quantity = 18
            },

            // Holiday Special items
            new ProductInMenu
            {
                Id = Guid.Parse("aa33aa33-aa33-aa33-aa33-aa33aa33aa33"),
                ProductId = Guid.Parse("a3a3a3a3-a3a3-a3a3-a3a3-a3a3a3a3a3a3"), // Pumpkin Spice Latte
                MenuId = Guid.Parse("22222233-4444-5555-6666-777788889999"),
                Quantity = 40
            },

            // Lunch Menu items
            new ProductInMenu
            {
                Id = Guid.Parse("bb44bb44-bb44-bb44-bb44-bb44bb44bb44"),
                ProductId = Guid.Parse("eeff0011-eeff-0011-2233-445566778899"), // Turkey Avocado Wrap
                MenuId = Guid.Parse("33333344-5555-6666-7777-888899990000"),
                Quantity = 25
            },
            new ProductInMenu
            {
                Id = Guid.Parse("cc55cc55-cc55-cc55-cc55-cc55cc55cc55"),
                ProductId = Guid.Parse("22334455-2233-4455-6677-8899aaddbbcc"), // Tuna Salad Sandwich
                MenuId = Guid.Parse("33333344-5555-6666-7777-888899990000"),
                Quantity = 20
            },
            new ProductInMenu
            {
                Id = Guid.Parse("dd66dd66-dd66-dd66-dd66-dd66dd66dd66"),
                ProductId = Guid.Parse("33445566-3344-5566-7788-99aabbddeeff"), // Panini
                MenuId = Guid.Parse("33333344-5555-6666-7777-888899990000"),
                Quantity = 18
            },
            new ProductInMenu
            {
                Id = Guid.Parse("ee77ee77-ee77-ee77-ee77-ee77ee77ee77"),
                ProductId = Guid.Parse("d4e4d4e4-d4e4-d4e4-d4e4-d4e4d4e4d4e4"), // Quiche
                MenuId = Guid.Parse("33333344-5555-6666-7777-888899990000"),
                Quantity = 12
            }
        };

        await _context.Categories.AddRangeAsync(categories);
        await _context.Products.AddRangeAsync(products);
        await _context.Menus.AddRangeAsync(menus);
        await _context.ProductInMenus.AddRangeAsync(productInMenus);

        await _context.SaveChangesAsync();
    }
}
