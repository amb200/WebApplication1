using AutoMapper;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Polly;
using System.Collections.Concurrent;
using System.Text;
using WebApplication1.Entities;
using WebApplication1.Services;

namespace WebApplication1.IssueDispatcher
{
    public class IssueDispatcher : IIssueDispatcher, IAsyncDisposable
    {
        private readonly IServiceScopeFactory _serviceProvider;
        private readonly IConfiguration _configuration;
        private readonly int _batchSize;
        private readonly TimeSpan _retryInterval;
        private readonly IMapper _mapper;

        private readonly ConcurrentQueue<Issue> _issueQueue = new ConcurrentQueue<Issue>();


        public IssueDispatcher(IServiceScopeFactory serviceProvider, IConfiguration configuration, IMapper mapper)
        {
            _serviceProvider = serviceProvider;
            _configuration = configuration;
            _batchSize = _configuration.GetValue<int>("IssueDispatcher:BatchSize");
            _retryInterval = TimeSpan.FromSeconds(_configuration.GetValue<int>("IssueDispatcher:RetryIntervalSeconds"));
            _mapper = mapper;

            Task.Run(ProcessQueueAsync);
        }

        public async ValueTask DisposeAsync()
        {
            try
            {
                await SendIssuesToDatabaseAsync();
            }
            catch (Exception ex) {
                IServiceScopeFactory b = _serviceProvider;
                await SendIssuesToDatabaseAsync();
            }
            
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
            using (var scope = _serviceProvider.CreateScope())
            {
                var issueServices = scope.ServiceProvider.GetRequiredService<IIssueServices>();
                var httpClientFactory = scope.ServiceProvider.GetRequiredService<IHttpClientFactory>();
                var httpClient = httpClientFactory.CreateClient();
                httpClient.BaseAddress = new Uri("https://localhost:7264");

                var issuesToSend = new List<Issue>();
                while (issuesToSend.Count < _batchSize && _issueQueue.TryDequeue(out var issue))
                {
                    // Separate db input from user input 
                    var created = _mapper.Map<Issue>(issue);
                    created.EventId = 0;
                    created.Timestamp = DateTime.UtcNow;
                    issuesToSend.Add(created);
                }

                var policy = Policy
                    .Handle<HttpRequestException>()
                    .WaitAndRetryAsync(3, _ => _retryInterval);

                await policy.ExecuteAsync(async () =>
                {
                    await issueServices.Add(issuesToSend);
                });
            }
        }
    }
}


