using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Parser.BL;
using Parser.BL.Entities;

namespace Parser.ConsoleApp
{
    public sealed class Storage : DbContext
    {
        public DbSet<OrganizationEntity> Organizations { get; set; }
        public DbSet<ContactEntity> Contacts { get; set; }
        private static bool MigrationWasChecked { get; set; }

        public Storage(DbContextOptions<Storage> options)
            : base(options)
        {
            
        }

        public Storage()
        {
            if (!MigrationWasChecked)
            {
                CultureInfo.DefaultThreadCurrentCulture = new CultureInfo("en-US");
                CultureInfo.DefaultThreadCurrentUICulture = new CultureInfo("en-US");
                MigrationWasChecked = true;
            }
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                optionsBuilder.UseSqlServer(@"Server=.\; Database=Parser; Initial Catalog=Parser;Integrated Security=False;Trusted_Connection=True;");
            }
            base.OnConfiguring(optionsBuilder);
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            DeletionPolicy(modelBuilder);

            base.OnModelCreating(modelBuilder);
        }
        

        private static void DeletionPolicy(ModelBuilder modelBuilder)
        {
            foreach (var relationship in modelBuilder.Model.GetEntityTypes().SelectMany(e => e.GetForeignKeys()))
            {
                relationship.DeleteBehavior = DeleteBehavior.Restrict;
            }
        }

        public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = new CancellationToken())
        {
            var now = DateTime.UtcNow;

            foreach (var entity in ChangeTracker.Entries())
            {
                if (entity.Entity is IEntity entityWithTrackedDates)
                {
                    switch (entity.State)
                    {
                        case EntityState.Added:
                            entityWithTrackedDates.CreatedDateUtc = now;
                            entityWithTrackedDates.LastUpdatedDateUtc = now;
                            break;
                        case EntityState.Modified:
                            entityWithTrackedDates.LastUpdatedDateUtc = now;
                            break;
                    }
                }

                if (entity is IEntity entityWithGuidId)
                {
                    if (entity.State == EntityState.Added && entityWithGuidId.Id == Guid.Empty)
                    {
                        entityWithGuidId.Id = Guid.NewGuid();
                    }
                }
            }

            return base.SaveChangesAsync(cancellationToken);
        }
    }
}
