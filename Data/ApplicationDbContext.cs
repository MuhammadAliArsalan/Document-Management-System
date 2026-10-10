using Microsoft.EntityFrameworkCore;
using DocumentSearch.Models;

namespace DocumentSearch.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<Employee> Employees { get; set; }
    public DbSet<Document> Documents { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Employee>()
            .ToTable("T_Employees");

        modelBuilder.Entity<Document>()
            .ToTable("T_Documents");

        modelBuilder.Entity<Document>()
            .HasOne(d => d.UploadedByEmployee)
            .WithMany(e => e.Documents)
            .HasForeignKey(d => d.UploadedByEmployeeId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Employee>()
        .HasIndex(e => e.EmployeeCode)
        .IsUnique();

        modelBuilder.Entity<Employee>()
            .Property(e => e.IsAdmin)
            .HasDefaultValue(false);
    }
}
