using AutoMapper;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Migrations.Internal;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using System;
using System.Data.Common;
using System.Diagnostics;
using WebApplication1;
using WebApplication1.Data;
using WebApplication1.Models;
using WebApplication1.Services;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;
using Microsoft.SqlServer.TransactSql.ScriptDom;
using Microsoft.SqlServer.Management.SqlParser;
using Microsoft.EntityFrameworkCore.SqlServer.Storage.Internal;
using FluentAssertions.Common;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

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

        //string databaseProvider = "1";

        // Configure the DbContext based on the database provider
 /*       switch (databaseProvider)
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
        }*/

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




        /*// Automatic migrations
        switch (databaseProvider)
        {
            case "1": // PostgreSQL
                using (var scope = app.Services.CreateScope())
                {
                    var serviceProvider = scope.ServiceProvider;
                    var dbContext = serviceProvider.GetRequiredService<PostgreSQLDbContext>();
                    // Apply migrations for the current database provider
                    dbContext.Database.Migrate();
                }
                break;
            case "2": // SQL Server
                using (var scope = app.Services.CreateScope())
                {
                    var serviceProvider = scope.ServiceProvider;
                    var dbContext = serviceProvider.GetRequiredService<SQLServerDbContext>();
                    // Apply migrations for the current database provider
                    dbContext.Database.Migrate();
                }
                break;
            default:
                throw new Exception("Invalid database provider specified in configuration.");
        }
*/


        app.Run();

    }
}