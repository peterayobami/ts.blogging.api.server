namespace Ts.Blogging.Api.Server
{
    /// <summary>
    /// The login credentials API model
    /// </summary>
    public class LoginCredentials
    {
        /// <summary>
        /// The user's email
        /// </summary>
        public string Email { get; set; }

        /// <summary>
        /// The user's password
        /// </summary>
        public string Password { get; set; }
    }
}
