namespace Ts.Blogging.Api.Server
{
    /// <summary>
    /// The base data model for other data models
    /// </summary>
    public class BaseDataModel
    {
        /// <summary>
        /// The unique id for any entry
        /// </summary>
        public string Id { get; set; } = Guid.NewGuid().ToString();

        /// <summary>
        /// The point in time record was created
        /// </summary>
        public DateTimeOffset DateCreated { get; set; }

        /// <summary>
        /// The point in time record was modified
        /// </summary>
        public DateTimeOffset DateModified { get; set; }
    }
}
