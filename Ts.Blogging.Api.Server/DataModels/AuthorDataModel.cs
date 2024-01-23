namespace Ts.Blogging.Api.Server
{
    /// <summary>
    /// The authors database table representational model
    /// </summary>
    public class AuthorDataModel : BaseDataModel
    {
        /// <summary>
        /// The user id of the author
        /// </summary>
        public string UserId { get; set; }

        /// <summary>
        /// The title of the author
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// The first name of the author
        /// </summary>
        public string FirstName { get; set; }

        /// <summary>
        /// The last name of the author
        /// </summary>
        public string LastName { get; set; }

        /// <summary>
        /// The email address of the author
        /// </summary>
        public string Email { get; set; }

        /// <summary>
        /// The username of the author
        /// </summary>
        public string UserName { get; set; }

        /// <summary>
        /// The phone number of the author
        /// </summary>
        public string Phone { get; set; }

        /// <summary>
        /// The photo id of the author
        /// </summary>
        public string PhotoId { get; set; }

        /// <summary>
        /// The photo url of the author
        /// </summary>
        public string PhotoUrl { get; set; }

        /// <summary>
        /// The approval status of the author
        /// </summary>
        public string Status { get; set; }

        /// <summary>
        /// The author's articles entity relational model
        /// </summary>
        public List<ArticleDataModel> Articles { get; set; }
    }
}
