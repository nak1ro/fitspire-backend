using backend.Modules.User.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace backend.Modules.User.Configuration;

public class AppUserConfiguration : IEntityTypeConfiguration<AppUser>
{
    public void Configure(EntityTypeBuilder<AppUser> builder)
    {
        builder.ToTable("User");

        builder.HasKey(u => u.Id);

        builder.Property(u => u.ProfilePictureUrl);
        builder.Property(u => u.IsPrivate).HasDefaultValue(false);
        builder.Property(u => u.CreatedAt).HasDefaultValueSql("NOW()");
        builder.Property(u => u.UpdatedAt).HasDefaultValueSql("NOW()");

        builder.HasOne(u => u.AppUserPreference)
            .WithOne(p => p.AppUser)
            .HasForeignKey<AppUserPreference>(p => p.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        // Workouts
        builder.HasMany(u => u.Workouts)
            .WithOne(w => w.User)
            .HasForeignKey(w => w.UserId)
            .OnDelete(DeleteBehavior.Cascade); // CASCADE

        // 📬 Notifications
        builder.HasMany(u => u.Notifications)
            .WithOne(n => n.User)
            .HasForeignKey(n => n.UserId)
            .OnDelete(DeleteBehavior.Cascade); // ✔️ CASCADE

        // 📝 Posts
        builder.HasMany(u => u.Posts)
            .WithOne(p => p.User)
            .HasForeignKey(p => p.UserId)
            .OnDelete(DeleteBehavior.Cascade); // ✔️ CASCADE

        // 💾 SavedPosts
        builder.HasMany(u => u.SavedPosts)
            .WithOne(sp => sp.User)
            .HasForeignKey(sp => sp.UserId)
            .OnDelete(DeleteBehavior.Cascade); // ✔️ CASCADE

        // 🎯 Goals
        builder.HasMany(u => u.Goals)
            .WithOne(g => g.User)
            .HasForeignKey(g => g.UserId)
            .OnDelete(DeleteBehavior.Cascade); // ✔️ CASCADE

        // 🏅 Badges
        builder.HasMany(u => u.Badges)
            .WithOne(b => b.User)
            .HasForeignKey(b => b.UserId)
            .OnDelete(DeleteBehavior.Cascade); // ✔️ CASCADE

        // 🍽 Meals
        builder.HasMany(u => u.Meals)
            .WithOne(m => m.User)
            .HasForeignKey(m => m.UserId)
            .OnDelete(DeleteBehavior.Cascade); // ✔️ CASCADE

        // 👥 GroupMemberships
        builder.HasMany(u => u.GroupMemberships)
            .WithOne(gm => gm.User)
            .HasForeignKey(gm => gm.UserId)
            .OnDelete(DeleteBehavior.Cascade); // ✔️ CASCADE

        // 👫 FriendshipSentRequests
        builder.HasMany(u => u.FriendshipSentRequests)
            .WithOne(r => r.Requester)
            .HasForeignKey(r => r.RequesterId)
            .OnDelete(DeleteBehavior.Cascade); // ✔️ CASCADE

        // 👫 FriendshipReceivedRequests
        builder.HasMany(u => u.FriendshipReceivedRequests)
            .WithOne(r => r.Addressee)
            .HasForeignKey(r => r.AddresseeId)
            .OnDelete(DeleteBehavior.Cascade); // ✔️ CASCADE

        // 🏆 ChallengeParticipants
        builder.HasMany(u => u.ChallengeParticipants)
            .WithOne(cp => cp.User)
            .HasForeignKey(cp => cp.UserId)
            .OnDelete(DeleteBehavior.Cascade); // ✔️ CASCADE

        // 📈 ProgressEntries
        builder.HasMany(u => u.ProgressEntries)
            .WithOne(pe => pe.User)
            .HasForeignKey(pe => pe.UserId)
            .OnDelete(DeleteBehavior.Cascade); // ✔️ CASCADE

        // 🚫 Bans
        builder.HasMany(u => u.Bans)
            .WithOne(b => b.User)
            .HasForeignKey(b => b.UserId)
            .OnDelete(DeleteBehavior.Cascade); // ✔️ CASCADE

        // 🏅 PersonalRecords
        builder.HasMany(u => u.PersonalRecords)
            .WithOne(pr => pr.User)
            .HasForeignKey(pr => pr.UserId)
            .OnDelete(DeleteBehavior.Cascade); // ✔️ CASCADE

        // 🏅 RecordHistory
        builder.HasMany(u => u.RecordHistory)
            .WithOne(prh => prh.User)
            .HasForeignKey(prh => prh.UserId)
            .OnDelete(DeleteBehavior.Cascade); // ✔️ CASCADE

        // ------------- NO CASCADE (Leave as orphaned) -------------

        // ❤️ Likes
        builder.HasMany(u => u.Likes)
            .WithOne(l => l.User)
            .HasForeignKey(l => l.UserId)
            .OnDelete(DeleteBehavior.NoAction); // ❌ NO CASCADE (orphan, handled in app)

        // 💬 Comments
        builder.HasMany(u => u.Comments)
            .WithOne(c => c.User)
            .HasForeignKey(c => c.UserId)
            .OnDelete(DeleteBehavior.NoAction); // ❌ NO CASCADE (orphan, handled in app)

        // 👥 Followers (who follows me)
        builder.HasMany(u => u.Followers)
            .WithOne(f => f.FollowedUser)
            .HasForeignKey(f => f.FollowedId)
            .OnDelete(DeleteBehavior.NoAction); // ❌ NO CASCADE

        // 👥 Following (who I follow)
        builder.HasMany(u => u.Following)
            .WithOne(f => f.FollowerUser)
            .HasForeignKey(f => f.FollowerId)
            .OnDelete(DeleteBehavior.NoAction); // ❌ NO CASCADE

        // 🏆 ChallengesCreated (as owner)
        builder.HasMany(u => u.ChallengesCreated)
            .WithOne(c => c.CreatedByUser)
            .HasForeignKey(c => c.CreatedBy)
            .OnDelete(DeleteBehavior.SetNull); // ✔️ SET NULL (challenge keeps existing, owner is null)

        // 🚩 ReportsMade
        builder.HasMany(u => u.ReportsMade)
            .WithOne(r => r.ReportedBy)
            .HasForeignKey(r => r.ReportedById)
            .OnDelete(DeleteBehavior.NoAction); // ❌ NO CASCADE

        // 🚩 ReportsReceived
        builder.HasMany(u => u.ReportsReceived)
            .WithOne(r => r.ReportedUser)
            .HasForeignKey(r => r.ReportedUserId)
            .OnDelete(DeleteBehavior.NoAction); // ❌ NO CASCADE

        // 👥 Friends
        builder.HasMany(u => u.Friends)
            .WithOne(f => f.User1)
            .HasForeignKey(f => f.User1Id)
            .OnDelete(DeleteBehavior.NoAction); // ❌ NO CASCADE

        // 👥 GroupsCreated
        builder.HasMany(u => u.GroupsCreated)
            .WithOne(g => g.CreatedBy)
            .HasForeignKey(g => g.CreatedById)
            .OnDelete(DeleteBehavior.SetNull);
    }
}