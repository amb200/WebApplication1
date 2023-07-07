using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Transactions;
using WebApplication1.Entities;
using WebApplication1.Data;
using Microsoft.Extensions.DependencyInjection;
using FluentAssertions;
using System.Net;

namespace IntegrationTests
{
    internal class IssueControllerIntegrationTests
    {


        private Process _appProcess;
        private Issue lastIssueEventId;
        private Issue lastIssueEventId2;

        [SetUp]
        public void Setup()
        {
            // Start the ASP.NET app server
            _appProcess = StartAppServer();


        }
        public async Task SetupHelper()
        {
            List<Issue> issues = new List<Issue>
                {
                    new Issue{MetricType = "test", EventId = 1, JsonField = "", MetricValue = 7.7, TenantId = ""},
                    new Issue{MetricType = "test", EventId = 1, JsonField = "", MetricValue = 7.7, TenantId = ""}
                };
            var httpClient = new HttpClient();
            httpClient.BaseAddress = new Uri("https://localhost:6941");

            var populate = new HttpRequestMessage(HttpMethod.Post, "/api/issue");
            populate.Content = new ObjectContent<List<Issue>>(issues, new JsonMediaTypeFormatter(), "application/json");
            await httpClient.SendAsync(populate);

            var request = new HttpRequestMessage(HttpMethod.Get, "/api/issue");
            var response = await httpClient.SendAsync(request);

            var json = await response.Content.ReadAsStringAsync();

            // Deserialize JSON into a list of objects
            var list = Newtonsoft.Json.JsonConvert.DeserializeObject<List<Issue>>(json);

            // Find the object with the highest EventId value
            lastIssueEventId = list.OrderByDescending(x => x.EventId).FirstOrDefault();
            lastIssueEventId2 = list.OrderByDescending(x => x.EventId).Skip(1).FirstOrDefault();
        }
        public async Task TearDownHelper()
        {
            var httpClient = new HttpClient();
            httpClient.BaseAddress = new Uri("https://localhost:6941");
            List<int> ints = new List<int> { lastIssueEventId.EventId, lastIssueEventId2.EventId };
            var requestDelete = new HttpRequestMessage(HttpMethod.Delete, $"/api/issue/");
            requestDelete.Content = new ObjectContent<List<int>>(ints, new JsonMediaTypeFormatter(), "application/json");
            await httpClient.SendAsync(requestDelete);
        }

        private Process StartAppServer()
        {

            string appPath = Path.GetFullPath(@"C:\Users\PC\source\repos\WebApplication1\ConsoleRun\ConsoleRun.csproj");

            var processStartInfo = new ProcessStartInfo
            {
                FileName = "dotnet",
                Arguments = $"run --project \"{appPath}\" ",
                RedirectStandardOutput = true,
                UseShellExecute = false,
                CreateNoWindow = false,
                Environment =
                {
                    { "ASPNETCORE_ENVIRONMENT", "IntegrationTest" },
                    { "Port", "6941" }

                }
            };

            return Process.Start(processStartInfo);
        }


        [TearDown]
        public void Cleanup()
        {
            // Stop the ASP.NET app server
            _appProcess?.Kill();
            _appProcess?.WaitForExit();
            _appProcess?.Dispose();
        }
        [Test]
        public async Task Get_Returns201Success()
        {
            //Arrange
            var httpClient = new HttpClient();
            httpClient.BaseAddress = new Uri("https://localhost:6941");
            var request = new HttpRequestMessage(HttpMethod.Get, "/api/issue");
            // Act
            var response = await httpClient.SendAsync(request);
            // Assert
            response.EnsureSuccessStatusCode(); // Ensure the response has a 2xx status code
        }
        [Test]
        public async Task GetById_ValidIds_Returns201Success()
        {
            await SetupHelper();

            // Arrange
            var httpClient = new HttpClient();
            httpClient.BaseAddress = new Uri("https://localhost:6941");

            var request = new HttpRequestMessage(HttpMethod.Get, $"/api/issue/{lastIssueEventId.EventId}");

            // Act
            var response = await httpClient.SendAsync(request);
            // Assert
            response.EnsureSuccessStatusCode(); // Ensure the response has a 2xx status code
            await TearDownHelper();

        }
        [Test]
        public async Task GetById_InvalidIds_Returns404NotFound()
        {
            await SetupHelper();

            // Arrange
            var httpClient = new HttpClient();
            httpClient.BaseAddress = new Uri("https://localhost:6941");

            var request = new HttpRequestMessage(HttpMethod.Get, $"/api/issue/{-1}");

            // Act
            var response = await httpClient.SendAsync(request);
            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.NotFound); 
            await TearDownHelper();

        }
        [Test]
        public async Task Create_ValidIds_Returns201Success()
        {
            await SetupHelper();

            // Arrange
            var httpClient = new HttpClient();
            httpClient.BaseAddress = new Uri("https://localhost:6941");

            // Act
            
            // Assert
       
            await TearDownHelper();
        }
        //[Test]
        public async Task Create_InvalidIds_Returns400BadRequest()
        {
            await SetupHelper();

            // Arrange
            var httpClient = new HttpClient();
            httpClient.BaseAddress = new Uri("https://localhost:6941");

            // Act

            // Assert

            await TearDownHelper();
        }
        //[Test]
        public async Task Update_ValidIds_Returns204NoContent()
        {
            await SetupHelper();

            // Arrange
            var httpClient = new HttpClient();
            httpClient.BaseAddress = new Uri("https://localhost:6941");

            // Act

            // Assert

            await TearDownHelper();
        }
        //[Test]
        public async Task Update_InvalidIds_Returns400BadRequest()
        {
            await SetupHelper();

            // Arrange
            var httpClient = new HttpClient();
            httpClient.BaseAddress = new Uri("https://localhost:6941");

            // Act

            // Assert

            await TearDownHelper();
        }
        //[Test]
        public async Task Delete_ValidIds_Returns204NoContent()
        {
            await SetupHelper();

            // Arrange
            var httpClient = new HttpClient();
            httpClient.BaseAddress = new Uri("https://localhost:6941");

            // Act

            // Assert

            await TearDownHelper();
        }
        //[Test]
        public async Task Delete_InvalidIds_Returns404NotFound()
        {
            await SetupHelper();

            // Arrange
            var httpClient = new HttpClient();
            httpClient.BaseAddress = new Uri("https://localhost:6941");

            // Act

            // Assert

            await TearDownHelper();
        }

    }
}
