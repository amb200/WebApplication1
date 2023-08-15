using AutoMapper;
using Polly;
using System.Collections.Concurrent;
using System.Net.Http.Formatting;
using WebApplication1.Entities;
using WebApplication1.Services;

namespace WebApplication1.IssueDispatcher
{
    public class IssueDispatcher : IIssueDispatcher
    {
        private readonly IServiceScopeFactory _serviceProvider;
        private readonly IConfiguration _configuration;
        private readonly int _batchSize;
        private readonly TimeSpan _retryInterval;
        private readonly IMapper _mapper;


        private readonly ConcurrentQueue<Issue> _issueQueue = new ConcurrentQueue<Issue>();


        public IssueDispatcher(IServiceScopeFactory serviceProvider, IConfiguration configuration, IMapper mapper, IHostApplicationLifetime appLifetime)
        {
            _serviceProvider = serviceProvider;
            _configuration = configuration;
            _batchSize = _configuration.GetValue<int>("IssueDispatcher:BatchSize");
            _retryInterval = TimeSpan.FromSeconds(_configuration.GetValue<int>("IssueDispatcher:RetryIntervalSeconds"));
            _mapper = mapper;
            appLifetime.ApplicationStopping.Register(OnApplicationStopping);

            Task.Run(ProcessQueueAsync);//kill upon garbage collection
        }

        private async void OnApplicationStopping()
        {
            await SendIssuesToDatabaseAsync();
        }



        public async void Send(Issue issue)
        {
            _issueQueue.Enqueue(issue);
        }

        private async Task ProcessQueueAsync()
        {
            while (true)
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
            var token = HttpContext.Request.Headers["Authorization"].ToString().Replace("Bearer ", "");//fix
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
}
}


