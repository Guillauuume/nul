using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using StackExchange.Redis;
using API_REST_ONLINE.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using Microsoft.EntityFrameworkCore;


[Route("api/[controller]")]
[ApiController]
public class ServerRegistrationController : ControllerBase
{
    private readonly ConnectionMultiplexer _redis;
    private readonly ApplicationDbContext _context;

    public ServerRegistrationController(ApplicationDbContext context, IConfiguration configuration)
    {
        _context = context;
        string redisConnectionString = configuration.GetConnectionString("RedisConnection");
        _redis = ConnectionMultiplexer.Connect(redisConnectionString);
    }

    [Authorize]
    [HttpPost("register")]
    public IActionResult RegisterServer(ServerRegistrationRequest request)
    {
        IDatabase db = _redis.GetDatabase();
        string idSessionKey = $"server:{request.address}";
        string addressSetKey = "Adress";

        // Check if a server with the same ID already exists
        //if (db.KeyExists(idSessionKey))
        //{
        //    return Conflict("Server with the same ID already exists");
        //}

        // Check if the address already exists
        if (db.KeyExists(idSessionKey))
        {
            return Conflict("Server with the same address already exists");
        }

        // Store session information in Redis
        var sessionData = new HashEntry[]
        {
            //new HashEntry("ServerId", request.serverid),
            new HashEntry("address", request.address),
            new HashEntry("nbplayers", request.nbplayers),
            new HashEntry("ratiomean", request.ratiomean),
            new HashEntry("mapname", request.mapname)
        };

        // Set the hash fields
        db.HashSet(idSessionKey, sessionData);

        // Add the address to the set of addresses
        db.SetAdd(addressSetKey, request.address);

        return Ok("Server registered successfully");
    }


    [Authorize]
    [HttpPost("add-player-to-session")]
    public IActionResult AddPlayerToSession(string serverAddress, int increment)
    {
        IDatabase db = _redis.GetDatabase();

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

        var ratio = user.killdeathratio;

        // Get the session key
        string sessionKey = $"server:{serverAddress}";

        // Log the session key for debugging
        Console.WriteLine($"Session Key: {sessionKey}");

        // Check if the session exists
        if (!db.KeyExists(sessionKey))
        {
            Console.WriteLine($"Session not found for key: {sessionKey}");
            return NotFound("Session not found");
        }

        // Check if the value stored at the key is a hash
        if (db.KeyType(sessionKey) != RedisType.Hash)
        {
            Console.WriteLine($"Session key {sessionKey} does not hold a hash value.");
            return StatusCode(500, "Session key does not hold a hash value.");
        }

        long currentNbPlayers = (long)db.HashGet(sessionKey, "nbplayers");
        double currentRatioMean = (double)db.HashGet(sessionKey, "ratiomean");
        string mapname = (string)db.HashGet(sessionKey, "mapname");


        double sum = currentRatioMean * currentNbPlayers;
        sum += ratio * (increment);

        double newRatioMean = sum / (currentNbPlayers + increment);

        // Increment the number of players
        db.HashIncrement(sessionKey, "nbplayers", increment);

        // Increment the mean of the ratio
        db.HashSet(sessionKey, "ratiomean", newRatioMean);


        // Assuming user is an instance of a class representing a user
        // Assign the retrieved value to the lastmapplayed property of the user object
        user.lastmapplayed = mapname;
        _context.SaveChanges();

        return Ok("Player added to session successfully");
    }


    //[Authorize]
    [HttpPost("test-increase-ratio")]
    public IActionResult TestIncreaseRatio(double newratio)
    {
        IDatabase db = _redis.GetDatabase();

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
            return NotFound("User not found.");
        }

        // Update the killdeathratio with the new value
        user.killdeathratio = newratio;

        try
        {
            // Save the changes to the database
            _context.SaveChanges();
            return Ok(user.killdeathratio);
        }
        catch (Exception ex)
        {
            // Handle any exceptions that occur during database save operation
            return StatusCode(500, $"An error occurred while updating the killdeathratio: {ex.Message}");
        }
    }




    [HttpGet("get-sessions")]
    public IActionResult GetAllSessions()
    {
        IDatabase db = _redis.GetDatabase();

        // Get all keys matching the pattern 'server:*'
        var sessionKeys = db.Multiplexer.GetServer(_redis.GetEndPoints().First()).Keys(pattern: "server:*");

        List<ServerRegistrationRequest> sessions = new List<ServerRegistrationRequest>();
        foreach (var key in sessionKeys)
        {
            // Convert RedisKey to string
            string sessionKey = key.ToString();

            // Get the session data from Redis as a hash
            HashEntry[] sessionHash = db.HashGetAll(sessionKey);

            // Convert the hash data to a dictionary
            var sessionDict = sessionHash.ToDictionary(
                hashEntry => hashEntry.Name.ToString(),
                hashEntry => hashEntry.Value.ToString()
            );

            // Deserialize the dictionary into a ServerRegistrationRequest object
            ServerRegistrationRequest sessionData = JsonConvert.DeserializeObject<ServerRegistrationRequest>(JsonConvert.SerializeObject(sessionDict));

            // Add session data to the list
            if (sessionData != null)
            {
                sessions.Add(sessionData);
            }
            else
            {
                // Log a warning if session data is null
                Console.WriteLine($"Session data is null for key: {sessionKey}");
            }
        }

        return Ok(sessions);
    }


    [Authorize]
    [HttpPost("find-closest-session")]
    public IActionResult FindClosestSession()
    {
        IDatabase db = _redis.GetDatabase();

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

        var ratio = user.killdeathratio;

        // Get all keys matching the pattern 'server:*'
        var sessionKeys = db.Multiplexer.GetServer(_redis.GetEndPoints().First()).Keys(pattern: "server:*");

        // Initialize variables to store the closest session and the difference in ratio
        string closestSessionKey = null;
        double closestDifference = double.MaxValue;
        int nbPlayers = int.MaxValue;

        foreach (var key in sessionKeys)
        {
            // Convert RedisKey to string
            string sessionKey = key.ToString();

            // Get the session data from Redis as a hash
            HashEntry[] sessionHash = db.HashGetAll(sessionKey);

            // Convert hash entries to ServerRegistrationRequest object
            ServerRegistrationRequest sessionData = new ServerRegistrationRequest();

            foreach (var entry in sessionHash)
            {
                string fieldName = entry.Name.ToString().ToLower();
                string fieldValue = entry.Value.ToString();

                // Assign fields based on field name
                switch (fieldName)
                {
                    //case "serverid":
                    //    sessionData.serverid = int.Parse(fieldValue);
                    //    break;
                    case "adress":
                        sessionData.address = fieldValue;
                        break;
                    case "nbplayers":
                        sessionData.nbplayers = int.Parse(fieldValue);
                        break;
                    case "ratiomean":
                        sessionData.ratiomean = float.Parse(fieldValue);
                        break;
                    default:
                        // Handle other fields if necessary
                        break;
                }
            }

            // Calculate the difference in ratio between the player and the session
            double difference = Math.Abs(sessionData.ratiomean - ratio);

            // Check if this session is closer than the previous closest session
            if (difference < closestDifference)
            {
                closestDifference = difference;
                closestSessionKey = sessionKey;
                nbPlayers = sessionData.nbplayers;
            }
            if(difference == closestDifference)
            {
                if(nbPlayers > sessionData.nbplayers)
                {
                    closestSessionKey = sessionKey;
                }
            }
        }

        // Check if a closest session was found
        if (closestSessionKey != null)
        {
            // Get the session data from Redis as a hash
            HashEntry[] closestSessionHash = db.HashGetAll(closestSessionKey);

            // Convert hash entries to ServerRegistrationRequest object
            ServerRegistrationRequest closestSessionData = new ServerRegistrationRequest();

            foreach (var entry in closestSessionHash)
            {
                string fieldName = entry.Name.ToString().ToLower();
                string fieldValue = entry.Value.ToString();

                // Assign fields based on field name
                switch (fieldName)
                {
                    //case "serverid":
                    //    closestSessionData.serverid = int.Parse(fieldValue);
                    //    break;
                    case "adress":
                        closestSessionData.address = fieldValue;
                        break;
                    case "nbplayers":
                        closestSessionData.nbplayers = int.Parse(fieldValue);
                        break;
                    case "ratiomean":
                        closestSessionData.ratiomean = float.Parse(fieldValue);
                        break;
                    default:
                        // Handle other fields if necessary
                        break;
                }
            }

            return Ok(closestSessionData.address);
        }
        else
        {
            return NotFound("No session found.");
        }
    }

}

namespace API_REST_ONLINE.Models
{
    public class ServerRegistrationRequest
    {
        //public int serverid { get; set; }

        [JsonProperty("adress")] // Specify the JSON property name explicitly
        public string address { get; set; }
        public string mapname { get; set; }
        public int nbplayers { get; set; }
        public float ratiomean { get; set; }

    }
}