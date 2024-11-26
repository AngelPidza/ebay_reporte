using ebay.Models;

namespace ebay.services;

public class ProductService
{
    private readonly HttpClient _httpClient;

    public ProductService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<List<Product>> GetProductsAsync()
    {
        var response = await _httpClient.GetAsync("api/products");
        response.EnsureSuccessStatusCode();

        var products = await response.Content.ReadFromJsonAsync<List<Product>>();
        return products ?? new List<Product>();
    }

    public async Task<Product> GetProductDetailsAsync(int id)
    {
        var response = await _httpClient.GetAsync($"api/products/{id}");
        if (response.IsSuccessStatusCode)
        {
            return await response.Content.ReadFromJsonAsync<Product>() ?? new Product();
        }

        throw new HttpRequestException($"Error retrieving product with id {id}");
    }
}