using Microsoft.AspNetCore.Identity;

namespace Ts.Blogging.Api.Server
{
    /// <summary>
    /// The application user data model
    /// </summary>
    public class ApplicationUser : IdentityUser
    {
        /// <summary>
        /// The first name of the user
        /// </summary>
        public string FirstName { get; set; }

        /// <summary>
        /// The last name of the user
        /// </summary>
        public string LastName { get; set; }

        /// <summary>
        /// The scope of the user
        /// </summary>
        public string Scope { get; set; }
    }
}
