using HRDirectAPI.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace HRDirectAPI.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
        
    }
    public DbSet<Employee> Employees { get; set; }

    public DbSet<Role> Roles { get; set; }

    public DbSet<EmployeeRoleAssignment> EmployeeRoleAssignments { get; set; }
    
    public DbSet<AccountToken> AccountTokens { get; set; }
    
    public DbSet<AuditLog> AuditLogs { get; set; }
    
    public DbSet<TrainingRequest> TrainingRequests { get; set; }

    public DbSet<TrainingAllowanceVersion> TrainingAllowanceVersions { get; set; }

    public DbSet<EstaCodeRate> EstacodeRates { get; set; }
    
    public DbSet<Course> Courses { get; set; }

    public DbSet<Lesson> Lessons { get; set; }

    public DbSet<QuizQuestion> QuizQuestions { get; set; }

    public DbSet<QuizOption> QuizOptions { get; set; }

    public DbSet<LearningAssignment> LearningAssignments { get; set; }

    public DbSet<LessonCompletion> LessonCompletions { get; set; }

    public DbSet<AssessmentAttempt> AssessmentAttempts { get; set; }

    public DbSet<AssignmentResetHistory> AssignmentResetHistories { get; set; }
    
    public DbSet<Notification> Notifications { get; set; }
    
    public DbSet<EmailQueue> EmailQueues { get; set; }
    
    public DbSet<KpiPlan> KpiPlans { get; set; }

    public DbSet<KpiItem> KpiItems { get; set; }

    public DbSet<KpiReviewHistory> KpiReviewHistories { get; set; }

    public DbSet<KpiPlanReopenHistory> KpiPlanReopenHistories { get; set; }
    
    public DbSet<Payslip> Payslips { get; set; }

    public DbSet<PayslipItem> PayslipItems { get; set; }
    
    public DbSet<InternalOpportunity> InternalOpportunities { get; set; }

    public DbSet<OpportunityApplication> OpportunityApplications { get; set; }

    public DbSet<OpportunityApplicationHistory> OpportunityApplicationHistories { get; set; }
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<EmployeeRoleAssignment>()
            .HasKey(x => new { x.EmployeeId, x.RoleId });

        modelBuilder.Entity<EmployeeRoleAssignment>()
            .HasOne(x => x.Employee)
            .WithMany(x => x.EmployeeRoleAssignments)
            .HasForeignKey(x => x.EmployeeId);

        modelBuilder.Entity<EmployeeRoleAssignment>()
            .HasOne(x => x.Role)
            .WithMany(x => x.EmployeeRoleAssignments)
            .HasForeignKey(x => x.RoleId);
        
        modelBuilder.Entity<AuditLog>()
            .HasOne(a => a.Employee)
            .WithMany()
            .HasForeignKey(a => a.EmployeeId)
            .OnDelete(DeleteBehavior.SetNull);

        modelBuilder.Entity<Employee>()
            .HasIndex(x => x.EmployeeNumber)
            .IsUnique();

        modelBuilder.Entity<Employee>()
            .HasIndex(x => x.Email)
            .IsUnique();

        modelBuilder.Entity<Role>()
            .HasIndex(x => x.Name)
            .IsUnique();
        
        modelBuilder.Entity<AccountToken>()
            .HasOne(x => x.Employee)
            .WithMany(x => x.AccountTokens)
            .HasForeignKey(x => x.EmployeeId)
            .OnDelete(DeleteBehavior.Cascade);
        
        modelBuilder.Entity<Role>().HasData(
            new Role { Id = 1, Name = "Staff" },
            new Role { Id = 2, Name = "Manager" },
            new Role { Id = 3, Name = "HR" },
            new Role { Id = 4, Name = "Finance" },
            new Role { Id = 5, Name = "Admin" }
        );
        
        modelBuilder.Entity<TrainingRequest>(entity =>
        {
            entity.Property(x => x.CourseFee)
                .HasPrecision(18, 2);

            entity.Property(x => x.EstimatedFlightCost)
                .HasPrecision(18, 2);

            entity.Property(x => x.Status)
                .HasConversion<string>();

            entity.HasOne(x => x.Employee)
                .WithMany()
                .HasForeignKey(x => x.EmployeeId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.AssignedManager)
                .WithMany()
                .HasForeignKey(x => x.AssignedManagerId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<TrainingAllowanceVersion>(entity =>
        {
            entity.Property(x => x.DailyRateUsd)
                .HasPrecision(18, 2);

            entity.Property(x => x.TotalAllowance)
                .HasPrecision(18, 2);

            entity.HasIndex(x => new
            {
                x.TrainingRequestId,
                x.VersionNumber
            }).IsUnique();

            entity.HasOne(x => x.TrainingRequest)
                .WithMany(x => x.AllowanceVersions)
                .HasForeignKey(x => x.TrainingRequestId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<EstaCodeRate>(entity =>
        {
            entity.Property(x => x.DailyRateUsd)
                .HasPrecision(18, 2);

            entity.HasIndex(x => new
            {
                x.CountryCode,
                x.Grade
            }).IsUnique();
        });
        
        modelBuilder.Entity<Course>()
            .Property(c => c.Status)
            .HasConversion<string>();

        modelBuilder.Entity<LearningAssignment>()
            .Property(a => a.Status)
            .HasConversion<string>();

        modelBuilder.Entity<Course>()
            .HasMany(c => c.Lessons)
            .WithOne(l => l.Course)
            .HasForeignKey(l => l.CourseId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Course>()
            .HasMany(c => c.Questions)
            .WithOne(q => q.Course)
            .HasForeignKey(q => q.CourseId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<QuizQuestion>()
            .HasMany(q => q.Options)
            .WithOne(o => o.QuizQuestion)
            .HasForeignKey(o => o.QuizQuestionId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<LearningAssignment>()
            .HasMany(a => a.LessonCompletions)
            .WithOne(l => l.LearningAssignment)
            .HasForeignKey(l => l.LearningAssignmentId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<LearningAssignment>()
            .HasMany(a => a.AssessmentAttempts)
            .WithOne(a => a.LearningAssignment)
            .HasForeignKey(a => a.LearningAssignmentId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<LearningAssignment>()
            .HasMany(a => a.ResetHistory)
            .WithOne(h => h.LearningAssignment)
            .HasForeignKey(h => h.LearningAssignmentId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<LessonCompletion>()
            .HasIndex(l => new { l.LearningAssignmentId, l.LessonId })
            .IsUnique();

        modelBuilder.Entity<LearningAssignment>()
            .HasIndex(a => new { a.EmployeeId, a.CourseId })
            .IsUnique();
        
        modelBuilder.Entity<InternalOpportunity>()
            .Property(o => o.AllowedDepartments)
            .HasConversion(
                departments => string.Join("|", departments),
                value => string.IsNullOrEmpty(value)
                    ? new List<string>()
                    : value.Split('|', StringSplitOptions.RemoveEmptyEntries).ToList());

        modelBuilder.Entity<InternalOpportunity>()
            .Property(o => o.AllowedGrades)
            .HasConversion(
                grades => string.Join("|", grades),
                value => string.IsNullOrEmpty(value)
                    ? new List<string>()
                    : value.Split('|', StringSplitOptions.RemoveEmptyEntries).ToList());

        modelBuilder.Entity<OpportunityApplication>()
            .HasIndex(a => new
            {
                a.InternalOpportunityId,
                a.EmployeeId
            })
            .IsUnique();

        modelBuilder.Entity<InternalOpportunity>()
            .HasMany(o => o.Applications)
            .WithOne(a => a.InternalOpportunity)
            .HasForeignKey(a => a.InternalOpportunityId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<OpportunityApplication>()
            .HasMany(a => a.History)
            .WithOne(h => h.OpportunityApplication)
            .HasForeignKey(h => h.OpportunityApplicationId)
            .OnDelete(DeleteBehavior.Cascade);
    }
    
}