using ebay.Models;
using ebay.services;
using jsreport.AspNetCore;
using jsreport.Local;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using jsreport.Binary;
using jsreport.Client;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

builder.Services.AddDbContext<GameWorldContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("Conn")));

// Configurar la autenticación (si la necesitas)
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Account/Login";
        options.LogoutPath = "/Account/Logout";
    });

// Registra el HttpClient global para todos los servicios
builder.Services.AddHttpClient<ProductService>()
    .ConfigureHttpClient(client =>
    {
        client.BaseAddress = new Uri("http://localhost:13226/"); // Cambia por la URL de tu API
    });

builder.Services.AddHttpClient<AuthService>()
    .ConfigureHttpClient(client =>
    {
        client.BaseAddress = new Uri("http://localhost:13226/"); // Cambia por la URL de tu API
    });

// Configurar los controladores
builder.Services.AddControllersWithViews();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
}
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapDefaultControllerRoute();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();