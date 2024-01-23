namespace Ts.Blogging.Api.Server
{
    /// <summary>
    /// The endpoint routes
    /// </summary>
    public class EndpointRoutes
    {
        #region Article

        /// <summary>
        /// The route to the CreateArticle endpoint
        /// </summary>
        public const string CreateArticle = "article/create";

        /// <summary>
        /// The route to the FetchArticles endpoint
        /// </summary>
        public const string FetchArticles = "articles/fetch";

        /// <summary>
        /// The route to the FetchArticle endpoint
        /// </summary>
        public const string FetchArticle = "article/fetch/{id}";

        /// <summary>
        /// The route to the FetchArticle endpoint
        /// </summary>
        public const string FetchArticlesByAuthor = "author-articles/fetch/{id}";

        /// <summary>
        /// The route to the UpdateArticle endpoint
        /// </summary>
        public const string UpdateArticle = "article/update/{id}";

        /// <summary>
        /// The route to the DeleteArticle endpoint
        /// </summary>
        public const string DeleteArticle = "article/delete/{id}";

        /// <summary>
        /// The route to the AdminDeleteArticle endpoint
        /// </summary>
        public const string AdminDeleteArticle = "article/admin-delete/{id}";

        #endregion

        #region Author

        /// <summary>
        /// The route to the RegisterAuthor endpoint
        /// </summary>
        public const string RegisterAuthor = "author/register";

        /// <summary>
        /// The route to the FetchAuthors endpoint
        /// </summary>
        public const string FetchAuthors = "authors/fetch";

        /// <summary>
        /// The route to the FetchAuthor endpoint
        /// </summary>
        public const string FetchAuthor = "author/fetch";

        /// <summary>
        /// The route to the GetAuthor endpoint
        /// </summary>
        public const string GetAuthor = "author/get/{id}";

        /// <summary>
        /// The route to the UpdateAuthor endpoint
        /// </summary>
        public const string UpdateAuthor = "author/update";

        /// <summary>
        /// The route to the DeleteAuthor endpoint
        /// </summary>
        public const string DeleteAuthor = "author/delete/{id}";

        #endregion

        #region Tag

        /// <summary>
        /// The route to the CreateTag endpoint
        /// </summary>
        public const string CreateTag = "tag/create";

        /// <summary>
        /// The route to the FetchTags endpoint
        /// </summary>
        public const string FetchTags = "tags/fetch";

        /// <summary>
        /// The route to the FetchTag endpoint
        /// </summary>
        public const string FetchTag = "tag/fetch/{id}";

        /// <summary>
        /// The route to the UpdateTag endpoint
        /// </summary>
        public const string UpdateTag = "tag/update/{id}";

        /// <summary>
        /// The route to the DeleteTag endpoint
        /// </summary>
        public const string DeleteTag = "tag/delete/{id}";

        #endregion

        #region Admin

        /// <summary>
        /// The endpoint route to update author's approval status
        /// </summary>
        public const string UpdateAuthorApprovalStatus = "author-approval-status/update/{id}";

        #endregion

        #region User

        /// <summary>
        /// The route to the Login endpoint
        /// </summary>
        public const string Login = "user/login";

        #endregion
    }
}