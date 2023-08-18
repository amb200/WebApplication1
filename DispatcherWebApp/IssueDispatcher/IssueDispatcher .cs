using AutoMapper;
using Microsoft.Extensions.Caching.Memory;
using Newtonsoft.Json.Linq;
using Polly;
using System.Collections.Concurrent;
using System.IdentityModel.Tokens.Jwt;
using System.Net.Http.Formatting;
using WebApplication1.Entities;
using WebApplication1.JWTAuthentication;


namespace WebApplication1.IssueDispatcher
{
    public class IssueDispatcher : IIssueDispatcher, IAsyncDisposable
    {
        private readonly IConfiguration _configuration;
        private readonly int _batchSize;
        private readonly TimeSpan _retryInterval;
        private readonly IMapper _mapper;
        private readonly IMemoryCache _memoryCache;
        private const string TokenCacheKey = "AuthToken";

        public CancellationTokenSource cts = new CancellationTokenSource();// to kill que process

        private readonly ConcurrentQueue<Issue> _issueQueue = new ConcurrentQueue<Issue>();


        public IssueDispatcher(IConfiguration configuration, IMapper mapper, IMemoryCache memoryCache)
        {
            _configuration = configuration;
            _batchSize = _configuration.GetValue<int>("IssueDispatcher:BatchSize");
            _retryInterval = TimeSpan.FromSeconds(_configuration.GetValue<int>("IssueDispatcher:RetryIntervalSeconds"));
            _mapper = mapper;
            _memoryCache = memoryCache;

            Task.Run(() => ProcessQueueAsync(cts.Token));//to kill que process
        }

        public async ValueTask DisposeAsync()
        {
            cts.Cancel();

            await SendIssuesToDatabaseAsync();
        }



        public async void Send(Issue issue)
        {
            _issueQueue.Enqueue(issue);
        }

        private async Task ProcessQueueAsync(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested) //to kill que process
            {
                if (_issueQueue.Count >= _batchSize)
                {
                    await SendIssuesToDatabaseAsync();
                }

                await Task.Delay(TimeSpan.FromSeconds(1));
            }
        }

        private async Task SendIssuesToDatabaseAsync()
        {
            var httpClient = new HttpClient();
            httpClient.BaseAddress = new Uri("https://localhost:7264");
            string token = GetOrGenerateToken().Result;

            httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            var issuesToSend = new List<Issue>();
            while (issuesToSend.Count < _batchSize && _issueQueue.TryDequeue(out var issue))
            {
                // Separate db input from user input 
                var created = _mapper.Map<Issue>(issue);
                created.EventId = 0;
                created.Timestamp = DateTime.UtcNow;
                issuesToSend.Add(created);
            }

            var populate = new HttpRequestMessage(HttpMethod.Post, "/api/issue");
            populate.Content = new ObjectContent<List<Issue>>(issuesToSend, new JsonMediaTypeFormatter(), "application/json");

            var policy = Policy
                .Handle<HttpRequestException>()
                .WaitAndRetryAsync(3, _ => _retryInterval);

            await policy.ExecuteAsync(async () =>
            {
                await httpClient.SendAsync(populate);
            });

        }

        public async Task<string> GetOrGenerateToken()
        {
            if (_memoryCache.TryGetValue(TokenCacheKey, out string cachedToken))
            {
                // Token found in cache, return it
                return cachedToken;
            }

            // Generate a new token
            var httpClient = new HttpClient();
            httpClient.BaseAddress = new Uri("https://localhost:7264");
            TokenRequestModel tokenRequestModel = new TokenRequestModel { Roles = "String", TokenType = 0, Username = "String" };
            HttpResponseMessage response = await httpClient.PostAsJsonAsync("/api/JWTAuth/token", tokenRequestModel);
            var responseContent = await response.Content.ReadAsStringAsync();
            JObject json = JObject.Parse(responseContent);
            string newToken = (string)json["token"];

            var handler = new JwtSecurityTokenHandler();
            var jwtSecurityToken = handler.ReadJwtToken(newToken);
            var tokenExpires = jwtSecurityToken.ValidTo;

            // Cache the token for Lifetime of token
            var cacheOptions = new MemoryCacheEntryOptions
            {
                AbsoluteExpiration = tokenExpires
            };

            _memoryCache.Set(TokenCacheKey, newToken, cacheOptions);

            return newToken;
        }
    }
}


