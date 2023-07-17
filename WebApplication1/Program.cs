using FluentAssertions.Common;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.IdentityModel.Tokens;
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
    private static void Main(string[] args)
    {

        var builder = WebApplication.CreateBuilder(args);

        // Add services to the container.
        builder.Services.AddControllers();
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen();
        builder.Services.AddAutoMapper(typeof(IssueMappingProfile));
        builder.Services.AddScoped<IIssueServices, IssueServices>();

        string databaseProvider = "1";

        // Configure the DbContext based on the database provider
        switch (databaseProvider)
        {
            case "1": // PostgreSQL
                builder.Services.AddDbContext<PostgreSQLDbContext>(o => o.UseNpgsql(builder.Configuration.GetConnectionString("PostgreSQLConnection")));
                builder.Services.AddScoped<DbContext>(provider => provider.GetService<PostgreSQLDbContext>());
                break;
            case "2": // SQL Server
                builder.Services.AddDbContext<SQLServerDbContext>(o => o.UseSqlServer(builder.Configuration.GetConnectionString("SQLServerConnection")));
                builder.Services.AddScoped<DbContext>(provider => provider.GetService<SQLServerDbContext>());
                break;
            default:
                throw new Exception("Invalid database provider specified in configuration.");
        }

        var ValidIssuer = builder.Configuration["Jwt:Issuer"];
        builder.Services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
        }).AddJwtBearer(o =>
        {
            o.TokenValidationParameters = new TokenValidationParameters
            {
                ValidIssuer = builder.Configuration["Jwt:Issuer"],
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:SecretKey"])),
                ValidateIssuer = true,
                ValidateAudience = false, // Assuming there is no specific audience validation
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true
            };
        });

        builder.Services.AddAuthorization();


        var app = builder.Build();

        // Configure the HTTP request pipeline.
        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }

        app.UseHttpsRedirection();
        app.UseAuthorization();
        app.MapControllers();

        app.UseAuthentication();
        app.UseAuthorization();



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
            default:
                throw new Exception("Invalid database provider specified in configuration.");
        }



        app.Run();

    }
}