using API_REST_ONLINE.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace API_REST_ONLINE.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FriendController : ControllerBase
    {

        private readonly ApplicationDbContext _context; // Assuming you have a DbContext instance

        public FriendController(ApplicationDbContext context)
        {
            _context = context;
        }


        [Authorize]
        [HttpPost("send-invite")]
        public IActionResult SendFriendInvitation(Guid receiverid)
        {
            var userIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userIdString))
            {
                return BadRequest("User id not found in token.");
            }

            var senderid = Guid.Parse(userIdString);
            var user = _context.users.FirstOrDefault(u => u.id == senderid);

            if (user == null)
            {
                return NotFound("User not found.");
            }

            if (senderid == null || receiverid == null)
            {
                return BadRequest("Sender or receiver not found.");
            }

            // Check if there's already an invitation between the sender and receiver
            var existingInvitation = _context.pendinginvite.FirstOrDefault(pi => pi.inviterid == senderid && pi.inviteeid == receiverid);

            if (existingInvitation != null)
            {
                return BadRequest("An invitation to this user already exists.");
            }

            // Set invitername here, inside the database transaction scope
            var invitername = user.username;

            // Add the invitation to the database
            var invitation = new PendingInvite
            {
                inviterid = senderid,
                inviteeid = receiverid,
                invitername = invitername,
            };

            _context.pendinginvite.Add(invitation);
            _context.SaveChanges();

            // Return a success response
            return Ok("Friend invitation sent successfully.");
        }




        [Authorize]
        [HttpGet("get-pending-invites")]
        public IActionResult GetPendingInvites()
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

            var pendinginviteid = user.pendinginviteid;

            try
            {
                // Find all pending invites where the player is either the inviter or the invitee
                var invites = _context.pendinginvite
                    .Where(pi => pi.inviteeid == userId)
                    .Select(pi => new PendingInvite
                    {
                        id = pi.id,
                        inviterid = pi.inviterid,
                        inviteeid = pi.inviteeid,
                        invitername = pi.invitername,
                    })
                    .ToList();

                return Ok(invites);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred while retrieving pending invites");
            }
        }


        [Authorize]
        [HttpGet("get-all-users")]
        public IActionResult GetAllUsers()
        {
            var userIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userIdString))
            {
                return BadRequest("User id not found in token.");
            }

            var userId = Guid.Parse(userIdString);

            try
            {
                // Retrieve the current user's friend IDs
                var friendIds = _context.friendship
                    .Where(f => f.userid1 == userId || f.userid2 == userId)
                    .Select(f => f.userid1 == userId ? f.userid2 : f.userid1)
                    .ToList();

                // Retrieve the IDs of users who have already received invitations from the current user
                var invitedUserIds = _context.pendinginvite
                    .Where(pi => pi.inviterid == userId)
                    .Select(pi => pi.inviteeid)
                    .ToList();

                // Retrieve all users from the database excluding the current user, their friends, and users who have already received invitations
                var users = _context.users
                    .Where(u => u.id != userId && !friendIds.Contains(u.id) && !invitedUserIds.Contains(u.id))
                    .ToList();

                return Ok(users);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred while retrieving users: {ex.Message}");
            }
        }



        [HttpGet("get-all-users-debug")]
        public IActionResult GetAllUsersDebug()
        {
            try
            {
                // Retrieve all users from the database
                var users = _context.users.ToList();
                return Ok(users);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred while retrieving users: {ex.Message}");
            }

        }


        [HttpGet("get-all-friendships-debug")]
        public IActionResult GetAllfriendshipsDebug()
        {
            try
            {
                // Retrieve all users from the database
                var friendships = _context.friendship.ToList();
                return Ok(friendships);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred while retrieving users: {ex.Message}");
            }

        }


        [Authorize]
        [HttpPost("create-friendship")]
        public IActionResult CreateFriendship(Guid inviterId)
        {
            try
            {
                var userIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

                if (string.IsNullOrEmpty(userIdString))
                {
                    return BadRequest("User id not found in token.");
                }

                var userId = Guid.Parse(userIdString);

                // Check if both users exist
                var user1 = _context.users.Find(userId);
                var user2 = _context.users.Find(inviterId);

                if (user1 == null || user2 == null)
                {
                    return NotFound("One or both users not found.");
                }

                // Check if friendship already exists
                var existingFriendship = _context.friendship
                    .FirstOrDefault(uf => (uf.userid1 == userId && uf.userid2 == inviterId) || (uf.userid1 == inviterId && uf.userid2 == userId));

                if (existingFriendship != null)
                {
                    return Conflict("Friendship already exists.");
                }

                // Delete pending invitations between the two users
                var pendingInvitations = _context.pendinginvite
                    .Where(pi => (pi.inviterid == userId && pi.inviteeid == inviterId) || (pi.inviterid == inviterId && pi.inviteeid == userId));

                _context.pendinginvite.RemoveRange(pendingInvitations);

                // Create a new friendship
                var friendship = new Friendship
                {
                    userid1 = userId,
                    userid2 = inviterId,
                };

                _context.friendship.Add(friendship);
                _context.SaveChanges();

                return Ok("Friendship created successfully.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred while creating friendship: {ex.Message}");
            }
        }



        [Authorize]
        [HttpGet("get-friends")]
        public IActionResult GetFriends()
        {
            try
            {
                var userIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

                if (string.IsNullOrEmpty(userIdString))
                {
                    return BadRequest("User id not found in token.");
                }

                var userId = Guid.Parse(userIdString);

                // Find all friendships where the user is either the first or the second user
                var friendships = _context.friendship
                    .Where(uf => uf.userid1 == userId || uf.userid2 == userId)
                    .ToList();

                // Find the IDs of the user's friends
                var friendIds = friendships.Select(uf => uf.userid1 == userId ? uf.userid2 : uf.userid1).ToList();

               // Find the friend objects based on the IDs and join with the rank table to get the rank name
               var friendUsers = _context.users
                    .Where(u => friendIds.Contains(u.id))
                    .ToList();

                var friendRanks = _context.rank
                    .Where(r => friendUsers.Select(u => u.rankid).Contains(r.id))
                    .ToList();

                var friends = friendUsers
                    .Select(u => new
                    {
                        u.username,
                        u.lastmapplayed,
                        rankname = friendRanks.FirstOrDefault(r => r.id == u.rankid)?.name
                    })
                    .ToList();


                return Ok(friends);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred while retrieving friends");
            }
        }



        [HttpDelete("remove-invite/{inviteId}")]
        public IActionResult RemoveInvite(int inviteId)
        {
            try
            {
                // Find the invitation to remove
                var invitation = _context.pendinginvite.Find(inviteId);

                // If the invitation is not found, return a not found response
                if (invitation == null)
                {
                    return NotFound("Invitation not found.");
                }

                // Remove the invitation from the database
                _context.pendinginvite.Remove(invitation);
                _context.SaveChanges();

                // Return a success response
                return Ok("Invitation removed successfully.");
            }
            catch (Exception ex)
            {
                // Return an internal server error response if an exception occurs
                return StatusCode(500, $"An error occurred while removing the invitation");
            }
        }
    }
}