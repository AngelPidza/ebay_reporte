namespace ebay.Models;

public class DashboardData
{
    public int TotalProducts { get; set; }
    public int TotalUsers { get; set; }
    public List<Product> TopProducts { get; set; } = new();
    public List<CategoryData> OrdersByCategory { get; set; } = new();
}

public class CategoryData
{
    public string Category { get; set; } = null!;
    public int Count { get; set; }
}

public class MonthlySale
{
    public string Month { get; set; } = null!;
    public decimal Amount { get; set; }
}