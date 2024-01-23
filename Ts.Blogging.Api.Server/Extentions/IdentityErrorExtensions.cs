using Dna;
using Microsoft.AspNetCore.Identity;

namespace Ts.Blogging.Api.Server
{
    /// <summary>
    /// Extension methods for <see cref="IdentityError"/> class
    /// </summary>
    public static class IdentityErrorExtensions
    {
        /// <summary>
        /// Combines all errors into a single string
        /// </summary>
        /// <param name="errors">The errors to aggregate</param>
        /// <returns>Returns a string with each error separated by a new line</returns>
        public static string AggregateErrors(this IEnumerable<IdentityError> errors)
        {
            // Get all errors into a list
            return errors?.ToList()
                          // Grab their description
                          .Select(f => f.Description)
                          // And combine them with a newline separator
                          .Aggregate((a, b) => $"{a}{Environment.NewLine}{b}");
        }

        /// <summary>
        /// Composes unique username extracted from email address
        /// </summary>
        /// <param name="email"></param>
        /// <returns></returns>
        public static string ComposeUserName(this string email)
        {
            // Get the index of the '@' character
            int atIndex = email.IndexOf('@');

            // Extract the name from the email address
            string username = email[..atIndex];

            // Get the unique username
            GetUniqueUserName(ref username);

            // Return username
            return username;
        }

        /// <summary>
        /// Generates unique username recursively
        /// </summary>
        /// <param name="username">The extracted username from email address</param>
        /// <returns></returns>
        private static void GetUniqueUserName(ref string username)
        {
            string un = username;

            // Create a scope instance
            using var scope = Framework.Provider.CreateScope();

            // Get the id context instance
            var idContext = scope.ServiceProvider.GetRequiredService<IdentityDbContext>();

            // If the username exist...
            if (idContext.Users.Any(u => u.UserName == un))
            {
                // Generate characters
                var chars = DateTimeOffset.Now.ToUnixTimeSeconds().ToString()[6..];

                // Append characters
                username += chars;

                // If username is still not unique...
                if (idContext.Users.Any(u => u.UserName == un))
                {
                    // Repeat the operation
                    GetUniqueUserName(ref username);
                }
            }
        }
    }
}
