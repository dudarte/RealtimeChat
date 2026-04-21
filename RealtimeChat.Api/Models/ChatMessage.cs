using System.ComponentModel.DataAnnotations;

namespace RealtimeChat.Api.Models;

public class ChatMessage
{
    [Key]
    public int Id { get; set; }

    [Required]
    public string Text { get; set; } = string.Empty;

    public DateTime Timestamp { get; set; } = DateTime.UtcNow;

    public string? SentimentScore { get; set; } 

    public string UserId { get; set; } = string.Empty;
    public ApplicationUser User { get; set; } = null!;

    public int ChatroomId { get; set; }
    public Chatroom Room { get; set; } = null!;
}