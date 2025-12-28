using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using EllisHope.Models.Domain;

namespace EllisHope.Data;

public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    // Domain tables
    public DbSet<Page> Pages { get; set; }
    public DbSet<ContentSection> ContentSections { get; set; }
    public DbSet<Media> MediaLibrary { get; set; }
    public DbSet<MediaUsage> MediaUsages { get; set; }
    public DbSet<ImageSize> ImageSizes { get; set; }
    public DbSet<PageImage> PageImages { get; set; }
    public DbSet<BlogPost> BlogPosts { get; set; }
    public DbSet<BlogCategory> BlogCategories { get; set; }
    public DbSet<BlogPostCategory> BlogPostCategories { get; set; }
    public DbSet<Event> Events { get; set; }
    public DbSet<Cause> Causes { get; set; }
    
    // Application System tables
    public DbSet<ClientApplication> ClientApplications { get; set; }
    public DbSet<ApplicationVote> ApplicationVotes { get; set; }
    public DbSet<ApplicationComment> ApplicationComments { get; set; }
    public DbSet<ApplicationNotification> ApplicationNotifications { get; set; }

    // System tables
    public DbSet<ApplicationLog> ApplicationLogs { get; set; }

    // Newsletter tables
    public DbSet<Subscriber> Subscribers { get; set; }
    public DbSet<Newsletter> Newsletters { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // Configure ApplicationUser self-referencing relationship (Sponsor -> Clients)
        builder.Entity<ApplicationUser>()
            .HasOne(u => u.Sponsor)
            .WithMany(u => u.SponsoredClients)
            .HasForeignKey(u => u.SponsorId)
            .OnDelete(DeleteBehavior.Restrict); // Prevent cascade delete

        builder.Entity<ApplicationUser>()
            .Property(u => u.MonthlyFee)
            .HasPrecision(18, 2);

        builder.Entity<ApplicationUser>()
            .HasIndex(u => u.UserRole);

        builder.Entity<ApplicationUser>()
            .HasIndex(u => u.Status);

        builder.Entity<ApplicationUser>()
            .HasIndex(u => u.IsActive);

        // Configure composite key for BlogPostCategories
        builder.Entity<BlogPostCategory>()
            .HasKey(bc => new { bc.BlogPostId, bc.CategoryId });

        builder.Entity<BlogPostCategory>()
            .HasOne(bc => bc.BlogPost)
            .WithMany(b => b.BlogPostCategories)
            .HasForeignKey(bc => bc.BlogPostId);

        builder.Entity<BlogPostCategory>()
            .HasOne(bc => bc.BlogCategory)
            .WithMany(c => c.BlogPostCategories)
            .HasForeignKey(bc => bc.CategoryId);

        // Media relationships
        builder.Entity<MediaUsage>()
            .HasOne(mu => mu.Media)
            .WithMany(m => m.Usages)
            .HasForeignKey(mu => mu.MediaId)
            .OnDelete(DeleteBehavior.Cascade);

        // Configure indexes for better performance
        builder.Entity<BlogPost>()
            .HasIndex(b => b.Slug)
            .IsUnique();

        builder.Entity<Event>()
            .HasIndex(e => e.Slug)
            .IsUnique();

        builder.Entity<Event>()
            .HasIndex(e => e.EventDate);

        builder.Entity<Media>()
            .HasIndex(m => m.Source);

        builder.Entity<Media>()
            .HasIndex(m => m.Category);

        builder.Entity<MediaUsage>()
            .HasIndex(mu => new { mu.EntityType, mu.EntityId });

        // Cause indexes and configuration
        builder.Entity<Cause>()
            .Property(c => c.GoalAmount)
            .HasPrecision(18, 2);

        builder.Entity<Cause>()
            .Property(c => c.RaisedAmount)
            .HasPrecision(18, 2);

        builder.Entity<Cause>()
            .HasIndex(c => c.Slug)
            .IsUnique();

        builder.Entity<Cause>()
            .HasIndex(c => c.Category);

        builder.Entity<Cause>()
            .HasIndex(c => c.IsPublished);

        #region Client Application Configuration
        
        // ClientApplication relationships
        builder.Entity<ClientApplication>()
            .HasOne(ca => ca.Applicant)
            .WithMany()
            .HasForeignKey(ca => ca.ApplicantId)
            .OnDelete(DeleteBehavior.Restrict);
        
        builder.Entity<ClientApplication>()
            .HasOne(ca => ca.AssignedSponsor)
            .WithMany()
            .HasForeignKey(ca => ca.AssignedSponsorId)
            .OnDelete(DeleteBehavior.SetNull);
        
        builder.Entity<ClientApplication>()
            .HasOne(ca => ca.DecisionMadeBy)
            .WithMany()
            .HasForeignKey(ca => ca.DecisionMadeById)
            .OnDelete(DeleteBehavior.Restrict);
        
        builder.Entity<ClientApplication>()
            .HasOne(ca => ca.LastModifiedBy)
            .WithMany()
            .HasForeignKey(ca => ca.LastModifiedById)
            .OnDelete(DeleteBehavior.Restrict);
        
        // Decimal precision
        builder.Entity<ClientApplication>()
            .Property(ca => ca.EstimatedMonthlyCost)
            .HasPrecision(10, 2);
        
        builder.Entity<ClientApplication>()
            .Property(ca => ca.ApprovedMonthlyAmount)
            .HasPrecision(10, 2);
        
        // Indexes for performance
        builder.Entity<ClientApplication>()
            .HasIndex(ca => ca.ApplicantId);
        
        builder.Entity<ClientApplication>()
            .HasIndex(ca => ca.Status);
        
        builder.Entity<ClientApplication>()
            .HasIndex(ca => ca.SubmittedDate);
        
        builder.Entity<ClientApplication>()
            .HasIndex(ca => new { ca.Status, ca.SubmittedDate });
        
        #endregion
        
        #region ApplicationVote Configuration
        
        builder.Entity<ApplicationVote>()
            .HasOne(av => av.Application)
            .WithMany(ca => ca.Votes)
            .HasForeignKey(av => av.ApplicationId)
            .OnDelete(DeleteBehavior.Cascade);
        
        builder.Entity<ApplicationVote>()
            .HasOne(av => av.Voter)
            .WithMany()
            .HasForeignKey(av => av.VoterId)
            .OnDelete(DeleteBehavior.Restrict);
        
        // Ensure one vote per board member per application
        builder.Entity<ApplicationVote>()
            .HasIndex(av => new { av.ApplicationId, av.VoterId })
            .IsUnique();
        
        builder.Entity<ApplicationVote>()
            .HasIndex(av => av.VotedDate);
        
        #endregion
        
        #region ApplicationComment Configuration
        
        builder.Entity<ApplicationComment>()
            .HasOne(ac => ac.Application)
            .WithMany(ca => ca.Comments)
            .HasForeignKey(ac => ac.ApplicationId)
            .OnDelete(DeleteBehavior.Cascade);
        
        builder.Entity<ApplicationComment>()
            .HasOne(ac => ac.Author)
            .WithMany()
            .HasForeignKey(ac => ac.AuthorId)
            .OnDelete(DeleteBehavior.Restrict);
        
        builder.Entity<ApplicationComment>()
            .HasOne(ac => ac.ParentComment)
            .WithMany(ac => ac.Replies)
            .HasForeignKey(ac => ac.ParentCommentId)
            .OnDelete(DeleteBehavior.Restrict);
        
        builder.Entity<ApplicationComment>()
            .HasIndex(ac => ac.CreatedDate);
        
        builder.Entity<ApplicationComment>()
            .HasIndex(ac => ac.IsPrivate);
        
        #endregion
        
        #region ApplicationNotification Configuration
        
        builder.Entity<ApplicationNotification>()
            .HasOne(an => an.Recipient)
            .WithMany()
            .HasForeignKey(an => an.RecipientId)
            .OnDelete(DeleteBehavior.Cascade);
        
        builder.Entity<ApplicationNotification>()
            .HasOne(an => an.Application)
            .WithMany()
            .HasForeignKey(an => an.ApplicationId)
            .OnDelete(DeleteBehavior.SetNull);
        
        builder.Entity<ApplicationNotification>()
            .HasIndex(an => new { an.RecipientId, an.IsRead });
        
        builder.Entity<ApplicationNotification>()
            .HasIndex(an => an.CreatedDate);
        
        builder.Entity<ApplicationNotification>()
            .HasIndex(an => an.Type);

        #endregion

        #region ApplicationLog Configuration

        builder.Entity<ApplicationLog>()
            .HasIndex(al => al.Level);

        builder.Entity<ApplicationLog>()
            .HasIndex(al => al.CreatedAt);

        builder.Entity<ApplicationLog>()
            .HasIndex(al => al.CorrelationId);

        builder.Entity<ApplicationLog>()
            .HasIndex(al => al.IsReviewed);

        builder.Entity<ApplicationLog>()
            .HasIndex(al => new { al.Level, al.CreatedAt });

        builder.Entity<ApplicationLog>()
            .Property(al => al.Message)
            .HasMaxLength(4000);

        builder.Entity<ApplicationLog>()
            .Property(al => al.Category)
            .HasMaxLength(500);

        builder.Entity<ApplicationLog>()
            .Property(al => al.RequestPath)
            .HasMaxLength(2000);

        builder.Entity<ApplicationLog>()
            .Property(al => al.ExceptionType)
            .HasMaxLength(500);

        builder.Entity<ApplicationLog>()
            .Property(al => al.CorrelationId)
            .HasMaxLength(50);

        #endregion

        #region Newsletter Configuration

        // Subscriber configuration
        builder.Entity<Subscriber>()
            .HasIndex(s => s.Email)
            .IsUnique();

        builder.Entity<Subscriber>()
            .HasIndex(s => s.UnsubscribeToken)
            .IsUnique();

        builder.Entity<Subscriber>()
            .HasIndex(s => s.SubscribedAt);

        // Newsletter configuration
        builder.Entity<Newsletter>()
            .HasOne(n => n.SentBy)
            .WithMany()
            .HasForeignKey(n => n.SentByUserId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.Entity<Newsletter>()
            .HasIndex(n => n.SentAt);

        builder.Entity<Newsletter>()
            .HasIndex(n => n.CreatedAt);

        #endregion
    }
}
