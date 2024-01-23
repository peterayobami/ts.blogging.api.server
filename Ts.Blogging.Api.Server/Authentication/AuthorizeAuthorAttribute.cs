using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;

namespace Ts.Blogging.Api.Server
{
    /// <summary>
    /// The authorization policy for token-based authentication
    /// </summary>
    public class AuthorizeAuthorAttribute : AuthorizeAttribute
    {
        #region Constructor

        /// <summary>
        /// Default constructor
        /// </summary>
        public AuthorizeAuthorAttribute()
        {
            // Add the JWT bearer authentication scheme
            AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme;

            // Authorization policy is author
            Policy = AuthorizationPolicies.Author;
        }

        #endregion
    }
}
