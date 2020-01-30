using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace AECOM.GNGApi.Models
{
    public partial class GoNoGoContext : DbContext
    {
        public GoNoGoContext(DbContextOptions<GoNoGoContext> options)
            : base(options)
        {
        }

        public virtual DbSet<GNGStatus> GNGStatus { get; set; }
        public virtual DbSet<GoNoGo> GoNoGo { get; set; }
        public virtual DbSet<GoNoGoPost> GoNoGoPost { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<GNGStatus>(entity =>
            {
                entity.ToTable("GNGStatus");

                entity.Property(e => e.GngstatusId)
                    .HasColumnName("GNGStatusId")
                    .ValueGeneratedNever();

                entity.Property(e => e.GngstatusValue)
                    .IsRequired()
                    .HasColumnName("GNGStatusValue")
                    .HasMaxLength(10);
            });

            modelBuilder.Entity<GoNoGo>(entity =>
            {
                entity.Property(e => e.CreatedBy)
                    .IsRequired()
                    .HasMaxLength(64);

                entity.Property(e => e.CreatedOn).HasColumnType("datetime");

                entity.Property(e => e.FormData).IsRequired();

                entity.Property(e => e.GngstatusId).HasColumnName("GNGStatusId");

                entity.Property(e => e.LastModifiedBy).HasMaxLength(64);

                entity.Property(e => e.LastModifiedOn).HasColumnType("datetime");

                entity.Property(e => e.OpportunityId)
                    .IsRequired()
                    .HasMaxLength(10);

                entity.HasOne(d => d.Gngstatus)
                    .WithMany(p => p.GoNoGo)
                    .HasForeignKey(d => d.GngstatusId)
                    .HasConstraintName("FK__GoNoGo__GNGStatu__5165187F");
            });

            /// Manual Definition for GoNoGoSP
            /// 

            modelBuilder.Entity<GoNoGoPost>(entity =>
            {
                entity.Property(e => e.CreatedBy)                 
                    .HasMaxLength(64);

                entity.Property(e => e.CreatedOn).HasColumnType("datetime");

                entity.Property(e => e.FormData).IsRequired();

                entity.Property(e => e.GngstatusId).HasColumnName("GNGStatusId");

                entity.Property(e => e.LastModifiedBy).HasMaxLength(64);

                entity.Property(e => e.LastModifiedOn).HasColumnType("datetime");

                entity.Property(e => e.OpportunityId).HasMaxLength(10);

                entity.Property(e => e.GngStatusValue).HasMaxLength(10);
                entity.Property(e => e.Id).HasColumnName("Id");
                    
            });

            OnModelCreatingPartial(modelBuilder);
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }
}
