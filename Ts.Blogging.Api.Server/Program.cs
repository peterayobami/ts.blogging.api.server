using Dna;
using Dna.AspNet;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.Filters;
using System.Reflection;
using Ts.Blogging.Api.Server;

// Create the web application builder
var builder = WebApplication.CreateBuilder(args);

// Configure DnaFramework
builder.WebHost.UseDnaFramework(construct =>
{
    // Add configuration
    construct.AddConfiguration(builder.Configuration);
});

// Add services to the container.

// Add controllers
builder.Services.AddControllers();

// Add ApplicationDbContext
builder.Services.AddDatabaseContext(builder.Configuration)
    .AddApplicationIdentity()
    .AddAuthentizationConfiguration(builder.Configuration)
    .AddAuthorizationConfiguration()
    .ConfigureCors()
    .AddCloudinary(builder.Configuration)
    .AddDomainServices();

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Ts Blogging API",
        Version = "v1",
        Description = "The APIs for Ts Blogging System"
    });

    // Include the XML comments
    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    options.IncludeXmlComments(xmlPath);

    // Add example filters
    options.ExampleFilters();
});

// Add swagger examples
builder.Services.AddSwaggerExamplesFromAssemblyOf<ArticleCredentials>();
builder.Services.AddSwaggerExamplesFromAssemblyOf<AuthorCredentials>();

var app = builder.Build();

// Configure the HTTP request pipeline.

// Migrate the databases
await app.ApplyMigrationsAsync();

app.UseSwagger();
app.UseSwaggerUI(options =>
{
    options.SwaggerEndpoint("/swagger/v1/swagger.json", "Ts Blogging API v1");
    options.RoutePrefix = string.Empty;
});

app.UseHttpsRedirection();

app.UseDnaFramework();

app.UseRouting();

app.UseAuthentication();

app.UseAuthorization();

app.MapControllers();

app.UseCors("Default");

app.Run();
