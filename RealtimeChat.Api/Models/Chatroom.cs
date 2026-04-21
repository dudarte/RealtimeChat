using System.ComponentModel.DataAnnotations;

namespace RealtimeChat.Api.Models;

public class Chatroom
{
    [Key]
    public int Id { get; set; }

    [Required, MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public List<string> Tags { get; set; } = new();

    public ICollection<ChatroomMember> Members { get; set; } = new List<ChatroomMember>();
    public ICollection<ChatMessage> Messages { get; set; } = new List<ChatMessage>();
}