using System.Security.Claims;
using System.Text.Json;
using IdentityModel;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Ts.Blogging.Api.Server
{
    public class AuthorManagement
    {
        #region Private Members

        /// <summary>
        /// The scoped instance of the <see cref="ApplicationDbContext"/>
        /// </summary>
        private readonly ApplicationDbContext context;

        /// <summary>
        /// The scoped instance of the <see cref="IdentityDbContext"/>
        /// </summary>
        private readonly IdentityDbContext idContext;

        /// <summary>
        /// The singleton instance of the <see cref="ILogger{CategoryName}"/>
        /// </summary>
        private ILogger<AuthorManagement> logger;

        /// <summary>
        /// The scoped instance of the <see cref="CloudinaryService"/>
        /// </summary>
        private readonly CloudinaryService cloudinaryService;

        /// <summary>
        /// The scoped instance of the <see cref="UserManager{TUser}"/>
        /// </summary>
        private readonly UserManager<ApplicationUser> userManager;

        #endregion

        #region Constructor

        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="context">The injected context</param>
        /// <param name="cloudinaryService">The injected cloudinary service</param>
        /// <param name="logger">The injected logger</param>
        /// <param name="userManager">The injected user manager</param>
        /// <param name="idContext">The injected id context</param>
        public AuthorManagement(ApplicationDbContext context,
            CloudinaryService cloudinaryService,
            ILogger<AuthorManagement> logger,
            UserManager<ApplicationUser> userManager,
            IdentityDbContext idContext)
        {
            this.context = context;
            this.cloudinaryService = cloudinaryService;
            this.logger = logger;
            this.userManager = userManager;
            this.idContext = idContext;
        }

        #endregion

        /// <summary>
        /// Creates author based on provided credentials
        /// </summary>
        /// <param name="credentials">The provided author credentials</param>
        /// <returns></returns>
        public async Task<OperationResult> RegisterAsync(AuthorCredentials credentials)
        {
            try
            {
                // Begin transaction
                await idContext.Database.BeginTransactionAsync();

                // Compose a username for the user
                var un = credentials.Email.ComposeUserName();

                // Create the user
                var userResult = await userManager.CreateAsync(new ApplicationUser
                {
                    UserName = un,
                    Email = credentials.Email,
                    PhoneNumber = credentials.Phone,
                    FirstName = credentials.FirstName,
                    LastName = credentials.LastName,
                    Scope = UserScopes.Author
                }, credentials.Password);

                // If user creation failed...
                if (!userResult.Succeeded)
                {
                    // Log the error
                    logger.LogError(userResult.Errors.AggregateErrors());

                    // Return error result
                    return new OperationResult
                    {
                        ErrorTitle = "BAD REQUEST",
                        StatusCode = StatusCodes.Status400BadRequest,
                        ErrorMessage = userResult.Errors.AggregateErrors()
                    };
                }

                // Fetch the user
                var authorUser = await userManager.FindByNameAsync(un);

                // Create claims
                var claimsResult = await userManager.AddClaimsAsync(authorUser, new List<Claim>
                {
                    new(ClaimTypes.Name, authorUser.UserName),
                    new(ClaimTypes.Email, authorUser.Email),
                    new(JwtClaimTypes.Scope, UserScopes.Author)
                });

                // If claims creation failed...
                if (!claimsResult.Succeeded)
                {
                    // Log the error
                    logger.LogError(claimsResult.Errors.AggregateErrors());

                    // Rollback
                    await idContext.Database.RollbackTransactionAsync();

                    // Return error result
                    return new OperationResult
                    {
                        ErrorTitle = "BAD REQUEST",
                        StatusCode = StatusCodes.Status500InternalServerError,
                        ErrorMessage = claimsResult.Errors.AggregateErrors()
                    };
                }

                // Upload the photo
                var uploadResult = await cloudinaryService
                    .UploadAsync(credentials.Photo, uploadPreset: CloudinaryUploadPresets.AuthorPhoto);

                // If upload failed...
                if (!uploadResult.Successful)
                {
                    // Log the error
                    logger.LogError(uploadResult.ErrorMessage + "\n" + JsonSerializer.Serialize(uploadResult.Errors));

                    // Rollback
                    await idContext.Database.RollbackTransactionAsync();

                    // Return error result
                    return new OperationResult
                    {
                        ErrorTitle = "SYSTEM ERROR",
                        StatusCode = StatusCodes.Status500InternalServerError,
                        ErrorMessage = uploadResult.ErrorMessage + "\n" + JsonSerializer.Serialize(uploadResult.Errors)
                    };
                }

                // Set the author's credentials
                var author = new AuthorDataModel
                {
                    Title = credentials.Title,
                    UserId = authorUser.Id,
                    Email = authorUser.Email,
                    UserName = authorUser.UserName,
                    Phone = authorUser.PhoneNumber,
                    FirstName = credentials.FirstName,
                    LastName = credentials.LastName,
                    PhotoId = uploadResult.ImageId,
                    PhotoUrl = uploadResult.ImageUrl,
                    Status = AuthorStatus.Pending
                };

                // Create the author
                await context.Authors.AddAsync(author);

                // Save changes
                var succeeded = await context.SaveChangesAsync() > 0;

                // If failed...
                if (!succeeded)
                {
                    // Log error
                    logger.LogError("Failed to create author due to database error");

                    // Rollback
                    await idContext.Database.RollbackTransactionAsync();

                    // Return error result
                    return new OperationResult
                    {
                        ErrorTitle = "SYSTEM ERROR",
                        StatusCode = StatusCodes.Status500InternalServerError,
                        ErrorMessage = "Failed to create author due to database error"
                    };
                }

                // Commit transaction
                await idContext.Database.CommitTransactionAsync();

                // Return result
                return new OperationResult
                {
                    StatusCode = StatusCodes.Status201Created,
                    Result = new AuthorApiModel
                    {
                        Id = author.Id,
                        Title = author.Title,
                        UserName = authorUser.UserName,
                        Email = authorUser.Email,
                        FirstName = author.FirstName,
                        LastName = author.LastName,
                        PhotoUrl = author.PhotoUrl,
                        DateModified = author.DateModified
                    }
                };
            }
            catch (Exception ex)
            {
                // Log the error
                logger.LogError(ex.Message);

                // Log the stack trace
                logger.LogError(ex.StackTrace);

                // Rollback
                await idContext.Database.RollbackTransactionAsync();

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
                // Retrieve the authors
                var authors = await context.Authors
                    .Select(author => new AuthorApiModel
                    {
                        Id = author.Id,
                        Title = author.Title,
                        Email = author.Email,
                        Status = author.Status,
                        FirstName = author.FirstName,
                        LastName = author.LastName,
                        PhotoUrl = author.PhotoUrl,
                        DateModified = author.DateModified
                    })
                    .OrderByDescending(author => author.DateModified)
                    .ToListAsync();

                // Return the result
                return new OperationResult
                {
                    Result = authors,
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
                    logger.LogError("The author id is required");

                    // Return error result
                    return new OperationResult
                    {
                        ErrorTitle = "BAD REQUEST",
                        StatusCode = StatusCodes.Status400BadRequest,
                        ErrorMessage = "The author id is required"
                    };
                }

                // Retrieve the author
                var author = await context.Authors
                    .Include(author => author.Articles)
                    .FirstOrDefaultAsync(author => author.Id == id);

                // If author was not found
                if (author == null)
                {
                    // Log the error
                    logger.LogError("Author with the specified id could not be found");

                    // Return error result
                    return new OperationResult
                    {
                        ErrorTitle = "NOT FOUND",
                        StatusCode = StatusCodes.Status404NotFound,
                        ErrorMessage = "Author with the specified id could not be found"
                    };
                }

                // Initialize author result
                AuthorApiModel authorResult = new()
                {
                    Id = author.Id,
                    Title = author.Title,
                    Email = author.Email,
                    Status = author.Status,
                    FirstName = author.FirstName,
                    LastName = author.LastName,
                    PhotoUrl = author.PhotoUrl,
                    DateModified = author.DateModified,
                    Articles = []
                };

                // For each author's article...
                foreach (var article in author.Articles)
                {
                    // Add author's article
                    authorResult.Articles.Add(new ArticleApiModel
                    {
                        Id = article.Id,
                        Title = article.Title,
                        Description = article.Description,
                        Content = article.Content,
                        DateModified = article.DateModified
                    });
                }

                // Return the result
                return new OperationResult
                {
                    Result = authorResult,
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

        public async Task<OperationResult> FetchAsync(ApplicationUser user)
        {
            try
            {
                // Retrieve the author
                var author = await context.Authors
                    .Include(author => author.Articles)
                    .FirstOrDefaultAsync(author => author.UserId == user.Id);

                // If author was not found
                if (author == null)
                {
                    // Log the error
                    logger.LogError("Author could not be found");

                    // Return error result
                    return new OperationResult
                    {
                        ErrorTitle = "NOT FOUND",
                        StatusCode = StatusCodes.Status404NotFound,
                        ErrorMessage = "Author could not be found"
                    };
                }

                // Initialize author result
                AuthorApiModel authorResult = new()
                {
                    Id = author.Id,
                    Title = author.Title,
                    UserName = user.UserName,
                    Email = user.Email,
                    Status = author.Status,
                    FirstName = author.FirstName,
                    LastName = author.LastName,
                    PhotoUrl = author.PhotoUrl,
                    DateModified = author.DateModified,
                    Articles = []
                };

                // For each author's article...
                foreach (var article in author.Articles)
                {
                    // Add author's article
                    authorResult.Articles.Add(new ArticleApiModel
                    {
                        Id = article.Id,
                        Title = article.Title,
                        Description = article.Description,
                        Content = article.Content,
                        DateModified = article.DateModified
                    });
                }

                // Return the result
                return new OperationResult
                {
                    Result = authorResult,
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

        public async Task<OperationResult> UpdateAsync(ApplicationUser user, UpdateAuthorApiModel credentials)
        {
            try
            {
                // Retrieve the author
                var author = await context.Authors.FirstOrDefaultAsync(a => a.UserId == user.Id);

                // If author was not found
                if (author == null)
                {
                    // Log the error
                    logger.LogError("Author with the specified id could not be found");

                    // Return error result
                    return new OperationResult
                    {
                        ErrorTitle = "NOT FOUND",
                        StatusCode = StatusCodes.Status404NotFound,
                        ErrorMessage = "Author with the specified id could not be found"
                    };
                }

                // Modify the author
                author.Title = credentials.Title ?? author.Title;
                author.FirstName = credentials.FirstName ?? author.FirstName;
                author.LastName = credentials.LastName ?? author.LastName;

                // Set the user details
                user.FirstName = author.FirstName;
                user.LastName = author.LastName;

                // If photo was specified...
                if (!string.IsNullOrEmpty(credentials.Photo))
                {
                    // Upload the photo
                    var uploadResult = await cloudinaryService
                        .UploadAsync(credentials.Photo, uploadPreset: CloudinaryUploadPresets.AuthorPhoto);

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

                    // Delete the photo from cloud
                    await cloudinaryService.DeleteFromCloudinaryAsync(author.PhotoId);

                    // Modify the author image credentials
                    author.PhotoId = uploadResult.ImageId;
                    author.PhotoUrl = uploadResult.ImageUrl;
                }

                // Update the author
                context.Authors.Update(author);

                // Update the user
                await userManager.UpdateAsync(user);

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
                    logger.LogError("The author id is required");

                    // Return error result
                    return new OperationResult
                    {
                        ErrorTitle = "BAD REQUEST",
                        StatusCode = StatusCodes.Status400BadRequest,
                        ErrorMessage = "The author id is required"
                    };
                }

                // Retrieve the author
                var author = await context.Authors.FindAsync(id);

                // If author was not found
                if (author == null)
                {
                    // Log the error
                    logger.LogError("Author with the specified id could not be found");

                    // Return error result
                    return new OperationResult
                    {
                        ErrorTitle = "NOT FOUND",
                        StatusCode = StatusCodes.Status404NotFound,
                        ErrorMessage = "Author with the specified id could not be found"
                    };
                }

                // Delete the caption from cloud
                await cloudinaryService.DeleteFromCloudinaryAsync(author.PhotoId);

                // Delete the author
                context.Authors.Remove(author);

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

        public async Task<OperationResult> UpdateStatusAsync(ApprovalApiModel model, string id)
        {
            try
            {
                // Fetch the author
                var author = await context.Authors.FindAsync(id);

                // If author was not found...
                if (author == null)
                {
                    // Log error
                    logger.LogError("Author with the specified id could not be found");

                    // Return error result
                    return new OperationResult
                    {
                        ErrorTitle = "NOT FOUND",
                        StatusCode = StatusCodes.Status404NotFound,
                        ErrorMessage = "Author with the specified id could not be found"
                    };
                }

                // If specified status is not valid...
                if (!(model.Status == AuthorStatus.Approved || model.Status == AuthorStatus.Disapproved))
                {
                    // Log error
                    logger.LogError("Specified author's status is not valid");

                    // Return error result
                    return new OperationResult
                    {
                        ErrorTitle = "BAD REQUEST",
                        StatusCode = StatusCodes.Status400BadRequest,
                        ErrorMessage = "Specified author's status is not valid"
                    };
                }

                // Set the author's status
                author.Status = model.Status;

                // Update the author
                context.Authors.Update(author);

                // Save changes
                await context.SaveChangesAsync();

                // Return result
                return new OperationResult
                {
                    StatusCode = StatusCodes.Status204NoContent
                };
            }
            catch (Exception ex)
            {
                // Log the error
                logger.LogError(ex.Message);

                // Return response
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