using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using Amazon.Extensions.NETCore.Setup;
using FluentAssertions.Common;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Text;
using WebApplication1;
using WebApplication1.Data;
using WebApplication1.Services;

[assembly: InternalsVisibleTo("SemiIntegrationTests")]
internal class Program
{
    [ExcludeFromCodeCoverage]
    private static  void Main(string[] args)
    {

        var builder = WebApplication.CreateBuilder(args);

        // Add services to the container.
        builder.Services.AddControllers();
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen(c =>
        {
            // Add the security definition
            c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            {
                Name = "Authorization",
                Description = "Enter the Bearer Authorization string as following: `Bearer Generated-JWT-Token`",
                In = ParameterLocation.Header,
                Type = SecuritySchemeType.ApiKey,
                Scheme = "Bearer"
            });

            // Add the security requirement
            c.AddSecurityRequirement(new OpenApiSecurityRequirement
            {
                {
                    new OpenApiSecurityScheme
                    {
                        Reference = new OpenApiReference
                        {
                            Type = ReferenceType.SecurityScheme,
                            Id = "Bearer"
                        }
                    },
                    new List<string>()
                }
            });
        });
        builder.Services.AddAutoMapper(typeof(IssueMappingProfile));
        //builder.Services.AddScoped<IIssueServices, IssueServices>();
        
        // Add authentication services
        builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                // Set JWT token validation parameters, such as valid issuers, audiences, and the signing key.
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(builder.Configuration["Jwt:SecretKey"])),
                    ValidateIssuer = true,
                    ValidIssuer = builder.Configuration["Jwt:Issuer"],
                    ValidateAudience = false, // Assuming there is no specific audience validation
                    ValidateLifetime = true
                };
            });

        builder.Services.AddAuthorization();

        builder.Services.AddHttpClient();




        string databaseProvider = "3";

        // Configure the DbContext based on the database provider
        switch (databaseProvider)
        {
            case "1": // PostgreSQL
                builder.Services.AddDbContext<PostgreSQLDbContext>(o => o.UseNpgsql(builder.Configuration.GetConnectionString("PostgreSQLConnection")));
                builder.Services.AddScoped<DbContext>(provider => provider.GetService<PostgreSQLDbContext>());
                builder.Services.AddScoped(typeof(IIssueServices), typeof(IssueServices<PostgreSQLDbContext>));
                break;
            case "2": // SQL Server
                builder.Services.AddDbContext<SQLServerDbContext>(o => o.UseSqlServer(builder.Configuration.GetConnectionString("SQLServerConnection")));
                builder.Services.AddScoped<DbContext>(provider => provider.GetService<SQLServerDbContext>());
                builder.Services.AddScoped(typeof(IIssueServices), typeof(IssueServices<SQLServerDbContext>));
                break;
            case "3": // DynamoDb
                
                builder.Services.AddAWSService<IAmazonDynamoDB>(new AWSOptions
                {
                    DefaultClientConfig =
                    {
                        ServiceURL = "http://localhost:8000"
                    }
                });
                builder.Services.AddDbContext<DbContext>();
                // Register DynamoDBContext as scoped service
                builder.Services.AddScoped<IDynamoDBContext, DynamoDBContext>(provider =>
                {
                    var dynamoDbClient = provider.GetRequiredService<IAmazonDynamoDB>();
                    return new DynamoDBContext(dynamoDbClient, new DynamoDBContextConfig());
                });

                // Register IssueServices with DynamoDBContext
                builder.Services.AddScoped(typeof(IIssueServices), typeof(IssueServices<IDynamoDBContext>));
                break;
            default:
                throw new Exception("Invalid database provider specified in configuration.");
        }




        var app = builder.Build();

        // Configure the HTTP request pipeline.
        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }


        app.UseHttpsRedirection();
        app.UseAuthentication();
        app.UseAuthorization();
        app.MapControllers();

        // Automatic migrations
        switch (databaseProvider)
        {
            case "1": // PostgreSQL
                using (var scope = app.Services.CreateScope())
                {
                    var serviceProvider = scope.ServiceProvider;
                    var dbContext = serviceProvider.GetRequiredService<PostgreSQLDbContext>();
                    // Apply migrations for the current database provider
                    if (dbContext.Database.IsRelational())
                    {
                        dbContext.Database.Migrate();
                    }
                }
                break;
            case "2": // SQL Server
                using (var scope = app.Services.CreateScope())
                {
                    var serviceProvider = scope.ServiceProvider;
                    var dbContext = serviceProvider.GetRequiredService<SQLServerDbContext>();
                    // Apply migrations for the current database provider
                    if (dbContext.Database.IsRelational())
                    {
                        dbContext.Database.Migrate();
                    }
                }
                break;
            case "3": // DynamoDb

                
                break;
            default:
                throw new Exception("Invalid database provider specified in configuration.");
        }



        app.Run();

    }
}