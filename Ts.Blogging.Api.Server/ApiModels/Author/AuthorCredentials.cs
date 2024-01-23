using System.ComponentModel.DataAnnotations;

namespace Ts.Blogging.Api.Server
{
    /// <summary>
    /// The credentials to create an author
    /// </summary>
    public class AuthorCredentials
    {
        /// <summary>
        /// The title of the author
        /// </summary>
        [Required(ErrorMessage = "The author's title is required")]
        public string Title { get; set; }

        [Required(ErrorMessage = "The author's email is required")]
        [EmailAddress(ErrorMessage = "Please provide a valid email address")]
        public string Email { get; set; }

        [Required(ErrorMessage = "The author's phone number is required")]
        public string Phone { get; set; }

        /// <summary>
        /// The author's first name
        /// </summary>
        [Required(ErrorMessage = "The author's first name is required")]
        public string FirstName { get; set; }

        /// <summary>
        /// The author's last name
        /// </summary>
        [Required(ErrorMessage = "The author's last name is required")]
        public string LastName { get; set; }

        /// <summary>
        /// The author's display photo
        /// </summary>
        [Required(ErrorMessage = "The author's display photo is required")]
        public string Photo { get; set; }

        /// <summary>
        /// The password of the author
        /// </summary>
        [Required(ErrorMessage = "Please specify a password")]
        public string Password { get; set; }
    }
}