using System.IdentityModel.Tokens.Jwt;
using ebay.Models;
using System.Net.Http.Headers;

namespace ebay.services
{
    public class AuthService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<AuthService> _logger;

        public AuthService(HttpClient httpClient, ILogger<AuthService> logger)
        {
            _httpClient = httpClient;
            _logger = logger;
        }

        public async Task<(bool Succeeded, User? User, string? ErrorMessage)> LoginAsync(string email, string password)
        {
            var payload = new { Email = email, Password = password };
            var response = await _httpClient.PostAsJsonAsync("api/Auth/login", payload);

            if (response.IsSuccessStatusCode)
            {
                // Leer la respuesta y extraer el token y el usuario
                var responseData = await response.Content.ReadFromJsonAsync<ApiResponse>();

                // Asegúrate de que el token existe
                if (responseData?.Token != null)
                {
                    try
                    {
                        // Decodificar el token JWT
                        var handler = new JwtSecurityTokenHandler();
                        var jsonToken = handler.ReadJwtToken(responseData.Token);

                        // Obtener el UID desde el JWT (usualmente en el campo "uid")
                        var userId = jsonToken.Payload["uid"]?.ToString();
                
                        if (userId != null)
                        {
                            // Crear un objeto User
                            var user = new User
                            {
                                Username = responseData.User.Username,
                                UserId = userId, // Asignamos el UID del token
                                Email = responseData.User.Email
                            };

                            return (true, user, null);
                        }
                        else
                        {
                            return (false, null, "UID not found in token.");
                        }
                    }
                    catch (Exception ex)
                    {
                        return (false, null, $"Error decoding token: {ex.Message}");
                    }
                }
                else
                {
                    return (false, null, "Token not found in response.");
                }
            }
            else
            {
                var error = await response.Content.ReadAsStringAsync();
                return (false, null, error);
            }
        }
        
        public async Task<bool> RegisterAsync(string Username, string Email, string Password)
        {
            var payload = new
            {
                username = Username,
                email = Email,
                password = Password,
                confirmPassword = Password
            };

            var response = await _httpClient.PostAsJsonAsync("api/Auth/register", payload);

            // Considera devolver un resultado más específico si es necesario.
            return response.IsSuccessStatusCode;
        }
    }
}
public class ErrorResponse
{
    public string? Message { get; set; }
}
