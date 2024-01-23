using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace Ts.Blogging.Api.Server
{
    /// <summary>
    /// Manages standard Web request for user operations
    /// </summary>
    public class UserController : Controller
    {
        #region Private Members

        /// <summary>
        /// The singleton instance of <see cref="ILogger{TCategoryName}"/>
        /// </summary>
        private readonly ILogger<UserController> logger;

        /// <summary>
        /// The scoped instance of the <see cref="UserManager{TUser}"/>
        /// </summary>
        private readonly UserManager<ApplicationUser> userManager;

        #endregion

        #region Constructor

        /// <summary>
        /// Default constructor
        /// </summary>
        public UserController(ILogger<UserController> logger, UserManager<ApplicationUser> userManager)
        {
            this.logger = logger;
            this.userManager = userManager;
        }

        #endregion

        /// <summary>
        /// Handles user login based on specified login credentials
        /// </summary>
        /// <param name="loginCredentials">The specified login credentials</param>
        /// <returns>An instance of <see cref="ActionResult"/> for this operation</returns>
        [HttpPost(EndpointRoutes.Login)]
        public async Task<ActionResult> LoginAsync([FromBody] LoginCredentials loginCredentials)
        {
            try
            {
                // The error message when login fails
                var errorMessage = "Invalid username or password";

                // Set the error response for failed login
                var errorResponse = Problem(title: "UNAUTHORIZED",
                    statusCode: StatusCodes.Status401Unauthorized, detail: errorMessage);

                // If email was not specified...
                if (loginCredentials?.Email == null || string.IsNullOrWhiteSpace(loginCredentials.Email))
                    // Return error response to user
                    return errorResponse;

                // Validate if the user credentials are correct...

                // Get the user
                var user = await userManager.FindByEmailAsync(loginCredentials.Email);

                // If user could not be found...
                if (user == null)
                    // Return error response
                    return errorResponse;

                // Get if password is valid
                var isValidPassword = await userManager.CheckPasswordAsync(user, loginCredentials.Password);

                // If the password was wrong...
                if (!isValidPassword)
                    // Return error response to user
                    return errorResponse;

                // Get username
                var username = user.UserName;

                // Return response with token
                return Ok(new
                {
                    user.FirstName,
                    user.LastName,
                    user.Email,
                    user.UserName,
                    Token = user.GenerateJwtToken()
                });
            }
            catch (Exception ex)
            {
                // Log the error
                logger.LogError(ex.Message);

                // Return error response
                return Problem(title: "SYSTEM ERROR",
                    statusCode: StatusCodes.Status500InternalServerError, detail: ex.Message);
            }
        }
    }
}
