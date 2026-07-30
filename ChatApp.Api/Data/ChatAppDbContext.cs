using System;
using System.Collections.Generic;
using ChatApp.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace ChatApp.Api.Data;

public partial class ChatAppDbContext : DbContext
{
    public ChatAppDbContext()
    {
    }

    public ChatAppDbContext(DbContextOptions<ChatAppDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<DirectRoomPair> DirectRoomPairs { get; set; }

    public virtual DbSet<Friendship> Friendships { get; set; }

    public virtual DbSet<Message> Messages { get; set; }

    public virtual DbSet<MessageAttachment> MessageAttachments { get; set; }

    public virtual DbSet<MessageReaction> MessageReactions { get; set; }

    public virtual DbSet<MessageReadReceipt> MessageReadReceipts { get; set; }

    public virtual DbSet<Room> Rooms { get; set; }

    public virtual DbSet<RoomMember> RoomMembers { get; set; }

    public virtual DbSet<User> Users { get; set; }

    public virtual DbSet<UserConnection> UserConnections { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseSqlServer("Server=localhost;Database=ChatApplication;Trusted_Connection=True;TrustServerCertificate=True;");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<DirectRoomPair>(entity =>
        {
            entity.HasIndex(e => new { e.SmallerUserId, e.LargerUserId }, "UQ_DirectRoomPairs_Pair").IsUnique();

            entity.Property(e => e.Id).HasDefaultValueSql("(newid())");

            entity.HasOne(d => d.LargerUser).WithMany(p => p.DirectRoomPairLargerUsers)
                .HasForeignKey(d => d.LargerUserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_DirectRoomPairs_LargerUser");

            entity.HasOne(d => d.Room).WithMany(p => p.DirectRoomPairs)
                .HasForeignKey(d => d.RoomId)
                .HasConstraintName("FK_DirectRoomPairs_Room");

            entity.HasOne(d => d.SmallerUser).WithMany(p => p.DirectRoomPairSmallerUsers)
                .HasForeignKey(d => d.SmallerUserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_DirectRoomPairs_SmallerUser");
        });

        modelBuilder.Entity<Friendship>(entity =>
        {
            entity.HasIndex(e => new { e.RequesterId, e.AddresseeId }, "UQ_Friendships_Pair").IsUnique();

            entity.Property(e => e.Id).HasDefaultValueSql("(newid())");
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(sysutcdatetime())");
            entity.Property(e => e.Status)
                .HasMaxLength(20)
                .HasDefaultValue("Pending");

            entity.HasOne(d => d.Addressee).WithMany(p => p.FriendshipAddressees)
                .HasForeignKey(d => d.AddresseeId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Friendships_Addressee");

            entity.HasOne(d => d.Requester).WithMany(p => p.FriendshipRequesters)
                .HasForeignKey(d => d.RequesterId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Friendships_Requester");
        });

        modelBuilder.Entity<Message>(entity =>
        {
            entity.HasIndex(e => new { e.RoomId, e.SentAt }, "IX_Messages_RoomId_SentAt").IsDescending(false, true);

            entity.Property(e => e.Id).HasDefaultValueSql("(newid())");
            entity.Property(e => e.SentAt).HasDefaultValueSql("(sysutcdatetime())");
            entity.Property(e => e.Type)
                .HasMaxLength(20)
                .HasDefaultValue("Text");

            entity.HasOne(d => d.ReplyToMessage).WithMany(p => p.InverseReplyToMessage)
                .HasForeignKey(d => d.ReplyToMessageId)
                .HasConstraintName("FK_Messages_ReplyTo");

            entity.HasOne(d => d.Room).WithMany(p => p.Messages)
                .HasForeignKey(d => d.RoomId)
                .HasConstraintName("FK_Messages_Room");

            entity.HasOne(d => d.Sender).WithMany(p => p.Messages)
                .HasForeignKey(d => d.SenderId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Messages_Sender");
        });

        modelBuilder.Entity<MessageAttachment>(entity =>
        {
            entity.HasIndex(e => e.MessageId, "IX_MessageAttachments_MessageId");

            entity.Property(e => e.Id).HasDefaultValueSql("(newid())");
            entity.Property(e => e.FileType).HasMaxLength(50);
            entity.Property(e => e.FileUrl).HasMaxLength(500);

            entity.HasOne(d => d.Message).WithMany(p => p.MessageAttachments)
                .HasForeignKey(d => d.MessageId)
                .HasConstraintName("FK_MessageAttachments_Message");
        });

        modelBuilder.Entity<MessageReaction>(entity =>
        {
            entity.HasIndex(e => new { e.MessageId, e.UserId, e.Emoji }, "UQ_MessageReactions_Message_User_Emoji").IsUnique();

            entity.Property(e => e.Id).HasDefaultValueSql("(newid())");
            entity.Property(e => e.Emoji).HasMaxLength(10);

            entity.HasOne(d => d.Message).WithMany(p => p.MessageReactions)
                .HasForeignKey(d => d.MessageId)
                .HasConstraintName("FK_MessageReactions_Message");

            entity.HasOne(d => d.User).WithMany(p => p.MessageReactions)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("FK_MessageReactions_User");
        });

        modelBuilder.Entity<MessageReadReceipt>(entity =>
        {
            entity.HasIndex(e => new { e.MessageId, e.UserId }, "UQ_MessageReadReceipts_Message_User").IsUnique();

            entity.Property(e => e.Id).HasDefaultValueSql("(newid())");
            entity.Property(e => e.ReadAt).HasDefaultValueSql("(sysutcdatetime())");

            entity.HasOne(d => d.Message).WithMany(p => p.MessageReadReceipts)
                .HasForeignKey(d => d.MessageId)
                .HasConstraintName("FK_MessageReadReceipts_Message");

            entity.HasOne(d => d.User).WithMany(p => p.MessageReadReceipts)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("FK_MessageReadReceipts_User");
        });

        modelBuilder.Entity<Room>(entity =>
        {
            entity.Property(e => e.Id).HasDefaultValueSql("(newid())");
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(sysutcdatetime())");
            entity.Property(e => e.Name).HasMaxLength(100);
            entity.Property(e => e.Type).HasMaxLength(20);

            entity.HasOne(d => d.CreatedByNavigation).WithMany(p => p.Rooms)
                .HasForeignKey(d => d.CreatedBy)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("FK_Rooms_CreatedBy_Users");
        });

        modelBuilder.Entity<RoomMember>(entity =>
        {
            entity.HasIndex(e => e.UserId, "IX_RoomMembers_UserId");

            entity.HasIndex(e => new { e.RoomId, e.UserId }, "UQ_RoomMembers_Room_User").IsUnique();

            entity.Property(e => e.Id).HasDefaultValueSql("(newid())");
            entity.Property(e => e.JoinedAt).HasDefaultValueSql("(sysutcdatetime())");
            entity.Property(e => e.Role)
                .HasMaxLength(20)
                .HasDefaultValue("Member");

            entity.HasOne(d => d.Room).WithMany(p => p.RoomMembers)
                .HasForeignKey(d => d.RoomId)
                .HasConstraintName("FK_RoomMembers_Room");

            entity.HasOne(d => d.User).WithMany(p => p.RoomMembers)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("FK_RoomMembers_User");
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasIndex(e => e.Email, "UQ_Users_Email").IsUnique();

            entity.HasIndex(e => e.Username, "UQ_Users_Username").IsUnique();

            entity.Property(e => e.Id).HasDefaultValueSql("(newid())");
            entity.Property(e => e.AvatarUrl).HasMaxLength(500);
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(sysutcdatetime())");
            entity.Property(e => e.Email).HasMaxLength(255);
            entity.Property(e => e.PasswordHash).HasMaxLength(255);
            entity.Property(e => e.Status)
                .HasMaxLength(20)
                .HasDefaultValue("Offline");
            entity.Property(e => e.Username).HasMaxLength(50);
        });

        modelBuilder.Entity<UserConnection>(entity =>
        {
            entity.HasIndex(e => e.UserId, "IX_UserConnections_UserId");

            entity.HasIndex(e => e.ConnectionId, "UQ_UserConnections_ConnectionId").IsUnique();

            entity.Property(e => e.Id).HasDefaultValueSql("(newid())");
            entity.Property(e => e.ConnectedAt).HasDefaultValueSql("(sysutcdatetime())");
            entity.Property(e => e.ConnectionId).HasMaxLength(100);

            entity.HasOne(d => d.User).WithMany(p => p.UserConnections)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("FK_UserConnections_User");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
