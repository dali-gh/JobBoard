using JobBoard.API.Errors;
using JobBoard.API.Queries;
using JobBoard.Application.Interfaces;
using JobBoard.Infrastructure.Data;
using JobBoard.Infrastructure.Repositories;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Datebase Setup
builder.Services.AddDbContext<AppDbContext>(options =>options.UseSqlServer(builder.Configuration.GetConnectionString("Defaultconnection")));

builder.Services.AddScoped<IJobRepository, JobRepository>();

// Get current user through IHttpContextAccessor/Claims
builder.Services.AddHttpContextAccessor();

// JWT Authentication Setup

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["JwtSettings:Issuer"],
            ValidAudience = builder.Configuration["JwtSettings:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["JwtSettings:SecretKey"]!))

        };
    });

builder.Services.AddAuthorization();
// Connect to the Client Side
builder.Services.AddCors(options =>
{
    options.AddPolicy("JobBoardClient", policy =>
    {
        policy
            .WithOrigins("http://127.0.0.1:5501")
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});
// SetUp Error

builder.Services.AddErrorFilter<GraphQLErrorFilter>();

// Add GraphQL Server
builder.Services.AddGraphQLServer()
    .AddQueryType<Query>()
    .AddMutationType<Mutation>()
    .AddSubscriptionType<Subscription>()
    .AddInMemorySubscriptions()
    .AddFiltering()
    .AddSorting()
    .AddProjections()
    .AddAuthorization();



var app = builder.Build();

// ---- Seed the database with initial data

// Create a temporary scope and automatically dispose of it when finished
using (var scope = app.Services.CreateScope())
{
    // Retrieve the database context from the dependency injection container
    var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

    // Insert initial data into the database
    await AppDbContextSeeder.SeedAsync(dbContext);
}

app.UseCors("JobBoardClient");

app.UseAuthentication();
app.UseAuthorization();

app.UseWebSockets();
app.MapGraphQL();


app.Run();
