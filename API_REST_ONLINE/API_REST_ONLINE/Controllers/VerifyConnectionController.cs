//using Microsoft.AspNetCore.Mvc;

//[ApiController]
//[Route("[controller]")]
//public class VerifyConnectionController : ControllerBase
//{
//    private readonly IPlayerSessionManager _playerSessionManager; // Assuming you have a session manager service

//    public VerifyConnectionController(IPlayerSessionManager playerSessionManager)
//    {
//        _playerSessionManager = playerSessionManager;
//    }

//    [HttpGet("connection")]
//    public IActionResult CheckPlayerConnection()
//    {
//        // Get the player's session from the session manager
//        var playerSession = _playerSessionManager.GetPlayerSession();

//        if (playerSession != null && playerSession.IsConnected)
//        {
//            return Ok("Player is connected");
//        }
//        else
//        {
//            return NotFound("Player is not connected");
//        }
//    }
//}

//public class PlayerSession
//{
//    public bool IsConnected { get; set; }
//    // Other session properties...
//}

