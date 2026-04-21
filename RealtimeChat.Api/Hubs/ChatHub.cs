using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using RealtimeChat.Api.Data;
using RealtimeChat.Api.Models;

namespace RealtimeChat.Api.Hubs;

[Authorize]
public class ChatHub : Hub
{
    private readonly ChatDbContext _context;

    public ChatHub(ChatDbContext context) 
    {
        _context = context;
    }

    public async Task JoinRoom(int roomId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, roomId.ToString());
    }

    public async Task LeaveRoom(int roomId)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, roomId.ToString());
    }

    public async Task SendMessage(int roomId, string message)
    {
        var email = Context.User?.Identity?.Name;
        
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);

        if (user == null) 
        {
            throw new HubException("User not found in database.");
        }

        var membership = await _context.ChatroomMembers
            .FirstOrDefaultAsync(m => m.UserId == user.Id && m.ChatroomId == roomId);

        if (membership != null && membership.IsBanned)
        {
            await Clients.Caller.SendAsync("ReceiveError", "You are banned from this room.");
            return; 
        }

        string detectedSentiment = AnalyzeSentiment(message);

        var chatMessage = new ChatMessage
        {
            UserId = user.Id,
            ChatroomId = roomId,
            Text = message,
            Sentiment = detectedSentiment,
            Timestamp = DateTime.UtcNow
        };

        _context.Messages.Add(chatMessage);
        await _context.SaveChangesAsync();

        await Clients.Group(roomId.ToString()).SendAsync(
            "ReceiveMessage", 
            chatMessage.Id,
            user.DisplayName, 
            user.ShortId, 
            message, 
            chatMessage.Timestamp);
    }
}