namespace RealtimeChat.Api.Models;

public class ChatroomMember
{
    public string UserId { get; set; } = string.Empty;
    public ApplicationUser User { get; set; } = null!;

    public int ChatroomId { get; set; }
    public Chatroom Chatroom { get; set; } = null!;

    public bool IsAdmin { get; set; } = false;
    public DateTime JoinedAt { get; set; } = DateTime.UtcNow;

    public bool IsBanned { get; set; } = false;
}