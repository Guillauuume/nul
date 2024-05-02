using API_REST_ONLINE.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;


[Route("[controller]")]
[ApiController]
public class SuccessController : ControllerBase
{
    private readonly ApplicationDbContext _context; // Assuming ApplicationDbContext is your database context
    private readonly string _jwtSecret;

    public SuccessController(ApplicationDbContext context, IConfiguration configuration)
    {
        _context = context;
    }


    [Authorize]
    [HttpGet("game-successes")]
    public IActionResult GetGameSuccesses()
    {
        // Retrieve the user's id from the JWT token
        var userIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (string.IsNullOrEmpty(userIdString))
        {
            return BadRequest("User id not found in token.");
        }

        var userId = Guid.Parse(userIdString);
        var user = _context.users.FirstOrDefault(u => u.id == userId);


        if (user == null)
        {
            return NotFound();
        }

        //var ratio = user.killdeathratio;

        var gameSuccesses = _context.success.ToList(); // Fetch game successes from the database
        return Ok(gameSuccesses);
    }


    [HttpGet("game-successes-debug")]
    public IActionResult GetGameSuccessesDebug()
    {
        var gameSuccesses = _context.success.ToList(); // Fetch game successes from the database
        return Ok(gameSuccesses);
    }

    [HttpGet("rank-debug")]
    public IActionResult GetRankDebug()
    {
        var ranks = _context.rank.ToList(); // Fetch game successes from the database
        return Ok(ranks);
    }


    [HttpPost("grant-achievement")]
    public IActionResult GrantAchievement(GrantAchievementRequest request)
    {
        // Validate request coming from the game server (e.g., check authentication token)
        if (!IsRequestFromGameServer(Request.Headers))
        {
            return Unauthorized();
        }

        // Retrieve the user from the database
        var user = _context.users.FirstOrDefault(u => u.id == request.PlayerId);
        if (user == null)
        {
            return NotFound("User not found");
        }

        // Check if the achievement already exists for the user
        if (user.achievements.Any(a => a.id == request.SuccessId))
        {
            return Conflict("Achievement already granted to the user");
        }

        var achievementDetails = _context.success
            .Where(s => s.id == request.SuccessId)
            .Select(s => new
            {
                AchievementName = s.name,
                AchievementDescription = s.description,
                AchievementImageUrl = s.imageurl
            })
            .FirstOrDefault();

        // Add the achievement to the user's list of achievements
        var achievement = new Success
        {
            id = request.SuccessId,
            name = "Achievement Name", // Provide the name of the achievement
            description = "Achievement Description", // Provide the description of the achievement
            imageurl = "Achievement Image URL", // Provide the URL of the achievement image
            timestamp = DateTime.UtcNow
        };
        user.achievements.Add(achievement);

        // Save the changes to the database
        _context.SaveChanges();

        return Ok("Achievement granted successfully");
    }


    private bool IsRequestFromGameServer(IHeaderDictionary headers)
    {
        // Retrieve the API key from the request headers
        if (headers.TryGetValue("Authorization", out var apiKeyValues))
        {
            var apiKey = apiKeyValues.FirstOrDefault();

            // Validate the API key (this is a placeholder, replace it with your actual validation logic)
            if (!string.IsNullOrWhiteSpace(apiKey) && apiKey == "YOUR_GAME_SERVER_API_KEY")
            {
                return true; // API key is valid, request is from the game server
            }
        }

        return false; // API key is missing or invalid
    }

}

public class GrantAchievementRequest
{
    public Guid PlayerId { get; set; }
    public int SuccessId { get; set; }
}

// Add your ApplicationDbContext and Achievement model here
// Replace with actual models if different
