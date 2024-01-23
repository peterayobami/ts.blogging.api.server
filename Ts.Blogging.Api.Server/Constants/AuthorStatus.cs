namespace Ts.Blogging.Api.Server
{
    /// <summary>
    /// The approval status of the author
    /// </summary>
    public class AuthorStatus
    {
        /// <summary>
        /// Authour approval is pending
        /// </summary>
        public const string Pending = "PENDING";

        /// <summary>
        /// Author is approved
        /// </summary>
        public const string Approved = "APPROVED";

        /// <summary>
        /// The autour has been disapproved
        /// </summary>
        public const string Disapproved = "DISAPPROVED";
    }
}
