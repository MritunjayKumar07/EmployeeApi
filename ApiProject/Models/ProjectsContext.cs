using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace ApiProject.Models;

public partial class ProjectsContext : DbContext
{
    public ProjectsContext()
    {
    }

    public ProjectsContext(DbContextOptions<ProjectsContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Address> Addresses { get; set; }

    public virtual DbSet<Employee> Employees { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseSqlServer("Data Source=DESKTOP-HOOMVQE\\MSSQLSERVER02; Initial Catalog=Projects; User Id=mk; Password=1234; TrustServerCertificate=True");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Address>(entity =>
        {
            entity.HasKey(e => e.Aid).HasName("PK__Addresse__DE508E2EB4ABC8F7");

            entity.Property(e => e.Aid).HasColumnName("aid");
            entity.Property(e => e.City)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("city");
            entity.Property(e => e.Eid).HasColumnName("eid");
            entity.Property(e => e.PinCode)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasColumnName("pinCode");
            entity.Property(e => e.State)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("state");
            entity.Property(e => e.Street)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasColumnName("street");

            entity.HasOne(d => d.EidNavigation).WithMany(p => p.Addresses)
                .HasForeignKey(d => d.Eid)
                .HasConstraintName("FK__Addresses__eid__3D5E1FD2");
        });

        modelBuilder.Entity<Employee>(entity =>
        {
            entity.HasKey(e => e.Eid).HasName("PK__Employee__D9509F6D2D166249");

            entity.Property(e => e.Eid).HasColumnName("eid");
            entity.Property(e => e.Name)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasColumnName("name");
            entity.Property(e => e.Position)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("position");
            entity.Property(e => e.Salary)
                .HasColumnType("decimal(18, 0)")
                .HasColumnName("salary");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
