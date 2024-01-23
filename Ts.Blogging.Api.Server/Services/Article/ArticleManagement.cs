using System.Text.Json;
using Microsoft.EntityFrameworkCore;

namespace Ts.Blogging.Api.Server
{
    public class ArticleManagement
    {
        #region Private Members

        /// <summary>
        /// The scoped instance of the <see cref="ApplicationDbContext"/>
        /// </summary>
        private readonly ApplicationDbContext context;

        /// <summary>
        /// The singleton instance of the <see cref="ILogger{CategoryName}"/>
        /// </summary>
        private ILogger<ArticleManagement> logger;

        /// <summary>
        /// The scoped instance of the <see cref="CloudinaryService"/>
        /// </summary>
        private readonly CloudinaryService cloudinaryService;

        #endregion

        #region Constructor

        public ArticleManagement(ApplicationDbContext context,
            CloudinaryService cloudinaryService,
            ILogger<ArticleManagement> logger)
        {
            this.context = context;
            this.cloudinaryService = cloudinaryService;
            this.logger = logger;
        }

        #endregion

        /// <summary>
        /// Creates article based on provided credentials
        /// </summary>
        /// <param name="credentials">The provided article credentials</param>
        /// <returns></returns>
        public async Task<OperationResult> CreateAsync(ArticleCredentials credentials, string userId)
        {
            try
            {
                // Fetch the author
                var author = await context.Authors.FirstOrDefaultAsync(a => a.UserId == userId);

                // If author id does not exist...
                if (author == null)
                {
                    // Log error
                    logger.LogError("Author could not be found");

                    // Return error result
                    return new OperationResult
                    {
                        ErrorTitle = "BAD REQUEST",
                        StatusCode = StatusCodes.Status400BadRequest,
                        ErrorMessage = "Author could not be found"
                    };
                }

                // If author is not approved...
                if (author.Status != AuthorStatus.Approved)
                {
                    // Log the error
                    logger.LogError("This author is not approved and therefore cannot post an article");

                    // Return error result
                    return new OperationResult
                    {
                        ErrorTitle = "UNAUTHORIZED",
                        StatusCode = StatusCodes.Status401Unauthorized,
                        ErrorMessage = "This author is not approved and therecfore cannot post an article"
                    };
                }
                
                // Upload the caption
                var uploadResult = await cloudinaryService
                    .UploadAsync(credentials.Caption, uploadPreset: CloudinaryUploadPresets.ArticleCaption);

                // If upload failed...
                if (!uploadResult.Successful)
                {
                    // Log the error
                    logger.LogError(uploadResult.ErrorMessage + "\n" + JsonSerializer.Serialize(uploadResult.Errors));

                    // Return error result
                    return new OperationResult
                    {
                        ErrorTitle = "SYSTEM ERROR",
                        StatusCode = StatusCodes.Status500InternalServerError,
                        ErrorMessage = uploadResult.ErrorMessage + "\n" + JsonSerializer.Serialize(uploadResult.Errors)
                    };
                }

                // Set the article credentials
                var article = new ArticleDataModel
                {
                    Title = credentials.Title,
                    Description = credentials.Description,
                    AuthorId = author.Id,
                    Content = credentials.Content,
                    Tags = credentials.Tags.Length < 1 ? null : string.Join(',', credentials.Tags),
                    ImageId = uploadResult.ImageId,
                    ImageUrl = uploadResult.ImageUrl
                };

                // Create the article
                await context.Articles.AddAsync(article);

                // Save changes
                await context.SaveChangesAsync();

                // Return result
                return new OperationResult
                {
                    StatusCode = StatusCodes.Status201Created
                };
            }
            catch (Exception ex)
            {
                // Log the error
                logger.LogError(ex.Message);

                // Log the stack trace
                logger.LogError(ex.StackTrace);

                // Return error result
                return new OperationResult
                {
                    ErrorTitle = "SYSTEM ERROR",
                    StatusCode = StatusCodes.Status500InternalServerError,
                    ErrorMessage = ex.Message
                };
            }
        }

        public async Task<OperationResult> FetchAsync()
        {
            try
            {
                // Retrieve the articles
                var articles = await context.Articles
                    .Include(article => article.Author)
                    .Select(article => new ArticleApiModel
                    {
                        Id = article.Id,
                        Title = article.Title,
                        Description = article.Description,
                        Content = article.Content,
                        Author = new AuthorApiModel
                        {
                            FirstName = article.Author.FirstName,
                            LastName = article.Author.LastName,
                            PhotoUrl = article.Author.PhotoUrl
                        },
                        CaptionUrl = article.ImageUrl,
                        DateModified = article.DateModified
                    })
                    .OrderByDescending(author => author.DateModified)
                    .ToListAsync();

                // Return the result
                return new OperationResult
                {
                    Result = articles,
                    StatusCode = StatusCodes.Status200OK
                };
            }
            catch (Exception ex)
            {
                // Log the error
                logger.LogError(ex.Message);

                // Log the stack trace
                logger.LogError(ex.StackTrace);

                // Return error result
                return new OperationResult
                {
                    ErrorTitle = "SYSTEM ERROR",
                    StatusCode = StatusCodes.Status500InternalServerError,
                    ErrorMessage = ex.Message
                };
            }
        }

        public async Task<OperationResult> FetchAsync(string id)
        {
            try
            {
                // If id was not specified...
                if (string.IsNullOrEmpty(id))
                {
                    // Log the error
                    logger.LogError("The article id is required");

                    // Return error result
                    return new OperationResult
                    {
                        ErrorTitle = "BAD REQUEST",
                        StatusCode = StatusCodes.Status400BadRequest,
                        ErrorMessage = "The article id is required"
                    };
                }

                // Retrieve the article
                var article = await context.Articles
                    .Include(article => article.Author)
                    .Select(article => new ArticleApiModel
                    {
                        Id = article.Id,
                        Title = article.Title,
                        Description = article.Description,
                        Content = article.Content,
                        Author = new AuthorApiModel
                        {
                            FirstName = article.Author.FirstName,
                            LastName = article.Author.LastName,
                            PhotoUrl = article.Author.PhotoUrl
                        },
                        CaptionUrl = article.ImageUrl,
                        DateModified = article.DateModified
                    })
                    .FirstOrDefaultAsync(article => article.Id == id);

                // If article was not found
                if (article == null)
                {
                    // Log the error
                    logger.LogError("Article with the specified id could not be found");

                    // Return error result
                    return new OperationResult
                    {
                        ErrorTitle = "NOT FOUND",
                        StatusCode = StatusCodes.Status404NotFound,
                        ErrorMessage = "Article with the specified id could not be found"
                    };
                }

                // Return the result
                return new OperationResult
                {
                    Result = article,
                    StatusCode = StatusCodes.Status200OK
                };
            }
            catch (Exception ex)
            {
                // Log the error
                logger.LogError(ex.Message);

                // Log the stack trace
                logger.LogError(ex.StackTrace);

                // Return error result
                return new OperationResult
                {
                    ErrorTitle = "SYSTEM ERROR",
                    StatusCode = StatusCodes.Status500InternalServerError,
                    ErrorMessage = ex.Message
                };
            }
        }

        public async Task<OperationResult> FetchByAuthorAsync(string id)
        {
            try
            {
                // If id was not specified...
                if (string.IsNullOrEmpty(id))
                {
                    // Log the error
                    logger.LogError("The author's id is required");

                    // Return error result
                    return new OperationResult
                    {
                        ErrorTitle = "BAD REQUEST",
                        StatusCode = StatusCodes.Status400BadRequest,
                        ErrorMessage = "The author's id is required"
                    };
                }

                // Retrieve the article
                var articles = await context.Articles
                    .Include(article => article.Author)
                    .Where(article => article.AuthorId == id)
                    .Select(article => new ArticleApiModel
                    {
                        Id = article.Id,
                        Title = article.Title,
                        Description = article.Description,
                        Content = article.Content,
                        Author = new AuthorApiModel
                        {
                            FirstName = article.Author.FirstName,
                            LastName = article.Author.LastName,
                            PhotoUrl = article.Author.PhotoUrl
                        },
                        CaptionUrl = article.ImageUrl,
                        DateModified = article.DateModified
                    })
                    .ToListAsync();

                // Return the result
                return new OperationResult
                {
                    Result = articles,
                    StatusCode = StatusCodes.Status200OK
                };
            }
            catch (Exception ex)
            {
                // Log the error
                logger.LogError(ex.Message);

                // Log the stack trace
                logger.LogError(ex.StackTrace);

                // Return error result
                return new OperationResult
                {
                    ErrorTitle = "SYSTEM ERROR",
                    StatusCode = StatusCodes.Status500InternalServerError,
                    ErrorMessage = ex.Message
                };
            }
        }

        public async Task<OperationResult> UpdateAsync(string id, UpdateArticleApiModel credentials)
        {
            try
            {
                // If id was not specified...
                if (string.IsNullOrEmpty(id))
                {
                    // Log the error
                    logger.LogError("The article id is required");

                    // Return error result
                    return new OperationResult
                    {
                        ErrorTitle = "BAD REQUEST",
                        StatusCode = StatusCodes.Status400BadRequest,
                        ErrorMessage = "The article id is required"
                    };
                }

                // Retrieve the article
                var article = await context.Articles.FindAsync(id);

                // If article was not found...
                if (article == null)
                {
                    // Log the error
                    logger.LogError("Article with the specified id could not be found");

                    // Return error result
                    return new OperationResult
                    {
                        ErrorTitle = "NOT FOUND",
                        StatusCode = StatusCodes.Status404NotFound,
                        ErrorMessage = "Article with the specified id could not be found"
                    };
                }

                // Modify the article
                article.Title = credentials.Title ?? article.Title;
                article.Description = credentials.Description ?? article.Description;
                article.Content = credentials.Content ?? article.Content;
                article.AuthorId = credentials.AuthorId ?? article.AuthorId;
                article.Tags = credentials.Tags.Length < 1 ? article.Tags : string.Join(',', credentials.Tags);

                // If image was specified...
                if (!string.IsNullOrEmpty(credentials.Caption))
                {
                    // Upload the image
                    var uploadResult = await cloudinaryService
                        .UploadAsync(credentials.Caption, uploadPreset: CloudinaryUploadPresets.ArticleCaption);

                    // If upload failed...
                    if (!uploadResult.Successful)
                    {
                        // Log the error
                        logger.LogError(uploadResult.ErrorMessage + "\n" + JsonSerializer.Serialize(uploadResult.Errors));

                        // Return error result
                        return new OperationResult
                        {
                            ErrorTitle = "SYSTEM ERROR",
                            StatusCode = StatusCodes.Status500InternalServerError,
                            ErrorMessage = uploadResult.ErrorMessage + "\n" + JsonSerializer.Serialize(uploadResult.Errors)
                        };
                    }

                    // Delete the caption from cloud
                    await cloudinaryService.DeleteFromCloudinaryAsync(article.ImageId);

                    // Modify the article image credentials
                    article.ImageId = uploadResult.ImageId;
                    article.ImageUrl = uploadResult.ImageUrl;
                }

                // Update the article
                context.Articles.Update(article);

                // Save changes
                await context.SaveChangesAsync();

                // Return the result
                return new OperationResult
                {
                    StatusCode = StatusCodes.Status201Created
                };
            }
            catch (Exception ex)
            {
                // Log the error
                logger.LogError(ex.Message);

                // Log the stack trace
                logger.LogError(ex.StackTrace);

                // Return error result
                return new OperationResult
                {
                    ErrorTitle = "SYSTEM ERROR",
                    StatusCode = StatusCodes.Status500InternalServerError,
                    ErrorMessage = ex.Message
                };
            }
        }

        public async Task<OperationResult> DeleteAsync(string id)
        {
            try
            {
                // If id was not specified...
                if (string.IsNullOrEmpty(id))
                {
                    // Log the error
                    logger.LogError("The article id is required");

                    // Return error result
                    return new OperationResult
                    {
                        ErrorTitle = "BAD REQUEST",
                        StatusCode = StatusCodes.Status400BadRequest,
                        ErrorMessage = "The article id is required"
                    };
                }

                // Retrieve the article
                var article = await context.Articles.FindAsync(id);

                // If article was not found...
                if (article == null)
                {
                    // Log the error
                    logger.LogError("Article with the specified id could not be found");

                    // Return error result
                    return new OperationResult
                    {
                        ErrorTitle = "NOT FOUND",
                        StatusCode = StatusCodes.Status404NotFound,
                        ErrorMessage = "Article with the specified id could not be found"
                    };
                }

                // Delete the caption from cloud
                await cloudinaryService.DeleteFromCloudinaryAsync(article.ImageId);

                // Delete the article
                context.Articles.Remove(article);

                // Save changes
                await context.SaveChangesAsync();

                // Return the result
                return new OperationResult
                {
                    StatusCode = StatusCodes.Status204NoContent
                };
            }
            catch (Exception ex)
            {
                // Log the error
                logger.LogError(ex.Message);

                // Log the stack trace
                logger.LogError(ex.StackTrace);

                // Return error result
                return new OperationResult
                {
                    ErrorTitle = "SYSTEM ERROR",
                    StatusCode = StatusCodes.Status500InternalServerError,
                    ErrorMessage = ex.Message
                };
            }
        }
    }
}