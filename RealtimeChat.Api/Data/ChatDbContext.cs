using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using RealtimeChat.Api.Models;

namespace RealtimeChat.Api.Data;

public class ChatDbContext : IdentityDbContext<ApplicationUser>
{
    public ChatDbContext(DbContextOptions<ChatDbContext> options) : base(options) { }

    public DbSet<ChatMessage> Messages { get; set; }
    public DbSet<Chatroom> Chatrooms { get; set; }
    public DbSet<ChatroomMember> ChatroomMembers { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<ApplicationUser>().HasIndex(u => u.ShortId).IsUnique();

        builder.Entity<ChatroomMember>()
            .HasKey(cm => new { cm.UserId, cm.ChatroomId });

        builder.Entity<ChatroomMember>()
            .HasOne(cm => cm.User)
            .WithMany(u => u.Chatrooms)
            .HasForeignKey(cm => cm.UserId);

        builder.Entity<ChatroomMember>()
            .HasOne(cm => cm.Chatroom)
            .WithMany(c => c.Members)
            .HasForeignKey(cm => cm.ChatroomId);

        builder.Entity<ChatMessage>()
            .HasOne(m => m.Room)
            .WithMany(c => c.Messages)
            .HasForeignKey(m => m.ChatroomId);

        builder.Entity<ChatMessage>()
            .HasOne(m => m.User)
            .WithMany(u => u.Messages)
            .HasForeignKey(m => m.UserId);
    }
}