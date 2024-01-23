using CloudinaryDotNet;
using Dna;
using Duende.IdentityServer.EntityFramework.DbContexts;
using IdentityModel;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;
using System.Text;

namespace Ts.Blogging.Api.Server
{
    /// <summary>
    /// Application extensions
    /// </summary>
    public static class ApplicationExtensions
    {
        /// <summary>
        /// This will apply pending migrations to the db
        /// </summary>
        /// <param name="app"></param>
        public async static Task ApplyMigrationsAsync(this WebApplication app)
        {
            // Create the scope instance
            using var scope = app.Services.CreateScope();

            // Get the service scope
            var service = scope.ServiceProvider;

            // Get the logger instance
            var loggerFactory = service.GetRequiredService<ILoggerFactory>();

            try
            {
                // Get the application database context instance
                var context = service.GetRequiredService<ApplicationDbContext>();

                // Get the identity database context instance
                var idContext = service.GetRequiredService<IdentityDbContext>();

                // Get the user manager instance
                var userManager = service.GetRequiredService<UserManager<ApplicationUser>>();

                // Migrate the application database
                context.Database.Migrate();

                // Migrate the identity database
                idContext.Database.Migrate();

                // If we don't have any user...
                if (!await idContext.Users.AnyAsync())
                {
                    // Create admin user
                    var userResult = await userManager.CreateAsync(new ApplicationUser
                    {
                        UserName = "tsadmin",
                        Email = "admin@tsblog.com",
                        PhoneNumber = "08021334725",
                        FirstName = "Ts",
                        LastName = "Admin",
                        Scope = UserScopes.Admin
                    }, "adminpassword");

                    // If admin user was created successfully...
                    if (userResult.Succeeded)
                    {
                        // Fetch the user
                        var adminUser = await userManager.FindByNameAsync("tsadmin");

                        // Create claims
                        await userManager.AddClaimsAsync(adminUser, new List<Claim>
                        {
                            new(ClaimTypes.Name, "tsadmin"),
                            new(ClaimTypes.Email, "admin@tsblog.com"),
                            new(JwtClaimTypes.Scope, UserScopes.Admin)
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                // Create logger
                var logger = loggerFactory.CreateLogger<Program>();

                // Log the error
                logger.LogError("An error occurred while applying migrations. Details: {error}", ex.Message);
            }
        }

        /// <summary>
        /// Configures the database context for the application
        /// </summary>
        /// <param name="services">The <see cref="IServiceCollection"/></param>
        /// <param name="configuration">The <see cref="IConfiguration"/></param>
        /// <returns>Return the <see cref="IServiceCollection"/> for further chaining</returns>
        public static IServiceCollection AddDatabaseContext(this IServiceCollection services, IConfiguration configuration)
        {
            // Add ApplicationDbContext to DI container
            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseNpgsql(configuration.GetConnectionString("DefaultConnection"),
                sqlOptions => sqlOptions.MigrationsAssembly(typeof(Program).Assembly.GetName().Name)));

            // Add IdentityDbContext to DI container
            services.AddDbContext<IdentityDbContext>(options =>
                options.UseNpgsql(configuration.GetConnectionString("IdentityConnection"),
                sqlOptions => sqlOptions.MigrationsAssembly(typeof(Program).Assembly.GetName().Name)));

            // Return services for further chaining
            return services;
        }

        /// <summary>
        /// Registers application identity to the DI
        /// </summary>
        /// <param name="services">The <see cref="IServiceCollection"/> instance</param>
        /// <returns></returns>
        public static IServiceCollection AddApplicationIdentity(this IServiceCollection services)
        {
            // Configure identity
            services.AddIdentity<ApplicationUser, IdentityRole>(options =>
            {
                options.User.RequireUniqueEmail = true;
                options.Password.RequiredLength = 8;
                options.Password.RequireUppercase = false;
                options.Password.RequireDigit = false;
                options.Password.RequireNonAlphanumeric = false;
            })
                .AddDefaultTokenProviders()
                .AddEntityFrameworkStores<IdentityDbContext>();

            // Return services for further chaining
            return services;
        }

        /// <summary>
        /// Registers the Identity Server configuration to DI container
        /// </summary>
        /// <param name="services">The instance of <see cref="IServiceCollection"/></param>
        /// <param name="services">The instance of <see cref="IConfiguration"/></param>
        /// <returns></returns>
        public static IServiceCollection AddIdentityServerConfiguration(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddIdentityServer(options =>
            {
                //options.UserInteraction.LoginUrl = WebRoutes.SignIn;
            })
                .AddConfigurationStore<ConfigurationDbContext>(options => options.ConfigureDbContext =
                    builder => builder.UseNpgsql(configuration.GetConnectionString("ConfigurationConnection"),
                    sqlOptions => sqlOptions.MigrationsAssembly(typeof(Program).Assembly.GetName().Name)));

            // Return services for further chaining
            return services;
        }

        public static IServiceCollection AddAuthentizationConfiguration(this IServiceCollection services, IConfiguration configuration)
        {
            // Add authentication
            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(JwtBearerDefaults.AuthenticationScheme, options =>
                {
                    options.Authority = configuration["Jwt:Authority"];

                    // Set validation parameters
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        // Validate issuer
                        ValidateIssuer = true,
                        // Validate audience
                        ValidateAudience = true,
                        // Validate expiration
                        ValidateLifetime = true,
                        // Validate signature
                        ValidateIssuerSigningKey = true,

                        // Set issuer
                        ValidIssuer = Framework.Construction.Configuration["Jwt:Issuer"],
                        // Set audience
                        ValidAudience = Framework.Construction.Configuration["Jwt:Audience"],

                        // Set signing key
                        IssuerSigningKey = new SymmetricSecurityKey(
                            // Get our secret key from configuration
                            Encoding.UTF8.GetBytes(Framework.Construction.Configuration["Jwt:SecretKey"])),

                        // Valid types are at + jwt
                        //ValidTypes = new[] { "jwt" }
                    };
                });

            // Return services for further chaining
            return services;
        }

        /// <summary>
        /// Registers authorization configuration to DI container
        /// </summary>
        /// <param name="services">The instance of the <see cref="IServiceCollection"/></param>
        /// <returns></returns>
        public static IServiceCollection AddAuthorizationConfiguration(this IServiceCollection services)
        {
            // Add authorization and configure policies
            services.AddAuthorization(options =>
            {
                // Configure authorization policy for author
                options.AddPolicy(AuthorizationPolicies.Author, policy =>
                {
                    policy.RequireAuthenticatedUser();

                    policy.RequireClaim(JwtClaimTypes.Scope, UserScopes.Author);
                });

                // Configure authorization policy for admin
                options.AddPolicy(AuthorizationPolicies.Admin, policy =>
                {
                    policy.RequireAuthenticatedUser();

                    policy.RequireClaim(JwtClaimTypes.Scope, UserScopes.Admin);
                });
            });

            // Return services for further chaining
            return services;
        }

        /// <summary>
        /// Add CORs configuration to DI
        /// </summary>
        /// <param name="services">The <see cref="IServiceCollection"/> instance</param>
        /// <returns>Returns the <see cref="IServiceCollection"/> for further chaining</returns>
        public static IServiceCollection ConfigureCors(this IServiceCollection services)
        {
            services.AddCors(options =>
            {
                options.AddPolicy("Default", policy =>
                {
                    policy.AllowAnyHeader()
                    .AllowAnyOrigin()
                    .AllowAnyMethod();
                });
            });

            // Return services for further chaining
            return services;
        }

        /// <summary>
        /// Add our domain services to IOC container
        /// </summary>
        /// <param name="services">The <see cref="IServiceCollection"/> instance</param>
        /// <returns>Returns the <see cref="IServiceCollection"/> for further chaining</returns>
        public static IServiceCollection AddDomainServices(this IServiceCollection services)
        {
            services.AddScoped<ArticleManagement>()
                .AddScoped<AuthorManagement>()
                .AddScoped<TagManagement>();

            // Return services for further chaining
            return services;
        }

        /// <summary>
        /// Registers the cloudinary media service to DI container
        /// </summary>
        /// <param name="services">The <see cref="IServiceCollection"/> instance</param>
        /// <param name="configuration">The <see cref="IConfiguration"/> instance</param>
        /// <returns>Returns the <see cref="IServiceCollection"/> for further chaining</returns>
        public static IServiceCollection AddCloudinary(this IServiceCollection services, IConfiguration configuration)
        {
            // Add cloudinary as a scoped service
            services.AddScoped(x => new Cloudinary(new Account
            {
                ApiKey = configuration["Cloudinary:Key"],
                ApiSecret = configuration["Cloudinary:Secret"],
                Cloud = configuration["Cloudinary:Cloud"]
            }));

            // Add the scoped instance
            services.AddScoped<CloudinaryService>();

            // Return services for further chaining
            return services;
        }
    }
}
