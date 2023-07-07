using Microsoft.EntityFrameworkCore;
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
builder.Services.AddScoped<IIssueServices, IssueServices>();

builder.Services.AddDbContext<PostgreSQLDbContext>(o => o.UseNpgsql(builder.Configuration.GetConnectionString("PostgreSQLConnection")));
builder.Services.AddScoped<DbContext>(provider => provider.GetService<PostgreSQLDbContext>());
var app = builder.Build();

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
Console.WriteLine($"Current Environment: {environment}");

app.Run($"https://localhost:{myPort.Value}");