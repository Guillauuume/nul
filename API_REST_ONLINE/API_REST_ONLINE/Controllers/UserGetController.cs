using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json.Linq;
using System.IdentityModel.Tokens.Jwt;
using System.Text;
using Microsoft.Extensions.Configuration;


[Authorize]
[Route("api/[controller]")]
[ApiController]
public class UserGetController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private IConfiguration _config;

    public UserGetController(ApplicationDbContext context, IConfiguration configuration)
    {
        _context = context;
        _config = configuration;
    }

    [HttpGet("userinfo")]
    public IActionResult GetUserInfo()
    {
        // Retrieve the user's identity from the current request
        var userIdentity = HttpContext.User.Identity as ClaimsIdentity;

        // Retrieve the user's claims
        var claims = userIdentity.Claims.Select(c => new { c.Type, c.Value });

        // You can access specific claims like username, email, etc.
        var username = userIdentity.FindFirst(ClaimTypes.Name)?.Value;
        var email = userIdentity.FindFirst(ClaimTypes.Email)?.Value;
       

        return Ok(new { Username = username, Email = email, Claims = claims });
    }


    [HttpGet("killdeathratio")]
    public async Task<ActionResult<double>> GetKillDeathRatio()
    {
        // Retrieve the user's id from the JWT token
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (string.IsNullOrEmpty(userId))
        {
            return BadRequest("User id not found in token.");
        }

        return Ok(new { UserId = userId });
    }



}
