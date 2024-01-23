using Microsoft.AspNetCore.Identity;

namespace Ts.Blogging.Api.Server
{
    /// <summary>
    /// Manages user account operations
    /// </summary>
    public class AccountManagement
    {
        #region Private Members

        /// <summary>
        /// The scoped instance of the <see cref="ApplicationDbContext"/>
        /// </summary>
        private readonly ApplicationDbContext context;

        /// <summary>
        /// The scoped instance of the <see cref="UserManager{TUser}"/>
        /// </summary>
        private readonly UserManager<ApplicationUser> userManager;

        #endregion

        #region Constructor

        /// <summary>
        /// Default constructor
        /// </summary>
        public AccountManagement(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            this.context = context;
            this.userManager = userManager;
        }

        #endregion

        public async Task<OperationResult> CreateAuthorAsync(AuthorCredentials authorCredentials)
        {
            return await Task.FromResult(new OperationResult { });
        }
    }
}
