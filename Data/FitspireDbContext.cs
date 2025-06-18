using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using backend.Modules.ScaffoldedModels;

namespace backend.Data;

public partial class FitspireDbContext : DbContext
{
    public FitspireDbContext(DbContextOptions<FitspireDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Badge> Badges { get; set; }

    public virtual DbSet<Ban> Bans { get; set; }

    public virtual DbSet<Challenge> Challenges { get; set; }

    public virtual DbSet<Challengeparticipant> Challengeparticipants { get; set; }

    public virtual DbSet<Comment> Comments { get; set; }

    public virtual DbSet<Cyclingworkoutdetail> Cyclingworkoutdetails { get; set; }

    public virtual DbSet<Exercise> Exercises { get; set; }

    public virtual DbSet<Exercisecategory> Exercisecategories { get; set; }

    public virtual DbSet<Follower> Followers { get; set; }

    public virtual DbSet<Friendship> Friendships { get; set; }

    public virtual DbSet<Friendshiprequest> Friendshiprequests { get; set; }

    public virtual DbSet<Goal> Goals { get; set; }

    public virtual DbSet<Goaltype> Goaltypes { get; set; }

    public virtual DbSet<Group> Groups { get; set; }

    public virtual DbSet<Groupmember> Groupmembers { get; set; }

    public virtual DbSet<Gymworkoutdetail> Gymworkoutdetails { get; set; }

    public virtual DbSet<Gymworkoutexercise> Gymworkoutexercises { get; set; }

    public virtual DbSet<Like> Likes { get; set; }

    public virtual DbSet<Meal> Meals { get; set; }

    public virtual DbSet<Mealitem> Mealitems { get; set; }

    public virtual DbSet<Notification> Notifications { get; set; }

    public virtual DbSet<Personalrecord> Personalrecords { get; set; }

    public virtual DbSet<Personalrecordhistory> Personalrecordhistories { get; set; }

    public virtual DbSet<Post> Posts { get; set; }

    public virtual DbSet<Progressentry> Progressentries { get; set; }

    public virtual DbSet<Report> Reports { get; set; }

    public virtual DbSet<Runningworkoutdetail> Runningworkoutdetails { get; set; }

    public virtual DbSet<Savedpost> Savedposts { get; set; }

    public virtual DbSet<Student> Students { get; set; }

    public virtual DbSet<Swimmingworkoutdetail> Swimmingworkoutdetails { get; set; }

    public virtual DbSet<User> Users { get; set; }

    public virtual DbSet<UserBadge> Userbadges { get; set; }

    public virtual DbSet<Userpreference> Userpreferences { get; set; }

    public virtual DbSet<Workout> Workouts { get; set; }

    public virtual DbSet<Yogaworkoutdetail> Yogaworkoutdetails { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasPostgresExtension("uuid-ossp");

        modelBuilder.Entity<Badge>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("badge_pkey");

            entity.ToTable("badge");

            entity.Property(e => e.Id)
                .HasDefaultValueSql("uuid_generate_v4()")
                .HasColumnName("id");
            entity.Property(e => e.Description).HasColumnName("description");
            entity.Property(e => e.IconUrl)
                .HasColumnType("character varying")
                .HasColumnName("iconurl");
            entity.Property(e => e.Name)
                .HasColumnType("character varying")
                .HasColumnName("name");
        });

        modelBuilder.Entity<Ban>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("ban_pkey");

            entity.ToTable("ban");

            entity.Property(e => e.Id)
                .HasDefaultValueSql("uuid_generate_v4()")
                .HasColumnName("id");
            entity.Property(e => e.Bannedat)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("bannedat");
            entity.Property(e => e.Expiresat)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("expiresat");
            entity.Property(e => e.Reason).HasColumnName("reason");
            entity.Property(e => e.Userid).HasColumnName("userid");

            entity.HasOne(d => d.User).WithMany(p => p.Bans)
                .HasForeignKey(d => d.Userid)
                .HasConstraintName("ban_userid_fkey");
        });

        modelBuilder.Entity<Challenge>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("challenge_pkey");

            entity.ToTable("challenge");

            entity.Property(e => e.Id)
                .HasDefaultValueSql("uuid_generate_v4()")
                .HasColumnName("id");
            entity.Property(e => e.Createdby).HasColumnName("createdby");
            entity.Property(e => e.Description).HasColumnName("description");
            entity.Property(e => e.Enddate).HasColumnName("enddate");
            entity.Property(e => e.Startdate).HasColumnName("startdate");
            entity.Property(e => e.Title)
                .HasColumnType("character varying")
                .HasColumnName("title");

            entity.HasOne(d => d.CreatedbyNavigation).WithMany(p => p.Challenges)
                .HasForeignKey(d => d.Createdby)
                .HasConstraintName("challenge_createdby_fkey");
        });

        modelBuilder.Entity<Challengeparticipant>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("challengeparticipant_pkey");

            entity.ToTable("challengeparticipant");

            entity.HasIndex(e => new { e.Challengeid, e.Userid }, "challengeparticipant_challengeid_userid_key").IsUnique();

            entity.Property(e => e.Id)
                .HasDefaultValueSql("uuid_generate_v4()")
                .HasColumnName("id");
            entity.Property(e => e.Challengeid).HasColumnName("challengeid");
            entity.Property(e => e.Score).HasColumnName("score");
            entity.Property(e => e.Userid).HasColumnName("userid");

            entity.HasOne(d => d.Challenge).WithMany(p => p.Challengeparticipants)
                .HasForeignKey(d => d.Challengeid)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("challengeparticipant_challengeid_fkey");

            entity.HasOne(d => d.User).WithMany(p => p.Challengeparticipants)
                .HasForeignKey(d => d.Userid)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("challengeparticipant_userid_fkey");
        });

        modelBuilder.Entity<Comment>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("comment_pkey");

            entity.ToTable("comment");

            entity.Property(e => e.Id)
                .HasDefaultValueSql("uuid_generate_v4()")
                .HasColumnName("id");
            entity.Property(e => e.Content).HasColumnName("content");
            entity.Property(e => e.Createdat)
                .HasDefaultValueSql("now()")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("createdat");
            entity.Property(e => e.Postid).HasColumnName("postid");
            entity.Property(e => e.Userid).HasColumnName("userid");

            entity.HasOne(d => d.Post).WithMany(p => p.Comments)
                .HasForeignKey(d => d.Postid)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("comment_postid_fkey");

            entity.HasOne(d => d.User).WithMany(p => p.Comments)
                .HasForeignKey(d => d.Userid)
                .HasConstraintName("comment_userid_fkey");
        });

        modelBuilder.Entity<Cyclingworkoutdetail>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("cyclingworkoutdetails_pkey");

            entity.ToTable("cyclingworkoutdetails");

            entity.HasIndex(e => e.Workoutid, "cyclingworkoutdetails_workoutid_key").IsUnique();

            entity.Property(e => e.Id)
                .HasDefaultValueSql("uuid_generate_v4()")
                .HasColumnName("id");
            entity.Property(e => e.Avgspeedkmperhour).HasColumnName("avgspeedkmperhour");
            entity.Property(e => e.Distancekm).HasColumnName("distancekm");
            entity.Property(e => e.Elevationgain).HasColumnName("elevationgain");
            entity.Property(e => e.Workoutid).HasColumnName("workoutid");

            entity.HasOne(d => d.Workout).WithOne(p => p.Cyclingworkoutdetail)
                .HasForeignKey<Cyclingworkoutdetail>(d => d.Workoutid)
                .HasConstraintName("cyclingworkoutdetails_workoutid_fkey");
        });

        modelBuilder.Entity<Exercise>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("exercise_pkey");

            entity.ToTable("exercise");

            entity.Property(e => e.Id)
                .HasDefaultValueSql("uuid_generate_v4()")
                .HasColumnName("id");
            entity.Property(e => e.Categoryid).HasColumnName("categoryid");
            entity.Property(e => e.Description).HasColumnName("description");
            entity.Property(e => e.Name)
                .HasColumnType("character varying")
                .HasColumnName("name");

            entity.HasOne(d => d.Category).WithMany(p => p.Exercises)
                .HasForeignKey(d => d.Categoryid)
                .HasConstraintName("exercise_categoryid_fkey");
        });

        modelBuilder.Entity<Exercisecategory>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("exercisecategory_pkey");

            entity.ToTable("exercisecategory");

            entity.Property(e => e.Id)
                .HasDefaultValueSql("uuid_generate_v4()")
                .HasColumnName("id");
            entity.Property(e => e.Description).HasColumnName("description");
            entity.Property(e => e.Name)
                .HasColumnType("character varying")
                .HasColumnName("name");
        });

        modelBuilder.Entity<Follower>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("follower_pkey");

            entity.ToTable("follower");

            entity.HasIndex(e => new { e.Followerid, e.Followedid }, "follower_followerid_followedid_key").IsUnique();

            entity.Property(e => e.Id)
                .HasDefaultValueSql("uuid_generate_v4()")
                .HasColumnName("id");
            entity.Property(e => e.Createdat)
                .HasDefaultValueSql("now()")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("createdat");
            entity.Property(e => e.Followedid).HasColumnName("followedid");
            entity.Property(e => e.Followerid).HasColumnName("followerid");

            entity.HasOne(d => d.Followed).WithMany(p => p.FollowerFolloweds)
                .HasForeignKey(d => d.Followedid)
                .HasConstraintName("follower_followedid_fkey");

            entity.HasOne(d => d.FollowerNavigation).WithMany(p => p.FollowerFollowerNavigations)
                .HasForeignKey(d => d.Followerid)
                .HasConstraintName("follower_followerid_fkey");
        });

        modelBuilder.Entity<Friendship>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("friendship_pkey");

            entity.ToTable("friendship");

            entity.HasIndex(e => e.Requestid, "friendship_requestid_key").IsUnique();

            entity.Property(e => e.Id)
                .HasDefaultValueSql("uuid_generate_v4()")
                .HasColumnName("id");
            entity.Property(e => e.Becamefriendsat)
                .HasDefaultValueSql("now()")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("becamefriendsat");
            entity.Property(e => e.Requestid).HasColumnName("requestid");
            entity.Property(e => e.User1id).HasColumnName("user1id");
            entity.Property(e => e.User2id).HasColumnName("user2id");

            entity.HasOne(d => d.Request).WithOne(p => p.Friendship)
                .HasForeignKey<Friendship>(d => d.Requestid)
                .HasConstraintName("friendship_requestid_fkey");

            entity.HasOne(d => d.User1).WithMany(p => p.FriendshipUser1s)
                .HasForeignKey(d => d.User1id)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("friendship_user1id_fkey");

            entity.HasOne(d => d.User2).WithMany(p => p.FriendshipUser2s)
                .HasForeignKey(d => d.User2id)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("friendship_user2id_fkey");
        });

        modelBuilder.Entity<Friendshiprequest>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("friendshiprequest_pkey");

            entity.ToTable("friendshiprequest");

            entity.Property(e => e.Id)
                .HasDefaultValueSql("uuid_generate_v4()")
                .HasColumnName("id");
            entity.Property(e => e.Addresseeid).HasColumnName("addresseeid");
            entity.Property(e => e.Requestedat)
                .HasDefaultValueSql("now()")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("requestedat");
            entity.Property(e => e.Requesterid).HasColumnName("requesterid");
            entity.Property(e => e.Respondedat)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("respondedat");
            entity.Property(e => e.Status)
                .HasColumnType("character varying")
                .HasColumnName("status");

            entity.HasOne(d => d.Addressee).WithMany(p => p.FriendshiprequestAddressees)
                .HasForeignKey(d => d.Addresseeid)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("friendshiprequest_addresseeid_fkey");

            entity.HasOne(d => d.Requester).WithMany(p => p.FriendshiprequestRequesters)
                .HasForeignKey(d => d.Requesterid)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("friendshiprequest_requesterid_fkey");
        });

        modelBuilder.Entity<Goal>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("goal_pkey");

            entity.ToTable("goal");

            entity.Property(e => e.Id)
                .HasDefaultValueSql("uuid_generate_v4()")
                .HasColumnName("id");
            entity.Property(e => e.Currentvalue).HasColumnName("currentvalue");
            entity.Property(e => e.Deadline).HasColumnName("deadline");
            entity.Property(e => e.Goaltypeid).HasColumnName("goaltypeid");
            entity.Property(e => e.Targetvalue).HasColumnName("targetvalue");
            entity.Property(e => e.Unit)
                .HasColumnType("character varying")
                .HasColumnName("unit");
            entity.Property(e => e.Userid).HasColumnName("userid");

            entity.HasOne(d => d.Goaltype).WithMany(p => p.Goals)
                .HasForeignKey(d => d.Goaltypeid)
                .HasConstraintName("goal_goaltypeid_fkey");

            entity.HasOne(d => d.User).WithMany(p => p.Goals)
                .HasForeignKey(d => d.Userid)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("goal_userid_fkey");
        });

        modelBuilder.Entity<Goaltype>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("goaltype_pkey");

            entity.ToTable("goaltype");

            entity.Property(e => e.Id)
                .HasDefaultValueSql("uuid_generate_v4()")
                .HasColumnName("id");
            entity.Property(e => e.Category)
                .HasColumnType("character varying")
                .HasColumnName("category");
            entity.Property(e => e.Description).HasColumnName("description");
            entity.Property(e => e.Iconurl)
                .HasColumnType("character varying")
                .HasColumnName("iconurl");
            entity.Property(e => e.Name)
                .HasColumnType("character varying")
                .HasColumnName("name");
            entity.Property(e => e.Unit)
                .HasColumnType("character varying")
                .HasColumnName("unit");
        });

        modelBuilder.Entity<Group>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("Group_pkey");

            entity.ToTable("Group");

            entity.Property(e => e.Id)
                .HasDefaultValueSql("uuid_generate_v4()")
                .HasColumnName("id");
            entity.Property(e => e.Createdat)
                .HasDefaultValueSql("now()")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("createdat");
            entity.Property(e => e.Createdby).HasColumnName("createdby");
            entity.Property(e => e.Description).HasColumnName("description");
            entity.Property(e => e.Isprivate)
                .HasDefaultValue(false)
                .HasColumnName("isprivate");
            entity.Property(e => e.Name)
                .HasColumnType("character varying")
                .HasColumnName("name");

            entity.HasOne(d => d.CreatedbyNavigation).WithMany(p => p.Groups)
                .HasForeignKey(d => d.Createdby)
                .HasConstraintName("Group_createdby_fkey");
        });

        modelBuilder.Entity<Groupmember>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("groupmember_pkey");

            entity.ToTable("groupmember");

            entity.HasIndex(e => new { e.Groupid, e.Userid }, "groupmember_groupid_userid_key").IsUnique();

            entity.Property(e => e.Id)
                .HasDefaultValueSql("uuid_generate_v4()")
                .HasColumnName("id");
            entity.Property(e => e.Groupid).HasColumnName("groupid");
            entity.Property(e => e.Joinedat)
                .HasDefaultValueSql("now()")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("joinedat");
            entity.Property(e => e.Userid).HasColumnName("userid");

            entity.HasOne(d => d.Group).WithMany(p => p.Groupmembers)
                .HasForeignKey(d => d.Groupid)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("groupmember_groupid_fkey");

            entity.HasOne(d => d.User).WithMany(p => p.Groupmembers)
                .HasForeignKey(d => d.Userid)
                .HasConstraintName("groupmember_userid_fkey");
        });

        modelBuilder.Entity<Gymworkoutdetail>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("gymworkoutdetails_pkey");

            entity.ToTable("gymworkoutdetails");

            entity.HasIndex(e => e.Workoutid, "gymworkoutdetails_workoutid_key").IsUnique();

            entity.Property(e => e.Id)
                .HasDefaultValueSql("uuid_generate_v4()")
                .HasColumnName("id");
            entity.Property(e => e.Intensitylevel)
                .HasColumnType("character varying")
                .HasColumnName("intensitylevel");
            entity.Property(e => e.Notes).HasColumnName("notes");
            entity.Property(e => e.Splittype)
                .HasColumnType("character varying")
                .HasColumnName("splittype");
            entity.Property(e => e.Workoutid).HasColumnName("workoutid");

            entity.HasOne(d => d.Workout).WithOne(p => p.Gymworkoutdetail)
                .HasForeignKey<Gymworkoutdetail>(d => d.Workoutid)
                .HasConstraintName("gymworkoutdetails_workoutid_fkey");
        });

        modelBuilder.Entity<Gymworkoutexercise>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("gymworkoutexercise_pkey");

            entity.ToTable("gymworkoutexercise");

            entity.Property(e => e.Id)
                .HasDefaultValueSql("uuid_generate_v4()")
                .HasColumnName("id");
            entity.Property(e => e.Durationminutes).HasColumnName("durationminutes");
            entity.Property(e => e.Exerciseid).HasColumnName("exerciseid");
            entity.Property(e => e.Gymworkoutid).HasColumnName("gymworkoutid");
            entity.Property(e => e.Orderindex).HasColumnName("orderindex");
            entity.Property(e => e.Reps).HasColumnName("reps");
            entity.Property(e => e.Sets).HasColumnName("sets");
            entity.Property(e => e.Weight).HasColumnName("weight");

            entity.HasOne(d => d.Exercise).WithMany(p => p.Gymworkoutexercises)
                .HasForeignKey(d => d.Exerciseid)
                .HasConstraintName("gymworkoutexercise_exerciseid_fkey");

            entity.HasOne(d => d.Gymworkout).WithMany(p => p.Gymworkoutexercises)
                .HasForeignKey(d => d.Gymworkoutid)
                .HasConstraintName("gymworkoutexercise_gymworkoutid_fkey");
        });

        modelBuilder.Entity<Like>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("Like_pkey");

            entity.ToTable("Like");

            entity.HasIndex(e => new { e.Userid, e.Targetid, e.Targettype }, "Like_userid_targetid_targettype_key").IsUnique();

            entity.Property(e => e.Id)
                .HasDefaultValueSql("uuid_generate_v4()")
                .HasColumnName("id");
            entity.Property(e => e.Createdat)
                .HasDefaultValueSql("now()")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("createdat");
            entity.Property(e => e.Targetid).HasColumnName("targetid");
            entity.Property(e => e.Targettype)
                .HasColumnType("character varying")
                .HasColumnName("targettype");
            entity.Property(e => e.Userid).HasColumnName("userid");

            entity.HasOne(d => d.User).WithMany(p => p.Likes)
                .HasForeignKey(d => d.Userid)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("Like_userid_fkey");
        });

        modelBuilder.Entity<Meal>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("meal_pkey");

            entity.ToTable("meal");

            entity.Property(e => e.Id)
                .HasDefaultValueSql("uuid_generate_v4()")
                .HasColumnName("id");
            entity.Property(e => e.Createdat)
                .HasDefaultValueSql("now()")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("createdat");
            entity.Property(e => e.Date).HasColumnName("date");
            entity.Property(e => e.Totalcalories).HasColumnName("totalcalories");
            entity.Property(e => e.Userid).HasColumnName("userid");

            entity.HasOne(d => d.User).WithMany(p => p.Meals)
                .HasForeignKey(d => d.Userid)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("meal_userid_fkey");
        });

        modelBuilder.Entity<Mealitem>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("mealitem_pkey");

            entity.ToTable("mealitem");

            entity.Property(e => e.Id)
                .HasDefaultValueSql("uuid_generate_v4()")
                .HasColumnName("id");
            entity.Property(e => e.Calories).HasColumnName("calories");
            entity.Property(e => e.Carbs).HasColumnName("carbs");
            entity.Property(e => e.Fat).HasColumnName("fat");
            entity.Property(e => e.Mealid).HasColumnName("mealid");
            entity.Property(e => e.Name)
                .HasColumnType("character varying")
                .HasColumnName("name");
            entity.Property(e => e.Protein).HasColumnName("protein");
            entity.Property(e => e.Quantity).HasColumnName("quantity");
            entity.Property(e => e.Unit)
                .HasColumnType("character varying")
                .HasColumnName("unit");

            entity.HasOne(d => d.Meal).WithMany(p => p.Mealitems)
                .HasForeignKey(d => d.Mealid)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("mealitem_mealid_fkey");
        });

        modelBuilder.Entity<Notification>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("notification_pkey");

            entity.ToTable("notification");

            entity.Property(e => e.Id)
                .HasDefaultValueSql("uuid_generate_v4()")
                .HasColumnName("id");
            entity.Property(e => e.Createdat)
                .HasDefaultValueSql("now()")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("createdat");
            entity.Property(e => e.Isread)
                .HasDefaultValue(false)
                .HasColumnName("isread");
            entity.Property(e => e.Message).HasColumnName("message");
            entity.Property(e => e.Userid).HasColumnName("userid");

            entity.HasOne(d => d.User).WithMany(p => p.Notifications)
                .HasForeignKey(d => d.Userid)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("notification_userid_fkey");
        });

        modelBuilder.Entity<Personalrecord>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("personalrecord_pkey");

            entity.ToTable("personalrecord");

            entity.HasIndex(e => new { e.Userid, e.Workouttype, e.Metric }, "personalrecord_userid_workouttype_metric_key").IsUnique();

            entity.Property(e => e.Id)
                .HasDefaultValueSql("uuid_generate_v4()")
                .HasColumnName("id");
            entity.Property(e => e.Createdat)
                .HasDefaultValueSql("now()")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("createdat");
            entity.Property(e => e.Metric)
                .HasColumnType("character varying")
                .HasColumnName("metric");
            entity.Property(e => e.Userid).HasColumnName("userid");
            entity.Property(e => e.Value).HasColumnName("value");
            entity.Property(e => e.Workoutid).HasColumnName("workoutid");
            entity.Property(e => e.Workouttype)
                .HasColumnType("character varying")
                .HasColumnName("workouttype");

            entity.HasOne(d => d.User).WithMany(p => p.Personalrecords)
                .HasForeignKey(d => d.Userid)
                .HasConstraintName("personalrecord_userid_fkey");

            entity.HasOne(d => d.Workout).WithMany(p => p.Personalrecords)
                .HasForeignKey(d => d.Workoutid)
                .HasConstraintName("personalrecord_workoutid_fkey");
        });

        modelBuilder.Entity<Personalrecordhistory>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("personalrecordhistory_pkey");

            entity.ToTable("personalrecordhistory");

            entity.Property(e => e.Id)
                .HasDefaultValueSql("uuid_generate_v4()")
                .HasColumnName("id");
            entity.Property(e => e.Metric)
                .HasColumnType("character varying")
                .HasColumnName("metric");
            entity.Property(e => e.Recordedat)
                .HasDefaultValueSql("now()")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("recordedat");
            entity.Property(e => e.Userid).HasColumnName("userid");
            entity.Property(e => e.Value).HasColumnName("value");
            entity.Property(e => e.Workoutid).HasColumnName("workoutid");
            entity.Property(e => e.Workouttype)
                .HasColumnType("character varying")
                .HasColumnName("workouttype");

            entity.HasOne(d => d.User).WithMany(p => p.Personalrecordhistories)
                .HasForeignKey(d => d.Userid)
                .HasConstraintName("personalrecordhistory_userid_fkey");

            entity.HasOne(d => d.Workout).WithMany(p => p.Personalrecordhistories)
                .HasForeignKey(d => d.Workoutid)
                .HasConstraintName("personalrecordhistory_workoutid_fkey");
        });

        modelBuilder.Entity<Post>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("post_pkey");

            entity.ToTable("post");

            entity.Property(e => e.Id)
                .HasDefaultValueSql("uuid_generate_v4()")
                .HasColumnName("id");
            entity.Property(e => e.Content).HasColumnName("content");
            entity.Property(e => e.Createdat)
                .HasDefaultValueSql("now()")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("createdat");
            entity.Property(e => e.Createdby).HasColumnName("createdby");
            entity.Property(e => e.Imageurl)
                .HasColumnType("character varying")
                .HasColumnName("imageurl");
            entity.Property(e => e.Updatedby).HasColumnName("updatedby");
            entity.Property(e => e.Userid).HasColumnName("userid");

            entity.HasOne(d => d.CreatedbyNavigation).WithMany(p => p.PostCreatedbyNavigations)
                .HasForeignKey(d => d.Createdby)
                .HasConstraintName("post_createdby_fkey");

            entity.HasOne(d => d.UpdatedbyNavigation).WithMany(p => p.PostUpdatedbyNavigations)
                .HasForeignKey(d => d.Updatedby)
                .HasConstraintName("post_updatedby_fkey");

            entity.HasOne(d => d.User).WithMany(p => p.PostUsers)
                .HasForeignKey(d => d.Userid)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("post_userid_fkey");
        });

        modelBuilder.Entity<Progressentry>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("progressentry_pkey");

            entity.ToTable("progressentry");

            entity.Property(e => e.Id)
                .HasDefaultValueSql("uuid_generate_v4()")
                .HasColumnName("id");
            entity.Property(e => e.Bodyfat).HasColumnName("bodyfat");
            entity.Property(e => e.Date).HasColumnName("date");
            entity.Property(e => e.Goalid).HasColumnName("goalid");
            entity.Property(e => e.Notes).HasColumnName("notes");
            entity.Property(e => e.Userid).HasColumnName("userid");
            entity.Property(e => e.Weight).HasColumnName("weight");

            entity.HasOne(d => d.Goal).WithMany(p => p.Progressentries)
                .HasForeignKey(d => d.Goalid)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("progressentry_goalid_fkey");

            entity.HasOne(d => d.User).WithMany(p => p.Progressentries)
                .HasForeignKey(d => d.Userid)
                .HasConstraintName("progressentry_userid_fkey");
        });

        modelBuilder.Entity<Report>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("report_pkey");

            entity.ToTable("report");

            entity.Property(e => e.Id)
                .HasDefaultValueSql("uuid_generate_v4()")
                .HasColumnName("id");
            entity.Property(e => e.Createdat)
                .HasDefaultValueSql("now()")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("createdat");
            entity.Property(e => e.Reason).HasColumnName("reason");
            entity.Property(e => e.Reportedby).HasColumnName("reportedby");
            entity.Property(e => e.Reporteduser).HasColumnName("reporteduser");

            entity.HasOne(d => d.ReportedbyNavigation).WithMany(p => p.ReportReportedbyNavigations)
                .HasForeignKey(d => d.Reportedby)
                .HasConstraintName("report_reportedby_fkey");

            entity.HasOne(d => d.ReporteduserNavigation).WithMany(p => p.ReportReporteduserNavigations)
                .HasForeignKey(d => d.Reporteduser)
                .HasConstraintName("report_reporteduser_fkey");
        });

        modelBuilder.Entity<Runningworkoutdetail>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("runningworkoutdetails_pkey");

            entity.ToTable("runningworkoutdetails");

            entity.HasIndex(e => e.Workoutid, "runningworkoutdetails_workoutid_key").IsUnique();

            entity.Property(e => e.Id)
                .HasDefaultValueSql("uuid_generate_v4()")
                .HasColumnName("id");
            entity.Property(e => e.Avgpaceminperkm).HasColumnName("avgpaceminperkm");
            entity.Property(e => e.Distancekm).HasColumnName("distancekm");
            entity.Property(e => e.Workoutid).HasColumnName("workoutid");

            entity.HasOne(d => d.Workout).WithOne(p => p.Runningworkoutdetail)
                .HasForeignKey<Runningworkoutdetail>(d => d.Workoutid)
                .HasConstraintName("runningworkoutdetails_workoutid_fkey");
        });

        modelBuilder.Entity<Savedpost>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("savedpost_pkey");

            entity.ToTable("savedpost");

            entity.HasIndex(e => new { e.Userid, e.Postid }, "savedpost_userid_postid_key").IsUnique();

            entity.Property(e => e.Id)
                .HasDefaultValueSql("uuid_generate_v4()")
                .HasColumnName("id");
            entity.Property(e => e.Postid).HasColumnName("postid");
            entity.Property(e => e.Savedat)
                .HasDefaultValueSql("now()")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("savedat");
            entity.Property(e => e.Userid).HasColumnName("userid");

            entity.HasOne(d => d.Post).WithMany(p => p.Savedposts)
                .HasForeignKey(d => d.Postid)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("savedpost_postid_fkey");

            entity.HasOne(d => d.User).WithMany(p => p.Savedposts)
                .HasForeignKey(d => d.Userid)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("savedpost_userid_fkey");
        });

        modelBuilder.Entity<Student>(entity =>
        {
            entity.HasKey(e => e.Idstudent).HasName("student_pkey");

            entity.ToTable("student");

            entity.Property(e => e.Idstudent)
                .ValueGeneratedNever()
                .HasColumnName("idstudent");
            entity.Property(e => e.Nazwisko)
                .HasMaxLength(100)
                .HasColumnName("nazwisko");
        });

        modelBuilder.Entity<Swimmingworkoutdetail>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("swimmingworkoutdetails_pkey");

            entity.ToTable("swimmingworkoutdetails");

            entity.HasIndex(e => e.Workoutid, "swimmingworkoutdetails_workoutid_key").IsUnique();

            entity.Property(e => e.Id)
                .HasDefaultValueSql("uuid_generate_v4()")
                .HasColumnName("id");
            entity.Property(e => e.Distancemeters).HasColumnName("distancemeters");
            entity.Property(e => e.Laps).HasColumnName("laps");
            entity.Property(e => e.Stroketype)
                .HasColumnType("character varying")
                .HasColumnName("stroketype");
            entity.Property(e => e.Workoutid).HasColumnName("workoutid");

            entity.HasOne(d => d.Workout).WithOne(p => p.Swimmingworkoutdetail)
                .HasForeignKey<Swimmingworkoutdetail>(d => d.Workoutid)
                .HasConstraintName("swimmingworkoutdetails_workoutid_fkey");
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("User_pkey");

            entity.ToTable("User");

            entity.HasIndex(e => e.Email, "User_email_key").IsUnique();

            entity.HasIndex(e => e.Username, "User_username_key").IsUnique();

            entity.Property(e => e.Id)
                .HasDefaultValueSql("uuid_generate_v4()")
                .HasColumnName("id");
            entity.Property(e => e.Bio).HasColumnName("bio");
            entity.Property(e => e.Createdat)
                .HasDefaultValueSql("now()")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("createdat");
            entity.Property(e => e.Displayname)
                .HasColumnType("character varying")
                .HasColumnName("displayname");
            entity.Property(e => e.Email)
                .HasColumnType("character varying")
                .HasColumnName("email");
            entity.Property(e => e.Isprivate)
                .HasDefaultValue(false)
                .HasColumnName("isprivate");
            entity.Property(e => e.Passwordhash)
                .HasColumnType("character varying")
                .HasColumnName("passwordhash");
            entity.Property(e => e.Profilepictureurl)
                .HasColumnType("character varying")
                .HasColumnName("profilepictureurl");
            entity.Property(e => e.Updatedat)
                .HasDefaultValueSql("now()")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("updatedat");
            entity.Property(e => e.Username)
                .HasColumnType("character varying")
                .HasColumnName("username");
        });

        modelBuilder.Entity<UserBadge>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("userbadge_pkey");

            entity.ToTable("userbadge");

            entity.HasIndex(e => new { e.Userid, e.Badgeid }, "userbadge_userid_badgeid_key").IsUnique();

            entity.Property(e => e.Id)
                .HasDefaultValueSql("uuid_generate_v4()")
                .HasColumnName("id");
            entity.Property(e => e.Awardedat)
                .HasDefaultValueSql("now()")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("awardedat");
            entity.Property(e => e.Badgeid).HasColumnName("badgeid");
            entity.Property(e => e.Userid).HasColumnName("userid");

            entity.HasOne(d => d.Badge).WithMany(p => p.UserBadges)
                .HasForeignKey(d => d.Badgeid)
                .HasConstraintName("userbadge_badgeid_fkey");

            entity.HasOne(d => d.User).WithMany(p => p.Userbadges)
                .HasForeignKey(d => d.Userid)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("userbadge_userid_fkey");
        });

        modelBuilder.Entity<Userpreference>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("userpreference_pkey");

            entity.ToTable("userpreference");

            entity.HasIndex(e => e.Userid, "userpreference_userid_key").IsUnique();

            entity.Property(e => e.Id)
                .HasDefaultValueSql("uuid_generate_v4()")
                .HasColumnName("id");
            entity.Property(e => e.Createdat)
                .HasDefaultValueSql("now()")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("createdat");
            entity.Property(e => e.Isdarkmodeenabled)
                .HasDefaultValue(false)
                .HasColumnName("isdarkmodeenabled");
            entity.Property(e => e.Preferredlanguage)
                .HasDefaultValueSql("'en'::character varying")
                .HasColumnType("character varying")
                .HasColumnName("preferredlanguage");
            entity.Property(e => e.Receiveemailnotifications)
                .HasDefaultValue(true)
                .HasColumnName("receiveemailnotifications");
            entity.Property(e => e.Unitsystem)
                .HasDefaultValueSql("'metric'::character varying")
                .HasColumnType("character varying")
                .HasColumnName("unitsystem");
            entity.Property(e => e.Updatedat)
                .HasDefaultValueSql("now()")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("updatedat");
            entity.Property(e => e.Userid).HasColumnName("userid");

            entity.HasOne(d => d.User).WithOne(p => p.Userpreference)
                .HasForeignKey<Userpreference>(d => d.Userid)
                .HasConstraintName("userpreference_userid_fkey");
        });

        modelBuilder.Entity<Workout>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("workout_pkey");

            entity.ToTable("workout");

            entity.Property(e => e.Id)
                .HasDefaultValueSql("uuid_generate_v4()")
                .HasColumnName("id");
            entity.Property(e => e.Createdat)
                .HasDefaultValueSql("now()")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("createdat");
            entity.Property(e => e.Createdby).HasColumnName("createdby");
            entity.Property(e => e.Createdfromroutineid).HasColumnName("createdfromroutineid");
            entity.Property(e => e.Date).HasColumnName("date");
            entity.Property(e => e.Durationminutes).HasColumnName("durationminutes");
            entity.Property(e => e.Isprivate)
                .HasDefaultValue(false)
                .HasColumnName("isprivate");
            entity.Property(e => e.Isroutine)
                .HasDefaultValue(false)
                .HasColumnName("isroutine");
            entity.Property(e => e.Notes).HasColumnName("notes");
            entity.Property(e => e.Routinename)
                .HasColumnType("character varying")
                .HasColumnName("routinename");
            entity.Property(e => e.Updatedby).HasColumnName("updatedby");
            entity.Property(e => e.Userid).HasColumnName("userid");
            entity.Property(e => e.Workouttype)
                .HasColumnType("character varying")
                .HasColumnName("workouttype");

            entity.HasOne(d => d.CreatedbyNavigation).WithMany(p => p.WorkoutCreatedbyNavigations)
                .HasForeignKey(d => d.Createdby)
                .HasConstraintName("workout_createdby_fkey");

            entity.HasOne(d => d.Createdfromroutine).WithMany(p => p.InverseCreatedfromroutine)
                .HasForeignKey(d => d.Createdfromroutineid)
                .HasConstraintName("workout_createdfromroutineid_fkey");

            entity.HasOne(d => d.UpdatedbyNavigation).WithMany(p => p.WorkoutUpdatedbyNavigations)
                .HasForeignKey(d => d.Updatedby)
                .HasConstraintName("workout_updatedby_fkey");

            entity.HasOne(d => d.User).WithMany(p => p.WorkoutUsers)
                .HasForeignKey(d => d.Userid)
                .HasConstraintName("workout_userid_fkey");
        });

        modelBuilder.Entity<Yogaworkoutdetail>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("yogaworkoutdetails_pkey");

            entity.ToTable("yogaworkoutdetails");

            entity.HasIndex(e => e.Workoutid, "yogaworkoutdetails_workoutid_key").IsUnique();

            entity.Property(e => e.Id)
                .HasDefaultValueSql("uuid_generate_v4()")
                .HasColumnName("id");
            entity.Property(e => e.Focusarea)
                .HasColumnType("character varying")
                .HasColumnName("focusarea");
            entity.Property(e => e.Intensity)
                .HasColumnType("character varying")
                .HasColumnName("intensity");
            entity.Property(e => e.Style)
                .HasColumnType("character varying")
                .HasColumnName("style");
            entity.Property(e => e.Workoutid).HasColumnName("workoutid");

            entity.HasOne(d => d.Workout).WithOne(p => p.Yogaworkoutdetail)
                .HasForeignKey<Yogaworkoutdetail>(d => d.Workoutid)
                .HasConstraintName("yogaworkoutdetails_workoutid_fkey");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
