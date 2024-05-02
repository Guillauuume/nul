// RegistrationController.cs
using API_REST_ONLINE.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Security.Cryptography;
using System.Text;

[Route("[controller]")]
[ApiController]
public class RegistrationController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public RegistrationController(ApplicationDbContext context)
    {
        _context = context;
    }

    [HttpPost("register")]
    public IActionResult Register(RegisterRequest request)
    {
        string salt = SaltGenerator.GenerateSalt(16);
        // Check if username already exists
        if (_context.users.Any(u => u.username == request.username))
        {
            return Conflict("Username already exists");
        }

        // Create user
        var user = new User
        {
            id = Guid.NewGuid(),
            username = request.username,
            email = request.email,
            password = HashingHelper.HashPassword(request.password, salt), // You should hash the password before saving it
            salt = salt, // You should generate a unique salt for each user
            roleid = 1 // Assuming role ID is provided in the request
        };

        _context.users.Add(user);
        _context.SaveChanges();

        return Ok("User registered successfully");
    }
}


public static class SaltGenerator
{
    public static string GenerateSalt(int size)
    {
        byte[] saltBytes = new byte[size];
        using (var rng = new RNGCryptoServiceProvider())
        {
            rng.GetBytes(saltBytes);
        }
        return Convert.ToBase64String(saltBytes);
    }
}


public static class HashingHelper
{
    public static string HashPassword(string password, string salt)
    {
        using (SHA256 sha256 = SHA256.Create())
        {
            // Combine the password and salt
            string combinedString = password + salt;

            // Compute hash value from the combined string
            byte[] hashBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(combinedString));

            // Convert byte array to a hexadecimal string
            StringBuilder builder = new StringBuilder();
            foreach (byte b in hashBytes)
            {
                builder.Append(b.ToString("x2")); // "x2" format specifier ensures each byte is represented by 2 characters
            }
            return builder.ToString();
        }
    }
}

public class RegisterRequest
{
    public string username { get; set; }
    public string email { get; set; }
    public string password { get; set; }
    public int roleid { get; set; }
}
