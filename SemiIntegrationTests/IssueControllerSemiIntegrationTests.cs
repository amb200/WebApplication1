using Azure.Core;
using FluentAssertions;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.IdentityModel.Tokens;
using Moq;
using Newtonsoft.Json;
using System.Net;
using System.Net.Http.Formatting;
using System.Text;
using WebApplication1;
using WebApplication1.Data;
using WebApplication1.Entities;
using WebApplication1.Models;
using WebApplication1.Services;

namespace SemiIntegrationTests
{
    internal class SemiIntegrationTests 
    {
        //private WebApplicationFactory<Program> _factory;
        private TestServer _server;
        private HttpClient _client;

        public void Initialize(string dbName)
        {

            var builder = new WebHostBuilder()
        .ConfigureServices(services =>
        {
            services.AddDbContext<PostgreSQLDbContext>(options =>
            {
                options.UseInMemoryDatabase(dbName);
            });
            services.AddControllers();

            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                // Set JWT token validation parameters, such as valid issuers, audiences, and the signing key.
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes("ThisismySecretKey")),
                    ValidateIssuer = true,
                    ValidIssuer = "Test.com",
                    ValidateAudience = false, // Assuming there is no specific audience validation
                    ValidateLifetime = false
                };
            });

            services.AddAuthorization(options =>
            {
                options.AddPolicy("UserPolicy", policy => policy.RequireClaim("IsUser"));
                options.AddPolicy("ServicePolicy", policy => policy.RequireClaim("IsService"));
            });

            services.AddAutoMapper(typeof(IssueMappingProfile));
            services.AddScoped<IIssueServices, MockIssueServices>();
        })
        .Configure(app =>
        {
            app.UseRouting();
            app.UseAuthorization();
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        });

            _server = new TestServer(builder); // Create the TestServer

            _client = _server.CreateClient(); // Create the HttpClient
        }

        [TearDown]
        public void Cleanup()
        {
            // Dispose the test server and client
            _client.Dispose();
            _server.Dispose();
        }


        [Test]
        public async Task Get_ReturnsAllIssues()
        {
            // Arrange
            Initialize("Get_ReturnsAllIssues");
            var request = new HttpRequestMessage(HttpMethod.Get, "/api/issue");

            request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJ1bmlxdWVfbmFtZSI6InN0cmluZyIsInJvbGUiOiJBZG1pbiIsIlRva2VuSWRlbnRpZmllciI6ImQyMTgzY2FjLWM3ZTAtNDAwZC1iOGQ1LTA1Mzg4ZjNhODc3OSIsIklzVXNlciI6IlRydWUiLCJuYmYiOjE2OTAyNzY2ODAsImV4cCI6MjY5MDM2MzA4MCwiaWF0IjoxNjkwMjc2NjgwLCJpc3MiOiJUZXN0LmNvbSJ9.SlcuJ75VIQct3iQOsLLaaP3nB-GIqGLqoVoc2D96CXk");

            // Act
            var response = await _client.SendAsync(request);

            // Assert
            response.EnsureSuccessStatusCode(); // Ensure the response has a 2xx status code

            var content = await response.Content.ReadAsStringAsync();
            var issues = JsonConvert.DeserializeObject<IEnumerable<Issue>>(content);

            Assert.IsNotNull(issues);
            Assert.AreEqual(0, issues.Count()); // Add more specific assertions as needed
        }

        [Test]
        public async Task GetById_ValidId_ReturnsIssueById()
        {
            // Arrange
            Initialize("GetById_ValidId_ReturnsIssueById");
            var issuesUpdate = new List<Issue>
            {
            new Issue {MetricType = "", JsonField = "", MetricValue = 69, TenantId = ""},
            new Issue {MetricType = "", JsonField = "", MetricValue = 69, TenantId = ""}
            };
            var populate = new HttpRequestMessage(HttpMethod.Post, $"/api/issue/");
            populate.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJ1bmlxdWVfbmFtZSI6InN0cmluZyIsInJvbGUiOiJBZG1pbiIsIlRva2VuSWRlbnRpZmllciI6ImQyMTgzY2FjLWM3ZTAtNDAwZC1iOGQ1LTA1Mzg4ZjNhODc3OSIsIklzVXNlciI6IlRydWUiLCJuYmYiOjE2OTAyNzY2ODAsImV4cCI6MjY5MDM2MzA4MCwiaWF0IjoxNjkwMjc2NjgwLCJpc3MiOiJUZXN0LmNvbSJ9.SlcuJ75VIQct3iQOsLLaaP3nB-GIqGLqoVoc2D96CXk");
            populate.Content = new ObjectContent<List<Issue>>(issuesUpdate, new JsonMediaTypeFormatter(), "application/json");
            await _client.SendAsync(populate);

            var request = new HttpRequestMessage(HttpMethod.Get, $"/api/issue/{1}");
            request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJ1bmlxdWVfbmFtZSI6InN0cmluZyIsInJvbGUiOiJBZG1pbiIsIlRva2VuSWRlbnRpZmllciI6ImQyMTgzY2FjLWM3ZTAtNDAwZC1iOGQ1LTA1Mzg4ZjNhODc3OSIsIklzU2VydmljZSI6IlRydWUiLCJuYmYiOjE2OTAyNzY2ODAsImV4cCI6MjY5MDM2MzA4MCwiaWF0IjoxNjkwMjc2NjgwLCJpc3MiOiJUZXN0LmNvbSJ9.mzcI80MHrym6x3ypgaJ_VrX5blY7BLldmCHL71NyC5Q");

            // Act
            var response = await _client.SendAsync(request);

            // Assert
            response.EnsureSuccessStatusCode(); // Ensure the response has a 2xx status code

            var content = await response.Content.ReadAsStringAsync();
            var issuesTest = JsonConvert.DeserializeObject<Issue>(content);

            Assert.IsNotNull(issuesTest);
        }
        [Test]
        public async Task GetById_InvalidId_ReturnsNotFound()
        {
            // Arrange
            Initialize("GetById_InvalidId_ReturnsNotFound");
            var request = new HttpRequestMessage(HttpMethod.Get, $"/api/issue/{4}");
            request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJ1bmlxdWVfbmFtZSI6InN0cmluZyIsInJvbGUiOiJBZG1pbiIsIlRva2VuSWRlbnRpZmllciI6ImQyMTgzY2FjLWM3ZTAtNDAwZC1iOGQ1LTA1Mzg4ZjNhODc3OSIsIklzU2VydmljZSI6IlRydWUiLCJuYmYiOjE2OTAyNzY2ODAsImV4cCI6MjY5MDM2MzA4MCwiaWF0IjoxNjkwMjc2NjgwLCJpc3MiOiJUZXN0LmNvbSJ9.mzcI80MHrym6x3ypgaJ_VrX5blY7BLldmCHL71NyC5Q");

            // Act
            var response = await _client.SendAsync(request);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.NotFound); ; // Ensure the response has a Not Found status code
        }

        [Test]
        public async Task Create_ValidIssues_Returns201Created()
        {
            // Arrange
            Initialize("Create_ValidIssues_Returns201Created");
            _client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJ1bmlxdWVfbmFtZSI6InN0cmluZyIsInJvbGUiOiJBZG1pbiIsIlRva2VuSWRlbnRpZmllciI6ImQyMTgzY2FjLWM3ZTAtNDAwZC1iOGQ1LTA1Mzg4ZjNhODc3OSIsIklzVXNlciI6IlRydWUiLCJuYmYiOjE2OTAyNzY2ODAsImV4cCI6MjY5MDM2MzA4MCwiaWF0IjoxNjkwMjc2NjgwLCJpc3MiOiJUZXN0LmNvbSJ9.SlcuJ75VIQct3iQOsLLaaP3nB-GIqGLqoVoc2D96CXk");
            var issues = new List<IssueInput>
        {
            new IssueInput {MetricType = "Cat", JsonField = "{}", MetricValue = 69, TenantId = "uno"},
            new IssueInput {MetricType = "Dog", JsonField = "{}", MetricValue = 69, TenantId = "dos"}
        };

            var request = new HttpRequestMessage(HttpMethod.Post, "/api/issue");
            request.Content = new ObjectContent<List<IssueInput>>(
                issues,
                new JsonMediaTypeFormatter(),
                "application/json");

            // Act
            var response = await _client.SendAsync(request);
            response.EnsureSuccessStatusCode(); // Ensure the response has a 2xx status code
        }

        [Test]
        public async Task Create_InvalidIssues_ReturnsBadRequest()
        {
            // Arrange
            Initialize("Create_InvalidIssues_ReturnsBadRequest");
            _client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJ1bmlxdWVfbmFtZSI6InN0cmluZyIsInJvbGUiOiJBZG1pbiIsIlRva2VuSWRlbnRpZmllciI6ImQyMTgzY2FjLWM3ZTAtNDAwZC1iOGQ1LTA1Mzg4ZjNhODc3OSIsIklzVXNlciI6IlRydWUiLCJuYmYiOjE2OTAyNzY2ODAsImV4cCI6MjY5MDM2MzA4MCwiaWF0IjoxNjkwMjc2NjgwLCJpc3MiOiJUZXN0LmNvbSJ9.SlcuJ75VIQct3iQOsLLaaP3nB-GIqGLqoVoc2D96CXk");
            var issues = new List<IssueInput>
            {
            };

            var request = new HttpRequestMessage(HttpMethod.Post, "/api/issue");
            request.Content = new ObjectContent<List<IssueInput>>(
                issues,
                new JsonMediaTypeFormatter(),
                "application/json");

            // Act
            var response = await _client.SendAsync(request);
            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest); // Ensure the response has a Bad Request status code
        }

        [Test]
        public async Task Update_ValidIssues_Returns201Created()
        {
            // Arrange
            Initialize("Update_ValidIssues_ReturnsNoContent");
            _client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJ1bmlxdWVfbmFtZSI6InN0cmluZyIsInJvbGUiOiJBZG1pbiIsIlRva2VuSWRlbnRpZmllciI6ImQyMTgzY2FjLWM3ZTAtNDAwZC1iOGQ1LTA1Mzg4ZjNhODc3OSIsIklzU2VydmljZSI6IlRydWUiLCJuYmYiOjE2OTAyNzY2ODAsImV4cCI6MjY5MDM2MzA4MCwiaWF0IjoxNjkwMjc2NjgwLCJpc3MiOiJUZXN0LmNvbSJ9.mzcI80MHrym6x3ypgaJ_VrX5blY7BLldmCHL71NyC5Q");
            var issuesInitial = new List<Issue>
            {
            new Issue {MetricType = "", JsonField = "", MetricValue = 69, TenantId = ""},
            new Issue {MetricType = "", JsonField = "", MetricValue = 69, TenantId = ""}
            };
            var populate = new HttpRequestMessage(HttpMethod.Post, $"/api/issue/");
            populate.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJ1bmlxdWVfbmFtZSI6InN0cmluZyIsInJvbGUiOiJBZG1pbiIsIlRva2VuSWRlbnRpZmllciI6ImQyMTgzY2FjLWM3ZTAtNDAwZC1iOGQ1LTA1Mzg4ZjNhODc3OSIsIklzVXNlciI6IlRydWUiLCJuYmYiOjE2OTAyNzY2ODAsImV4cCI6MjY5MDM2MzA4MCwiaWF0IjoxNjkwMjc2NjgwLCJpc3MiOiJUZXN0LmNvbSJ9.SlcuJ75VIQct3iQOsLLaaP3nB-GIqGLqoVoc2D96CXk");
            populate.Content = new ObjectContent<List<Issue>>(issuesInitial, new JsonMediaTypeFormatter(), "application/json");
            await _client.SendAsync(populate);

            var issuesUpdate = new List<Issue>
            {
            new Issue {MetricType = "Kitten", JsonField = "{}", MetricValue = 420, TenantId = "tres", EventId = 1},
            new Issue {MetricType = "Pupper", JsonField = "{}", MetricValue = 420, TenantId = "quattro", EventId = 2}
            };
            //Act
            var response = await _client.PutAsync("/api/issue", new ObjectContent<List<Issue>>(
       issuesUpdate,
       new JsonMediaTypeFormatter(),
       "application/json"
   ));
            //Assert
            response.EnsureSuccessStatusCode(); // Ensure the response has a 2xx status code
        }

        [Test]
        public async Task Update_InvalidIssues_ReturnsBadRequest()
        {
            // Arrange
            Initialize("Update_InvalidIssues_ReturnsBadRequest");
            _client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJ1bmlxdWVfbmFtZSI6InN0cmluZyIsInJvbGUiOiJBZG1pbiIsIlRva2VuSWRlbnRpZmllciI6ImQyMTgzY2FjLWM3ZTAtNDAwZC1iOGQ1LTA1Mzg4ZjNhODc3OSIsIklzU2VydmljZSI6IlRydWUiLCJuYmYiOjE2OTAyNzY2ODAsImV4cCI6MjY5MDM2MzA4MCwiaWF0IjoxNjkwMjc2NjgwLCJpc3MiOiJUZXN0LmNvbSJ9.mzcI80MHrym6x3ypgaJ_VrX5blY7BLldmCHL71NyC5Q");

            var issuesUpdate = new List<Issue>
            {
            new Issue {},
            new Issue {}
            };
            //Act
            var response = await _client.PutAsync("/api/issue", new ObjectContent<List<Issue>>(
       issuesUpdate,
       new JsonMediaTypeFormatter(),
       "application/json"
   ));
            //Assert
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest); ; // Ensure the response has a Bad Request status code
        }

        [Test]
        public async Task Delete_ValidIssues_Returns201Created()
        {
            // Arrange
            Initialize("Delete_ValidIssues_ReturnsNoContent");
            
            var issuesUpdate = new List<Issue>
            {
            new Issue {MetricType = "", JsonField = "", MetricValue = 69, TenantId = ""},
            new Issue {MetricType = "", JsonField = "", MetricValue = 69, TenantId = ""}
            };
            var populate = new HttpRequestMessage(HttpMethod.Post, $"/api/issue/");
            populate.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJ1bmlxdWVfbmFtZSI6InN0cmluZyIsInJvbGUiOiJBZG1pbiIsIlRva2VuSWRlbnRpZmllciI6ImQyMTgzY2FjLWM3ZTAtNDAwZC1iOGQ1LTA1Mzg4ZjNhODc3OSIsIklzVXNlciI6IlRydWUiLCJuYmYiOjE2OTAyNzY2ODAsImV4cCI6MjY5MDM2MzA4MCwiaWF0IjoxNjkwMjc2NjgwLCJpc3MiOiJUZXN0LmNvbSJ9.SlcuJ75VIQct3iQOsLLaaP3nB-GIqGLqoVoc2D96CXk");

            populate.Content = new ObjectContent<List<Issue>>(issuesUpdate, new JsonMediaTypeFormatter(), "application/json");
            await _client.SendAsync( populate );

            List<int> ints = new List<int> { 1, 2 };
            var requestDelete = new HttpRequestMessage(HttpMethod.Delete, $"/api/issue/");
            requestDelete.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJ1bmlxdWVfbmFtZSI6InN0cmluZyIsInJvbGUiOiJBZG1pbiIsIlRva2VuSWRlbnRpZmllciI6ImQyMTgzY2FjLWM3ZTAtNDAwZC1iOGQ1LTA1Mzg4ZjNhODc3OSIsIklzU2VydmljZSI6IlRydWUiLCJuYmYiOjE2OTAyNzY2ODAsImV4cCI6MjY5MDM2MzA4MCwiaWF0IjoxNjkwMjc2NjgwLCJpc3MiOiJUZXN0LmNvbSJ9.mzcI80MHrym6x3ypgaJ_VrX5blY7BLldmCHL71NyC5Q");
            requestDelete.Content = new ObjectContent<List<int>>(ints, new JsonMediaTypeFormatter(), "application/json");


            // Act
            var response = await _client.SendAsync(requestDelete);

            //Assert
            response.EnsureSuccessStatusCode(); // Ensure the response has a 2xx status code

        }
        [Test]
        public async Task Delete_InvalidIssues_ReturnsNotFound()
        {
            // Arrange
            Initialize("Delete_InvalidIssues_ReturnsNotFound");
            _client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJ1bmlxdWVfbmFtZSI6InN0cmluZyIsInJvbGUiOiJBZG1pbiIsIlRva2VuSWRlbnRpZmllciI6ImQyMTgzY2FjLWM3ZTAtNDAwZC1iOGQ1LTA1Mzg4ZjNhODc3OSIsIklzU2VydmljZSI6IlRydWUiLCJuYmYiOjE2OTAyNzY2ODAsImV4cCI6MjY5MDM2MzA4MCwiaWF0IjoxNjkwMjc2NjgwLCJpc3MiOiJUZXN0LmNvbSJ9.mzcI80MHrym6x3ypgaJ_VrX5blY7BLldmCHL71NyC5Q");

            List<int> ids = new List<int> { 4 }; // ID that doesn't exist in the database
            var requestDelete = new HttpRequestMessage(HttpMethod.Delete, "/api/issue");

            requestDelete.Content = new ObjectContent<List<int>>(
                ids,
                new JsonMediaTypeFormatter(),
                "application/json");

            // Act
            var response = await _client.SendAsync(requestDelete);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        [Test]
        public async Task UpdateByIds_ValidIds_ReturnsNoContent()
        {
            // Arrange
            Initialize("UpdateByIds_ValidIds_Returns201Created");
            _client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJ1bmlxdWVfbmFtZSI6InN0cmluZyIsInJvbGUiOiJBZG1pbiIsIlRva2VuSWRlbnRpZmllciI6ImQyMTgzY2FjLWM3ZTAtNDAwZC1iOGQ1LTA1Mzg4ZjNhODc3OSIsIklzU2VydmljZSI6IlRydWUiLCJuYmYiOjE2OTAyNzY2ODAsImV4cCI6MjY5MDM2MzA4MCwiaWF0IjoxNjkwMjc2NjgwLCJpc3MiOiJUZXN0LmNvbSJ9.mzcI80MHrym6x3ypgaJ_VrX5blY7BLldmCHL71NyC5Q");
            var issuesUpdate = new List<Issue>
            {
            new Issue {MetricType = "", JsonField = "", MetricValue = 69, TenantId = ""},
            new Issue {MetricType = "", JsonField = "", MetricValue = 69, TenantId = ""}
            };
            var populate = new HttpRequestMessage(HttpMethod.Post, $"/api/issue/");
            populate.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJ1bmlxdWVfbmFtZSI6InN0cmluZyIsInJvbGUiOiJBZG1pbiIsIlRva2VuSWRlbnRpZmllciI6ImQyMTgzY2FjLWM3ZTAtNDAwZC1iOGQ1LTA1Mzg4ZjNhODc3OSIsIklzVXNlciI6IlRydWUiLCJuYmYiOjE2OTAyNzY2ODAsImV4cCI6MjY5MDM2MzA4MCwiaWF0IjoxNjkwMjc2NjgwLCJpc3MiOiJUZXN0LmNvbSJ9.SlcuJ75VIQct3iQOsLLaaP3nB-GIqGLqoVoc2D96CXk");
            populate.Content = new ObjectContent<List<Issue>>(issuesUpdate, new JsonMediaTypeFormatter(), "application/json");
            await _client.SendAsync(populate);


            var requestUpdate = new HttpRequestMessage(HttpMethod.Put, "/api/issue/UpdateByIds?ids=1");
            requestUpdate.Content = new StringContent(
       JsonConvert.SerializeObject(new IssueBulkUpdateInput()),
       Encoding.UTF8,
       "application/json");
            //Act
            var response = await _client.SendAsync(requestUpdate);
            //Assert
            response.StatusCode.Should().Be(HttpStatusCode.Created);
        }

        [Test]  
        public async Task UpdateByIds_InvalidIds_ReturnsNotFound()
        {
            // Arrange
            Initialize("UpdateByIds_InvalidIds_ReturnsNotFound");
            _client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJ1bmlxdWVfbmFtZSI6InN0cmluZyIsInJvbGUiOiJBZG1pbiIsIlRva2VuSWRlbnRpZmllciI6ImQyMTgzY2FjLWM3ZTAtNDAwZC1iOGQ1LTA1Mzg4ZjNhODc3OSIsIklzU2VydmljZSI6IlRydWUiLCJuYmYiOjE2OTAyNzY2ODAsImV4cCI6MjY5MDM2MzA4MCwiaWF0IjoxNjkwMjc2NjgwLCJpc3MiOiJUZXN0LmNvbSJ9.mzcI80MHrym6x3ypgaJ_VrX5blY7BLldmCHL71NyC5Q");

            var requestUpdate = new HttpRequestMessage(HttpMethod.Put, "/api/issue/UpdateByIds?ids=4");
            requestUpdate.Content = new StringContent(
       JsonConvert.SerializeObject(new IssueBulkUpdateInput()),
       Encoding.UTF8,
       "application/json");
            //Act
            var response = await _client.SendAsync(requestUpdate);
            //Assert
            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }


    }


}
