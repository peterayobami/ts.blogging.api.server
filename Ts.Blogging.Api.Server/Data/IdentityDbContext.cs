using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Ts.Blogging.Api.Server
{
    public class IdentityDbContext : IdentityDbContext<ApplicationUser>
    {
        #region Constructor

        /// <summary>
        /// Default constructor
        /// </summary>
        public IdentityDbContext(DbContextOptions<IdentityDbContext> options) : base(options)
        {

        }

        #endregion

        public override int SaveChanges()
        {
            // For each entry...
            foreach (var entry in ChangeTracker.Entries<BaseDataModel>())
            {
                // If data is being created...
                if (entry.State == EntityState.Added)
                {
                    // Set date created
                    entry.Entity.DateCreated = DateTime.UtcNow;

                    // Set date modified
                    entry.Entity.DateModified = DateTime.UtcNow;
                }

                // If data is being modified...
                if (entry.State == EntityState.Modified)
                {
                    // Set date modified
                    entry.Entity.DateModified = DateTime.UtcNow;
                }
            }

            return base.SaveChanges();
        }

        public async override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            // For each entry...
            foreach (var entry in ChangeTracker.Entries<BaseDataModel>())
            {
                // If data is being created...
                if (entry.State == EntityState.Added)
                {
                    // Set date created
                    entry.Entity.DateCreated = DateTime.UtcNow;

                    // Set date modified
                    entry.Entity.DateModified = DateTime.UtcNow;
                }

                // If data is being modified...
                if (entry.State == EntityState.Modified)
                {
                    // Set date modified
                    entry.Entity.DateModified = DateTime.UtcNow;
                }
            }

            return await base.SaveChangesAsync(cancellationToken);
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
        }
    }
}
