using AutoMapper;
using Newtonsoft.Json;
using Polly;
using System.Collections.Concurrent;
using System.Text;
using WebApplication1.Entities;
using WebApplication1.Services;

namespace WebApplication1.IssueDispatcher
{
    public class IssueDispatcher : IIssueDispatcher
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly IConfiguration _configuration;
        private readonly int _batchSize;
        private readonly TimeSpan _retryInterval;
        private readonly IMapper _mapper;

        private readonly ConcurrentQueue<Issue> _issueQueue = new ConcurrentQueue<Issue>();


        public IssueDispatcher(IServiceProvider serviceProvider, IConfiguration configuration, IMapper mapper)
        {
            _serviceProvider = serviceProvider;
            _configuration = configuration;
            _batchSize = _configuration.GetValue<int>("IssueDispatcher:BatchSize");
            _retryInterval = TimeSpan.FromSeconds(_configuration.GetValue<int>("IssueDispatcher:RetryIntervalSeconds"));
            _mapper = mapper;
        }

        public async void Send(Issue issue)
        {
            _issueQueue.Enqueue(issue);
            if (_issueQueue.Count == _batchSize)
            {
                using (var scope = _serviceProvider.CreateScope())
                {
                    var issueServices = scope.ServiceProvider.GetRequiredService<IIssueServices>();
                    var httpClientFactory = scope.ServiceProvider.GetRequiredService<IHttpClientFactory>();
                    var httpClient = httpClientFactory.CreateClient();
                    httpClient.BaseAddress = new Uri("https://localhost:7264");

                    var issuesToSend = new ConcurrentQueue<Issue>();
                    while (issuesToSend.Count <= _batchSize && _issueQueue.TryDequeue(out var issue1))
                    {
                        issuesToSend.Enqueue(issue1);
                    }

                    if (issuesToSend.Count == _batchSize)
                    {
                        var policy = Policy
                            .Handle<HttpRequestException>()
                            .WaitAndRetryAsync(3, _ => _retryInterval);

                        await policy.ExecuteAsync(async () =>
                        {
                            List<Issue> createdList = new List<Issue>();

                            foreach (var issue in issuesToSend)
                            {
                                //seperate db input from user input 
                                var created = _mapper.Map<Issue>(issue);
                                created.EventId = 0;
                                created.Timestamp = DateTime.UtcNow;
                                createdList.Add(created);

                            }


                            await issueServices.Add(createdList);
                        });
                    }
                }
            }

        }
    }
}


