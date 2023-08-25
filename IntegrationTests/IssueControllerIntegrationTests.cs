using FluentAssertions;
using Microsoft.Extensions.Caching.Memory;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Polly;
using System.Diagnostics;
using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Net.Http.Formatting;
using System.Text;
using WebApplication1.Entities;
using WebApplication1.JWTAuthentication;
using WebApplication1.Models;

namespace IntegrationTests
{
    internal class IssueControllerIntegrationTests
    {


        private Process _appProcess;
        private Issue lastIssueEventId;
        private Issue lastIssueEventId2;
        private string tokenUser;
        private string tokenService;

        [SetUp]
        public void Setup()
        {
            // Start the ASP.NET app server
            tokenUser = GetOrGenerateToken(0).Result;
            tokenService = GetOrGenerateToken(1).Result;
            _appProcess = StartAppServer();
            StartAppServer();

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
        public async Task SetupHelper()
        {
            var httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", tokenUser);
            httpClient.BaseAddress = new Uri("https://localhost:6941");
            List<Issue> issues = new List<Issue>
                {
                    new Issue{MetricType = "test", JsonField = "", MetricValue = 7.7, TenantId = ""},
                    new Issue{MetricType = "test", JsonField = "", MetricValue = 7.7, TenantId = ""}
                };

            var populate = new HttpRequestMessage(HttpMethod.Post, "/api/issue");
            populate.Content = new ObjectContent<List<Issue>>(issues, new JsonMediaTypeFormatter(), "application/json");
            await httpClient.SendAsync(populate);

            var request = new HttpRequestMessage(HttpMethod.Get, "/api/issue");
            var response = await httpClient.SendAsync(request);

            var json = await response.Content.ReadAsStringAsync();

            // Deserialize JSON into a list of objects
            var list = JsonConvert.DeserializeObject<List<Issue>>(json);

            // Find the object with the highest EventId value
            lastIssueEventId = list.OrderByDescending(x => x.EventId).FirstOrDefault();
            lastIssueEventId2 = list.OrderByDescending(x => x.EventId).Skip(1).FirstOrDefault();
        }
        public async Task TearDownHelper()
        {
            var httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", tokenUser);
            httpClient.BaseAddress = new Uri("https://localhost:6941");
            List<int> ints = new List<int> { lastIssueEventId.EventId, lastIssueEventId2.EventId };
            var requestDelete = new HttpRequestMessage(HttpMethod.Delete, $"/api/issue/");
            requestDelete.Content = new ObjectContent<List<int>>(ints, new JsonMediaTypeFormatter(), "application/json");
            await httpClient.SendAsync(requestDelete);
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
            httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", tokenUser);
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
            httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", tokenUser);
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
            httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", tokenUser);
            httpClient.BaseAddress = new Uri("https://localhost:6941");

            var request = new HttpRequestMessage(HttpMethod.Get, $"/api/issue/{-1}");

            // Act
            var response = await httpClient.SendAsync(request);
            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.NotFound); 
            await TearDownHelper();

        }
        [Test]
        public async Task Create_ValidIssues_Returns201Success()
        {
            //arrange
            var httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", tokenUser);
            httpClient.BaseAddress = new Uri("https://localhost:6941");
            List<Issue> issues = new List<Issue>
                {
                    new Issue{MetricType = "test", JsonField = "", MetricValue = 7.7, TenantId = ""},
                    new Issue{MetricType = "test", JsonField = "", MetricValue = 7.7, TenantId = ""}
                };

            var populate = new HttpRequestMessage(HttpMethod.Post, "/api/issue");
            populate.Content = new ObjectContent<List<Issue>>(issues, new JsonMediaTypeFormatter(), "application/json");

            //Act
            var evaluate = await httpClient.SendAsync(populate);

            // Deserialize JSON into a list of objects
            var request = new HttpRequestMessage(HttpMethod.Get, "/api/issue");
            var response = await httpClient.SendAsync(request);
            var json = await response.Content.ReadAsStringAsync();
            var list = JsonConvert.DeserializeObject<List<Issue>>(json);

            // Find the object with the highest EventId value
            lastIssueEventId = list.OrderByDescending(x => x.EventId).FirstOrDefault();
            lastIssueEventId2 = list.OrderByDescending(x => x.EventId).Skip(1).FirstOrDefault();

            // Assert
            evaluate.EnsureSuccessStatusCode(); 

            List<int> ints = new List<int> { lastIssueEventId.EventId, lastIssueEventId2.EventId };
            var requestDelete = new HttpRequestMessage(HttpMethod.Delete, $"/api/issue/");
            requestDelete.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", tokenService);
            requestDelete.Content = new ObjectContent<List<int>>(ints, new JsonMediaTypeFormatter(), "application/json");
            await httpClient.SendAsync(requestDelete);

        }
        [Test]
        public async Task Create_InvalidIssues_Returns400BadRequest()
        {
            // Arrange
            var httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", tokenUser);
            httpClient.BaseAddress = new Uri("https://localhost:6941");
            List<Issue> issues = new List<Issue>
                {
                };

            var populate = new HttpRequestMessage(HttpMethod.Post, "/api/issue");
            populate.Content = new ObjectContent<List<Issue>>(issues, new JsonMediaTypeFormatter(), "application/json");

            // Act
            var response = await httpClient.SendAsync(populate);
            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        }
        [Test]
        public async Task Update_ValidIds_Returns204NoContent()
        {
            await SetupHelper();

            // Arrange
            var httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", tokenService);
            httpClient.BaseAddress = new Uri("https://localhost:6941");
            List<Issue> issues = new List<Issue>
                {
                    new Issue{MetricType = "Update", JsonField = "", MetricValue = 7.7, TenantId = "", EventId = lastIssueEventId.EventId},
                    new Issue{MetricType = "Update", JsonField = "", MetricValue = 7.7, TenantId = "", EventId = lastIssueEventId2.EventId}
                };

            var populate = new HttpRequestMessage(HttpMethod.Put, "/api/issue");
            populate.Content = new ObjectContent<List<Issue>>(issues, new JsonMediaTypeFormatter(), "application/json");

            // Act
            var response = await httpClient.SendAsync(populate);
            // Assert
            response.EnsureSuccessStatusCode();
            await TearDownHelper();
        }
        [Test]
        public async Task Update_InvalidIds_Returns400BadRequest()
        {
            await SetupHelper();
            // Arrange
            var httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", tokenService);
            httpClient.BaseAddress = new Uri("https://localhost:6941");
            List<Issue> issues = new List<Issue>
                {
                new Issue{MetricType = "Update", JsonField = "", MetricValue = 7.7, TenantId = "", EventId = lastIssueEventId.EventId+9},
                };

            var populate = new HttpRequestMessage(HttpMethod.Put, "/api/issue");
            populate.Content = new ObjectContent<List<Issue>>(issues, new JsonMediaTypeFormatter(), "application/json");

            // Act
            var response = await httpClient.SendAsync(populate);
            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
            await TearDownHelper();

        }
        [Test]
        public async Task Delete_ValidIds_Returns204NoContent()
        {
            await SetupHelper();

            // Arrange
            var httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", tokenService  );
            httpClient.BaseAddress = new Uri("https://localhost:6941");
            List<int> ints = new List<int> { lastIssueEventId.EventId, lastIssueEventId2.EventId };
            var requestDelete = new HttpRequestMessage(HttpMethod.Delete, $"/api/issue/");
            requestDelete.Content = new ObjectContent<List<int>>(ints, new JsonMediaTypeFormatter(), "application/json");
            // Act
            var request = await httpClient.SendAsync(requestDelete);
            // Assert
            request.EnsureSuccessStatusCode();
        }
        [Test]
        public async Task Delete_InvalidIds_Returns404NotFound()
        {
            // Arrange
            var httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", tokenService);
            httpClient.BaseAddress = new Uri("https://localhost:6941");
            List<int> ints = new List<int> { 999};
            var requestDelete = new HttpRequestMessage(HttpMethod.Delete, $"/api/issue/");
            requestDelete.Content = new ObjectContent<List<int>>(ints, new JsonMediaTypeFormatter(), "application/json");
            // Act
            var request = await httpClient.SendAsync(requestDelete);
            // Assert
            request.StatusCode.Should().Be(HttpStatusCode.NotFound);

        }
        [Test]
        public async Task UpdateById_ValidIds_Returns201Created()
        {
            await SetupHelper();

            // Arrange
            var httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", tokenService);
            httpClient.BaseAddress = new Uri("https://localhost:6941");
            var populate = new HttpRequestMessage(HttpMethod.Put, $"/api/issue/UpdateByIds?ids={lastIssueEventId.EventId}");
            populate.Content = new StringContent(JsonConvert.SerializeObject(new IssueBulkUpdateInput { MetricValue = 69.420}),Encoding.UTF8,"application/json");

            // Act
            var response = await httpClient.SendAsync(populate);
            // Assert
            response.EnsureSuccessStatusCode();

            await TearDownHelper();
        }
        [Test]
        public async Task UpdateById_InvalidIds_Returns404NotFound()
        {
            await SetupHelper();
            // Arrange
            var httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", tokenService);
            httpClient.BaseAddress = new Uri("https://localhost:6941");
            var populate = new HttpRequestMessage(HttpMethod.Put, $"/api/issue/UpdateByIds?ids={lastIssueEventId.EventId+4}");
            populate.Content = new StringContent(JsonConvert.SerializeObject(new IssueBulkUpdateInput { MetricValue = 69.420 }), Encoding.UTF8, "application/json");

            // Act
            var response = await httpClient.SendAsync(populate);
            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
            await TearDownHelper();
        }

        public async Task<string> GetOrGenerateToken(int role)
        {
            // Generate a new token
            var httpClient = new HttpClient();
            httpClient.BaseAddress = new Uri("https://localhost:6941");
            TokenRequestModel tokenRequestModel = new TokenRequestModel { Roles = "String", TokenType = (TokenType)role, Username = "String" };
            HttpResponseMessage response = await httpClient.PostAsJsonAsync("/api/JWTAuth/token", tokenRequestModel);
            var responseContent = await response.Content.ReadAsStringAsync();
            JObject json = JObject.Parse(responseContent);
            string newToken = (string)json["token"];

            var handler = new JwtSecurityTokenHandler();
            var jwtSecurityToken = handler.ReadJwtToken(newToken);
            var tokenExpires = jwtSecurityToken.ValidTo;

            return newToken;
        }
    }

}

