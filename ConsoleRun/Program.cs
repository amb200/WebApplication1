﻿using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using Amazon.Extensions.NETCore.Setup;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using WebApplication1;
using WebApplication1.Data;
using WebApplication1.Services;

var builder = WebApplication.CreateBuilder();
var myPort = builder.Configuration.GetSection("Port");

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddAutoMapper(typeof(IssueMappingProfile));
builder.Services.AddScoped(typeof(IIssueServices), typeof(IssueServices<PostgreSQLDbContext>));

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
                    ValidateLifetime = false
                };
            });

builder.Services.AddAuthorization();

builder.Services.AddDbContext<PostgreSQLDbContext>(o => o.UseNpgsql(builder.Configuration.GetConnectionString("PostgreSQLConnection")));
builder.Services.AddScoped<DbContext>(provider => provider.GetService<PostgreSQLDbContext>());

builder.Services.AddAWSService<IAmazonDynamoDB>();
builder.Services.AddScoped<IDynamoDBContext, DynamoDBContext>(provider =>
{
    var dynamoDbClient = provider.GetRequiredService<IAmazonDynamoDB>();
    return new DynamoDBContext(dynamoDbClient, new DynamoDBContextConfig());
});
var app = builder.Build();

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();


app.Run($"https://localhost:{myPort.Value}");