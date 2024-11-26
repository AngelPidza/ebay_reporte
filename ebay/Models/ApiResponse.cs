namespace ebay.Models;
public class ApiResponse
{
    public string Status { get; set; }
    public string Message { get; set; }
    public string Token { get; set; }
    public User User { get; set; }
}