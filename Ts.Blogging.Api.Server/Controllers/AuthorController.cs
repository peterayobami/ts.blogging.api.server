using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace Ts.Blogging.Api.Server
{
    /// <summary>
    /// Manages author management standard Web API transactions
    /// </summary>
    public class AuthorController : ControllerBase
    {
        #region Private Members

        /// <summary>
        /// The singleton instance of the <see cref="ILogger{TCategoryName}"/>
        /// </summary>
        private readonly ILogger<AuthorManagement> logger;

        /// <summary>
        /// The scoped instance of the <see cref="AuthorManagement"/>
        /// </summary>
        private readonly AuthorManagement authorManagement;

        /// <summary>
        /// The scoped instance of the <see cref="UserManager{TUser}"/>
        /// </summary>
        private readonly UserManager<ApplicationUser> userManager;

        #endregion

        #region Constructor

        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="logger">The injected <see cref="ILogger"/></param>
        /// <param name="AuthorManagement">The injected <see cref="AuthorManagement"/></param>
        public AuthorController(ILogger<AuthorManagement> logger,
            AuthorManagement authorManagement, UserManager<ApplicationUser> userManager)
        {
            this.logger = logger;
            this.authorManagement = authorManagement;
            this.userManager = userManager;
        }

        #endregion

        /// <summary>
        /// API endpoint to create author based on specified credentials
        /// </summary>
        /// <param name="credentials">The specified author credentials</param>
        [HttpPost(EndpointRoutes.RegisterAuthor)]
        public async Task<ActionResult> RegisterAuthorAsync([FromBody] AuthorCredentials credentials)
        {
            try
            {
                // Invoke the transaction
                var transaction = await authorManagement.RegisterAsync(credentials);

                // If transaction failed...
                if (!transaction.Successful)
                {
                    // Return error response
                    return Problem(title: transaction.ErrorTitle,
                        statusCode: transaction.StatusCode, detail: transaction.ErrorMessage);
                }

                // Return response
                return Created(string.Empty, transaction.Result);
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

        /// <summary>
        /// API endpoint to retrieve authors
        /// </summary>
        [AuthorizeAdmin]
        [HttpGet(EndpointRoutes.FetchAuthors)]
        public async Task<ActionResult> FetchAuthorsAsync()
        {
            try
            {
                // Invoke the transaction
                var transaction = await authorManagement.FetchAsync();

                // If transaction failed...
                if (!transaction.Successful)
                {
                    // Return error response
                    return Problem(title: transaction.ErrorTitle,
                        statusCode: transaction.StatusCode, detail: transaction.ErrorMessage);
                }

                // Return response
                return Ok(transaction.Result);
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

        /// <summary>
        /// API endpoint to retrieve specified author
        /// </summary>
        [AuthorizeAdmin]
        [HttpGet(EndpointRoutes.GetAuthor)]
        public async Task<ActionResult> FetchAuthorAsync(string id)
        {
            try
            {
                // Invoke the transaction
                var transaction = await authorManagement.FetchAsync(id);

                // If transaction failed...
                if (!transaction.Successful)
                {
                    // Return error response
                    return Problem(title: transaction.ErrorTitle,
                        statusCode: transaction.StatusCode, detail: transaction.ErrorMessage);
                }

                // Return response
                return Ok(transaction.Result);
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

        /// <summary>
        /// API endpoint to retrieve authorized author
        /// </summary>
        [AuthorizeAuthor]
        [HttpGet(EndpointRoutes.FetchAuthor)]
        public async Task<ActionResult> FetchAuthorAsync()
        {
            try
            {
                // Get the user
                var user = await userManager.GetUserAsync(HttpContext.User);

                // Invoke the transaction
                var transaction = await authorManagement.FetchAsync(user);

                // If transaction failed...
                if (!transaction.Successful)
                {
                    // Return error response
                    return Problem(title: transaction.ErrorTitle,
                        statusCode: transaction.StatusCode, detail: transaction.ErrorMessage);
                }

                // Return response
                return Ok(transaction.Result);
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

        /// <summary>
        /// API endpoint to update specified author
        /// </summary>
        [AuthorizeAuthor]
        [HttpPut(EndpointRoutes.UpdateAuthor)]
        public async Task<ActionResult> UpdateAuthorAsync([FromBody] UpdateAuthorApiModel credentials)
        {
            try
            {
                // Get the user
                var user = await userManager.GetUserAsync(HttpContext.User);

                // Invoke the transaction
                var transaction = await authorManagement.UpdateAsync(user, credentials);

                // If transaction failed...
                if (!transaction.Successful)
                {
                    // Return error response
                    return Problem(title: transaction.ErrorTitle,
                        statusCode: transaction.StatusCode, detail: transaction.ErrorMessage);
                }

                // Return response
                return Ok(transaction.Result);
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

        /// <summary>
        /// API endpoint to detete specified author
        /// </summary>
        [AuthorizeAdmin]
        [HttpDelete(EndpointRoutes.DeleteAuthor)]
        public async Task<ActionResult> DeleteAuthorAsync(string id)
        {
            try
            {
                // Invoke the transaction
                var transaction = await authorManagement.DeleteAsync(id);

                // If transaction failed...
                if (!transaction.Successful)
                {
                    // Return error response
                    return Problem(title: transaction.ErrorTitle,
                        statusCode: transaction.StatusCode, detail: transaction.ErrorMessage);
                }

                // Return response
                return Ok(transaction.Result);
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