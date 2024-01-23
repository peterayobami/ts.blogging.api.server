using Microsoft.AspNetCore.Mvc;

namespace Ts.Blogging.Api.Server
{
    /// <summary>
    /// Manages Web requests for Administrators' operations
    /// </summary>
    [AuthorizeAdmin]
    public class AdminController : ControllerBase
    {
        #region Private Members

        /// <summary>
        /// The scoped instance of <see cref="AuthorManagement"/>
        /// </summary>
        private readonly AuthorManagement authorManagement;

        /// <summary>
        /// The singleton instance of the <see cref="ILogger{TCategoryName}"/>
        /// </summary>
        private readonly ILogger<AdminController> logger;

        /// <summary>
        /// The scoped instance of the <see cref="ArticleManagement"/>
        /// </summary>
        private readonly ArticleManagement articleManagement;

        #endregion

        #region Constructor

        /// <summary>
        /// Default constructor
        /// </summary>
        public AdminController(AuthorManagement authorManagement, ILogger<AdminController> logger, ArticleManagement articleManagement)
        {
            this.authorManagement = authorManagement;
            this.logger = logger;
            this.articleManagement = articleManagement;
        }

        #endregion

        /// <summary>
        /// API endpoint to detete specified article
        /// </summary>
        [HttpDelete(EndpointRoutes.AdminDeleteArticle)]
        public async Task<ActionResult> DeleteArticleAsync(string id)
        {
            try
            {
                // Invoke the transaction
                var transaction = await articleManagement.DeleteAsync(id);

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
        /// Updates author approval status based on input
        /// </summary>
        /// <param name="model">The approval model</param>
        /// <param name="id">The id of the author</param>
        /// <returns></returns>
        [HttpPut(EndpointRoutes.UpdateAuthorApprovalStatus)]
        public async Task<ActionResult> ApproveAuthorAsync([FromBody] ApprovalApiModel model, string id)
        {
            try
            {
                // Trigger the operation
                var operation = await authorManagement.UpdateStatusAsync(model, id);

                // If operation failed...
                if (!operation.Successful)
                {
                    // Return error response
                    return Problem(title: operation.ErrorTitle,
                        statusCode: operation.StatusCode, detail: operation.ErrorMessage);
                }

                // Return response
                return NoContent();
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
