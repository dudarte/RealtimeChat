using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace RealtimeChat.Api.Models;

public class ApplicationUser : IdentityUser
{
    [Required]
    [MaxLength(50)]
    public string DisplayName { get; set; } = string.Empty;

    [Required]
    [MaxLength(20)]
    public string ShortId { get; set; } = string.Empty;

    public ICollection<ChatroomMember> Chatrooms { get; set; } = new List<ChatroomMember>();
    public ICollection<ChatMessage> Messages { get; set; } = new List<ChatMessage>();
}