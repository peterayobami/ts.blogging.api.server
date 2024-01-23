using Microsoft.EntityFrameworkCore;

namespace Ts.Blogging.Api.Server
{
    /// <summary>
    /// The database representational model for the application
    /// </summary>
    public class ApplicationDbContext : DbContext
    {
        #region Constructor

        /// <summary>
        /// Default constructor
        /// </summary>
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {

        }

        #endregion

        #region DbSets

        /// <summary>
        /// The tags database table
        /// </summary>
        public DbSet<TagDataModel> Tags { get; set; }

        /// <summary>
        /// The authors database table
        /// </summary>
        public DbSet<AuthorDataModel> Authors { get; set; }

        /// <summary>
        /// The articles database table
        /// </summary>
        public DbSet<ArticleDataModel> Articles { get; set; }

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
