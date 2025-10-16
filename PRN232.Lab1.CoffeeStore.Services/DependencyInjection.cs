using Ganss.Xss;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using PRN232.Lab1.CoffeeStore.Services.Interfaces;
using PRN232.Lab1.CoffeeStore.Services.Interfaces.Services;
using PRN232.Lab1.CoffeeStore.Services.Models;
using PRN232.Lab1.CoffeeStore.Services.Services;
using VNPAY.NET;

namespace PRN232.Lab1.CoffeeStore.Services;

public static class DependencyInjection
{
    public static void AddBusinessServices(this IServiceCollection services, IConfiguration configuration)
    {
        // Configure JWT Settings
        services.Configure<JwtSettings>(configuration.GetSection("JwtSettings"));
        
        services.AddScoped<IVnpay, Vnpay>();
        services.AddScoped<IProductService, ProductService>();
        services.AddScoped<IMenuService, MenuService>();
        services.AddScoped<IOrderService, OrderService>();
        services.AddScoped<IVnPayService, VnPayService>();
        services.AddScoped<ITokenService, TokenService>();
        services.AddScoped<IAuthService, AuthService>();
        // services.AddSingleton<IHtmlSanitizer, HtmlSanitizer>();
        services.AddSingleton<IHtmlSanitizer>(_ =>
        {
            var s = new HtmlSanitizer();
            //
            // s.AllowedTags.Add("p"); s.AllowedTags.Add("br");
            // s.AllowedTags.Add("ul"); s.AllowedTags.Add("ol"); s.AllowedTags.Add("li");
            // s.AllowedTags.Add("strong"); s.AllowedTags.Add("em");
            // s.AllowedAttributes.Add("href");
            // s.AllowedSchemes.Clear();
            // s.AllowedSchemes.Add("http"); s.AllowedSchemes.Add("https"); s.AllowedSchemes.Add("mailto");
            //
            s.KeepChildNodes = true;
        
            return s;
        });

    }
}
