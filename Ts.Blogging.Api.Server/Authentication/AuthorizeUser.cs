using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;

namespace Ts.Blogging.Api.Server
{
    /// <summary>
    /// The authorization policy for token-based authentication
    /// </summary>
    public class AuthorizeUser : AuthorizeAttribute
    {
        #region Constructor

        /// <summary>
        /// Default constructor
        /// </summary>
        public AuthorizeUser()
        {
            // Add the JWT bearer authentication scheme
            AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme;
        }

        #endregion
    }
}
