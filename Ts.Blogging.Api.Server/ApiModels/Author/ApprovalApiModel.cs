using System.ComponentModel.DataAnnotations;

namespace Ts.Blogging.Api.Server
{
    /// <summary>
    /// The API model to approve to author
    /// </summary>
    public class ApprovalApiModel
    {
        /// <summary>
        /// The status of an author
        /// </summary>
        [Required(ErrorMessage = "Please specify a valid staus")]
        public string Status { get; set; }
    }
}
