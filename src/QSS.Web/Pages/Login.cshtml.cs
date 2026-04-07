using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Net.Http.Json;
using System.Security.Claims;
using System.Text.Json;

namespace QSS.Web.Pages;

public class LoginModel : PageModel
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<LoginModel> _logger;

    public string? ErrorMessage { get; set; }
    public string? Email { get; set; }

    public LoginModel(IHttpClientFactory httpClientFactory, ILogger<LoginModel> logger)
    {
        _httpClientFactory = httpClientFactory;
        _logger = logger;
    }

    public IActionResult OnGet()
    {
        if (User.Identity?.IsAuthenticated == true)
            return RedirectToPage("/Dashboard");
        return Page();
    }

    public async Task<IActionResult> OnPostAsync(string email, string password)
    {
        Email = email;
        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
        {
            ErrorMessage = "Please enter email and password.";
            return Page();
        }

        try
        {
            var client = _httpClientFactory.CreateClient("QssApi");
            var response = await client.PostAsJsonAsync("/api/auth/login", new { email, password });

            if (!response.IsSuccessStatusCode)
            {
                ErrorMessage = "Invalid credentials. Please try again.";
                return Page();
            }

            var json = await response.Content.ReadAsStringAsync();
            var doc = JsonDocument.Parse(json);
            var root = doc.RootElement;
            var token = root.GetProperty("token").GetString()!;
            var userEl = root.GetProperty("user");
            var userId = userEl.GetProperty("id").GetString()!;
            var fullName = userEl.GetProperty("fullName").GetString()!;
            var role = userEl.TryGetProperty("role", out var roleEl) ? roleEl.GetString() ?? "" : "";

            // Store JWT in cookie for API calls
            Response.Cookies.Append("qss_token", token, new CookieOptions
            {
                HttpOnly = false, // Readable by JS for API calls
                Expires = DateTimeOffset.UtcNow.AddDays(7),
                SameSite = SameSiteMode.Strict
            });

            // Create cookie auth session
            var claims = new List<Claim>
            {
                new(ClaimTypes.NameIdentifier, userId),
                new(ClaimTypes.Name, fullName),
                new(ClaimTypes.Email, email),
                new(ClaimTypes.Role, role)
            };
            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var principal = new ClaimsPrincipal(identity);
            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);

            return RedirectToPage("/Dashboard");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Login failed");
            ErrorMessage = "Unable to connect to the API server. Please check your connection and try again.";
            return Page();
        }
    }
}
