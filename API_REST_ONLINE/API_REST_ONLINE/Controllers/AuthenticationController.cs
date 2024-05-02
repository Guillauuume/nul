using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;


[Route("[controller]")]
[ApiController]
public class AuthenticationController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly string _jwtSecret;
    private IConfiguration _config;

    public AuthenticationController(ApplicationDbContext context, IConfiguration configuration)
    {
        _context = context;
        _jwtSecret = configuration["Jwt:Key"];
        _config = configuration;
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginRequest request)
    {
        var user = await _context.users.SingleOrDefaultAsync(u => u.username == request.username);

        if (user == null || !VerifyPassword(user.password, request.password, user.salt))
        {
            return Unauthorized();
        }

        // Generate JWT token
        //var token = GenerateJwtToken(user.id.ToString(), user.roleid.ToString());

        //HttpContext.Response.Cookies.Append("jwtToken", token);

        //var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSecret));
        var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]));
        var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

        var claims = new[] {
            new Claim(JwtRegisteredClaimNames.NameId, user.id.ToString()),
            new Claim(JwtRegisteredClaimNames.Name, user.username),
            new Claim(JwtRegisteredClaimNames.Email, user.email),
            new Claim("Role", user.roleid.ToString()),
        };

        var Sectoken = new JwtSecurityToken(_config["Jwt:Issuer"],
          _config["Jwt:Issuer"],
          claims,
          expires: DateTime.Now.AddMinutes(120),
          signingCredentials: credentials);

        var token = new JwtSecurityTokenHandler().WriteToken(Sectoken);

        return Ok(new { token });
    }

    private bool VerifyPassword(string hashedPassword, string password, string salt)
    {
        // Implement your password verification logic here
        // This is just a placeholder
        return hashedPassword == HashPassword(password, salt);
    }

    private static string HashPassword(string password, string salt)
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


    private static byte[] GenerateRandomKey(int keySize)
    {
        byte[] key = new byte[keySize / 8]; // Convert bits to bytes
        using (var rng = new RNGCryptoServiceProvider())
        {
            rng.GetBytes(key);
        }
        return key;
    }


    private string GenerateJwtToken(string userId, string roleId)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = GenerateRandomKey(256); // Generate a 256-bit (32-byte) key
        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(new Claim[]
            {
            new Claim(ClaimTypes.Name, userId),
            new Claim(ClaimTypes.Role, roleId)
            }),
            Expires = DateTime.UtcNow.AddHours(1), // Token expires in 1 hour
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
        };
        var token = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(token);
    }



}

public class LoginRequest
{
    public required string username { get; set; }
    public required string password { get; set; }
}
