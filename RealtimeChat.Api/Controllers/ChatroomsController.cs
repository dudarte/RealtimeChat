using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RealtimeChat.Api.Data;
using RealtimeChat.Api.Models;
using System.Security.Claims;

namespace RealtimeChat.Api.Controllers;

[Authorize(AuthenticationSchemes = "Identity.Bearer")]
[ApiController]
[Route("api/[controller]")]
public class ChatroomsController : ControllerBase
{
    private readonly ChatDbContext _context;

    public ChatroomsController(ChatDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<IActionResult> GetChatrooms()
    {
        var rooms = await _context.Chatrooms
            .Select(r => new {
                r.Id,
                r.Name,
                r.Tags,
                CreationDate = r.CreatedAt,
                LastActivity = r.Messages.OrderByDescending(m => m.Timestamp).Select(m => m.Timestamp).FirstOrDefault(),
                MemberCount = r.Members.Count
            })
            .ToListAsync();
        
        var formattedRooms = rooms.Select(r => new {
            r.Id,
            r.Name,
            r.Tags,
            r.CreationDate,
            LastActivity = r.LastActivity == default ? r.CreationDate : r.LastActivity,
            r.MemberCount
        });

        return Ok(formattedRooms);
    }

    [HttpGet("{id}/messages")]
    public async Task<IActionResult> GetMessages(int id)
    {
        var messages = await _context.Messages
            .Include(m => m.User)
            .Where(m => m.ChatroomId == id)
            .OrderBy(m => m.Timestamp)
            .Select(m => new {
                m.Id,
                SenderName = m.User.DisplayName,
                SenderId = m.User.ShortId,
                m.Text,
                m.Timestamp
            })
            .ToListAsync();

        return Ok(messages);
    }

    [HttpPost]
    public async Task<IActionResult> CreateRoom([FromBody] CreateRoomRequest request)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var user = await _context.Users.FindAsync(userId);
        
        if (user == null) return Unauthorized("User not found.");

        var room = new Chatroom 
        { 
            Name = request.Name, 
            Tags = request.Tags 
        };

        var member = new ChatroomMember
        {
            UserId = user.Id,
            Chatroom = room,
            IsAdmin = true
        };

        _context.Chatrooms.Add(room);
        _context.ChatroomMembers.Add(member);
        
        await _context.SaveChangesAsync();
        
        return Ok(new { 
            room.Id, 
            room.Name, 
            room.Tags 
        });
    }

    [HttpGet("managed")]
    public async Task<IActionResult> GetManagedRooms()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        
        var managedRooms = await _context.Chatrooms
            .Where(r => r.Members.Any(m => m.UserId == userId && m.IsAdmin))
            .Select(r => new { r.Id, r.Name, r.Tags })
            .ToListAsync();

        return Ok(managedRooms);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateRoom(int id, [FromBody] UpdateRoomRequest request)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        
        var isAdmin = await _context.ChatroomMembers
            .AnyAsync(m => m.ChatroomId == id && m.UserId == userId && m.IsAdmin);
        
        if (!isAdmin) return Forbid();

        var room = await _context.Chatrooms.FindAsync(id);
        if (room == null) return NotFound();

        room.Name = request.Name;
        room.Tags = request.Tags;

        await _context.SaveChangesAsync();
        return Ok();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteRoom(int id)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        
        var isAdmin = await _context.ChatroomMembers
            .AnyAsync(m => m.ChatroomId == id && m.UserId == userId && m.IsAdmin);
        
        if (!isAdmin) return Forbid();

        var room = await _context.Chatrooms.FindAsync(id);
        if (room == null) return NotFound();

        _context.Chatrooms.Remove(room);
        await _context.SaveChangesAsync();

        return NoContent();
    }

    [HttpGet("{id}/is-admin")]
    public async Task<IActionResult> IsAdmin(int id)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var isAdmin = await _context.ChatroomMembers
            .AnyAsync(m => m.ChatroomId == id && m.UserId == userId && m.IsAdmin);
        return Ok(isAdmin);
    }

    [HttpPost("{roomId}/ban/{targetShortId}")]
    public async Task<IActionResult> BanUser(int roomId, string targetShortId)
    {
        var adminId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var isAdmin = await _context.ChatroomMembers.AnyAsync(m => m.ChatroomId == roomId && m.UserId == adminId && m.IsAdmin);
        if (!isAdmin) return Forbid();

        var targetUser = await _context.Users.FirstOrDefaultAsync(u => u.ShortId == targetShortId);
        if (targetUser == null) return NotFound();

        var membership = await _context.ChatroomMembers.FirstOrDefaultAsync(m => m.ChatroomId == roomId && m.UserId == targetUser.Id);
        if (membership == null)
        {
            membership = new ChatroomMember { ChatroomId = roomId, UserId = targetUser.Id, IsBanned = true };
            _context.ChatroomMembers.Add(membership);
        }
        else
        {
            membership.IsBanned = true;
        }

        await _context.SaveChangesAsync();
        return Ok();
    }

    [HttpDelete("{roomId}/messages/{messageId}")]
    public async Task<IActionResult> DeleteMessage(int roomId, int messageId)
    {
        var adminId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var isAdmin = await _context.ChatroomMembers.AnyAsync(m => m.ChatroomId == roomId && m.UserId == adminId && m.IsAdmin);
        if (!isAdmin) return Forbid();

        var message = await _context.Messages.FirstOrDefaultAsync(m => m.Id == messageId && m.ChatroomId == roomId);
        if (message != null)
        {
            _context.Messages.Remove(message);
            await _context.SaveChangesAsync();
        }
        return Ok();
    }
}

public class CreateRoomRequest
{
    public string Name { get; set; } = string.Empty;
    public List<string> Tags { get; set; } = new();
}

public class UpdateRoomRequest
{
    public string Name { get; set; } = string.Empty;
    public List<string> Tags { get; set; } = new();
}