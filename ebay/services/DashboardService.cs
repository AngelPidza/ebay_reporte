using ebay.Models;

namespace ebay.services;

public class DashboardService
{
    private readonly HttpClient _httpClient;

    public DashboardService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<DashboardData> GetDashboardDataAsync()
    {
        var products = await _httpClient.GetFromJsonAsync<List<Product>>("api/products") ?? new List<Product>();
        var users = await _httpClient.GetFromJsonAsync<List<User>>("api/users") ?? new List<User>();

        var data = new DashboardData
        {
            TotalProducts = products.Count,
            TotalUsers = users.Count,
            TopProducts = products.OrderByDescending(p => p.Stock).Take(5).ToList(),
            OrdersByCategory = products.GroupBy(p => p.Category)
                .Select(g => new CategoryData { 
                    Category = g.Key, 
                    Count = g.Count()
                }).ToList()
        };

        return data;
    }
}