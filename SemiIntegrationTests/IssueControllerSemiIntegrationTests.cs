using Azure;
using FluentAssertions;
using FluentAssertions.Common;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.VisualStudio.TestPlatform.TestHost;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Text;
using System.Threading.Tasks;
using WebApplication1;
using WebApplication1.Data;
using WebApplication1.Entities;
using WebApplication1.Models;

namespace SemiIntegrationTests
{
    internal class SemiIntegrationTests
    {
        private WebApplicationFactory<Program> _factory;
        private HttpClient _client;

        public void Initialize(string dbName)
        {
            // Set up the test server
            _factory = new WebApplicationFactory<Program>()
                .WithWebHostBuilder(builder =>
                {
                    builder.ConfigureServices(services =>
                    {
                        services.AddDbContext<PostgreSQLDbContext>(options => options.UseInMemoryDatabase(dbName));
                        services.AddScoped<DbContext>(provider => provider.GetService<PostgreSQLDbContext>());
                    });
                });

            // Create a client for interacting with the test server
            _client = _factory.CreateClient();
        }

        [TearDown]
        public void Cleanup()
        {
            // Dispose the test server and client
            _client.Dispose();
            _factory.Dispose();

        }


        [Test]
        public async Task Get_ReturnsAllIssues()
        {
            // Arrange
            Initialize("Get_ReturnsAllIssues");
            var request = new HttpRequestMessage(HttpMethod.Get, "/api/issue");

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
            var issues = new List<IssueInput>
        {
            new IssueInput {MetricType = "Cat", JsonField = "{}", MetricValue = 69, TenantId = "uno"},
            new IssueInput {MetricType = "Dog", JsonField = "{}", MetricValue = 69, TenantId = "dos"}
        };
            var request = new HttpRequestMessage(HttpMethod.Get, $"/api/issue/{1}");

            var requestSend = new HttpRequestMessage(HttpMethod.Post, "/api/issue");
            requestSend.Content = new ObjectContent<List<IssueInput>>(
                issues,
                new JsonMediaTypeFormatter(),
                "application/json");

            await _client.SendAsync(requestSend);

            // Act
            var response = await _client.SendAsync(request);

            // Assert
            response.EnsureSuccessStatusCode(); // Ensure the response has a 2xx status code

            var content = await response.Content.ReadAsStringAsync();
            var issuesTest = JsonConvert.DeserializeObject<Issue>(content);

            Assert.IsNotNull(issues);
            Assert.AreEqual(issues[0].MetricValue, issuesTest.MetricValue);
            Assert.AreEqual(issues[0].MetricType, issuesTest.MetricType);
            Assert.AreEqual(issues[0].TenantId, issuesTest.TenantId);
            Assert.AreEqual(issues[0].JsonField, issuesTest.JsonField);
        }
        [Test]
        public async Task GetById_InvalidId_ReturnsIssueById()
        {
            // Arrange
            Initialize("GetById_InvalidId_ReturnsIssueById");
            var request = new HttpRequestMessage(HttpMethod.Get, $"/api/issue/{1}");

            // Act
            var response = await _client.SendAsync(request);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.NotFound); ; // Ensure the response has a Not Found status code
        }

        [Test]
        public async Task Create_ValidIssues_Returns201Created()
        {
            // Arrange
            Initialize("Create_Returns210Created");
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
            // Assert
            response.EnsureSuccessStatusCode(); // Ensure the response has a 2xx status code
        }

        [Test]
        public async Task Create_InvalidIssues_ReturnsBadRequest()
        {
            // Arrange
            Initialize("Create_InvalidIssues_ReturnsBadRequest");
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
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest); ; // Ensure the response has a Bad Request status code
        }

        [Test]
        public async Task Update_ValidIssues_ReturnsNoContent()
        {
            // Arrange
            Initialize("Update_ValidIssues_NoContent");
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
            await _client.SendAsync(request);

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
            await _client.SendAsync(request);

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
        public async Task Delete_ValidIssues_ReturnsNoContent()
        {
            // Arrange
            Initialize("Delete_ValidIssues_ReturnsNoContent");
            var issues = new List<IssueInput>
            {
            new IssueInput {MetricType = "Cat", JsonField = "{}", MetricValue = 69, TenantId = "uno"},
            new IssueInput {MetricType = "Dog", JsonField = "{}", MetricValue = 69, TenantId = "dos"}
            };

            var request = new HttpRequestMessage(HttpMethod.Post, "/api/issue");
            request.Content = new ObjectContent<List<IssueInput>>(issues, new JsonMediaTypeFormatter(), "application/json");
            await _client.SendAsync(request);

            List<int> ints = new List<int> { 1, 2 };
            var requestDelete = new HttpRequestMessage(HttpMethod.Delete, $"/api/issue/");
            requestDelete.Content = new ObjectContent<List<int>>(ints, new JsonMediaTypeFormatter(), "application/json");

            // Act
            var response = await _client.SendAsync(requestDelete);

            //Assert
            response.EnsureSuccessStatusCode(); // Ensure the response has a 2xx status code

        }
        [Test]
        public async Task Delete_InvalidIssues_ReturnsBadRequest()
        {
            // Arrange
            Initialize("Delete_InvalidIssues_ReturnsBadRequest");
            var issues = new List<IssueInput>
    {
        new IssueInput { MetricType = "Cat", JsonField = "{}", MetricValue = 69, TenantId = "uno" },
        new IssueInput { MetricType = "Dog", JsonField = "{}", MetricValue = 69, TenantId = "dos" }
    };

            var request = new HttpRequestMessage(HttpMethod.Post, "/api/issue");
            request.Content = new ObjectContent<List<IssueInput>>(
                issues,
                new JsonMediaTypeFormatter(),
                "application/json");
            await _client.SendAsync(request);

            List<int> ids = new List<int> { 3 }; // ID that doesn't exist in the database
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


    }


}
