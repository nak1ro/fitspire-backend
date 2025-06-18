using System;
using System.Collections.Generic;

namespace backend.Modules.ScaffoldedModels;

public partial class User
{
    public Guid Id { get; set; }

    public string Username { get; set; } = null!;

    public string Email { get; set; } = null!;

    public string Passwordhash { get; set; } = null!;

    public string Displayname { get; set; } = null!;

    public string? Bio { get; set; }

    public string? Profilepictureurl { get; set; }

    public bool? Isprivate { get; set; }

    public DateTime? Createdat { get; set; }

    public DateTime? Updatedat { get; set; }

    public virtual ICollection<Ban> Bans { get; set; } = new List<Ban>();

    public virtual ICollection<Challengeparticipant> Challengeparticipants { get; set; } = new List<Challengeparticipant>();

    public virtual ICollection<Challenge> Challenges { get; set; } = new List<Challenge>();

    public virtual ICollection<Comment> Comments { get; set; } = new List<Comment>();

    public virtual ICollection<Follower> FollowerFolloweds { get; set; } = new List<Follower>();

    public virtual ICollection<Follower> FollowerFollowerNavigations { get; set; } = new List<Follower>();

    public virtual ICollection<Friendship> FriendshipUser1s { get; set; } = new List<Friendship>();

    public virtual ICollection<Friendship> FriendshipUser2s { get; set; } = new List<Friendship>();

    public virtual ICollection<Friendshiprequest> FriendshiprequestAddressees { get; set; } = new List<Friendshiprequest>();

    public virtual ICollection<Friendshiprequest> FriendshiprequestRequesters { get; set; } = new List<Friendshiprequest>();

    public virtual ICollection<Goal> Goals { get; set; } = new List<Goal>();

    public virtual ICollection<Groupmember> Groupmembers { get; set; } = new List<Groupmember>();

    public virtual ICollection<Group> Groups { get; set; } = new List<Group>();

    public virtual ICollection<Like> Likes { get; set; } = new List<Like>();

    public virtual ICollection<Meal> Meals { get; set; } = new List<Meal>();

    public virtual ICollection<Notification> Notifications { get; set; } = new List<Notification>();

    public virtual ICollection<Personalrecordhistory> Personalrecordhistories { get; set; } = new List<Personalrecordhistory>();

    public virtual ICollection<Personalrecord> Personalrecords { get; set; } = new List<Personalrecord>();

    public virtual ICollection<Post> PostCreatedbyNavigations { get; set; } = new List<Post>();

    public virtual ICollection<Post> PostUpdatedbyNavigations { get; set; } = new List<Post>();

    public virtual ICollection<Post> PostUsers { get; set; } = new List<Post>();

    public virtual ICollection<Progressentry> Progressentries { get; set; } = new List<Progressentry>();

    public virtual ICollection<Report> ReportReportedbyNavigations { get; set; } = new List<Report>();

    public virtual ICollection<Report> ReportReporteduserNavigations { get; set; } = new List<Report>();

    public virtual ICollection<Savedpost> Savedposts { get; set; } = new List<Savedpost>();

    public virtual ICollection<UserBadge> Userbadges { get; set; } = new List<UserBadge>();

    public virtual Userpreference? Userpreference { get; set; }

    public virtual ICollection<Workout> WorkoutCreatedbyNavigations { get; set; } = new List<Workout>();

    public virtual ICollection<Workout> WorkoutUpdatedbyNavigations { get; set; } = new List<Workout>();

    public virtual ICollection<Workout> WorkoutUsers { get; set; } = new List<Workout>();
}
