using AutoMapper;
using Microsoft.EntityFrameworkCore;
using WebApplication1.Data;
using WebApplication1.Entities;
using WebApplication1.Services;

namespace SemiIntegrationTests
{
    public class MockIssueServices : IIssueServices
    {
        private readonly PostgreSQLDbContext _context;
        private readonly IMapper _mapper;

        public MockIssueServices(PostgreSQLDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<IEnumerable<Issue>> GetAll()
        {
            return await _context.Set<Issue>().ToListAsync();
        }

        public async Task<Issue> GetById(int id)
        {
            return await _context.Set<Issue>().FindAsync(id);
        }

        public async Task Add(List<Issue> issues)
        {
            await _context.Set<Issue>().AddRangeAsync(issues);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateById(int[] ids, Issue updatedIssue)
        {
            var models = _context.Set<Issue>().Where(m => ids.Contains(m.EventId));
            foreach (var model in models)
            {
                model.MetricType = updatedIssue.MetricType ?? model.MetricType;
                if (updatedIssue.MetricValue != 0)
                {
                    model.MetricValue = updatedIssue.MetricValue;
                }
                model.TenantId = updatedIssue.TenantId ?? model.TenantId;
                model.JsonField = updatedIssue.JsonField ?? model.JsonField;
            }

            await _context.SaveChangesAsync();
        }

        public async Task Update(List<Issue> issues)
        {
            var issueDict = _context.Set<Issue>()
                .Where(e => issues.Select(v => v.EventId).Contains(e.EventId))
                .ToDictionary(e => e.EventId);

            foreach (var issue in issues)
            {
                var existingIssue = issueDict[issue.EventId];
                _context.Entry(existingIssue).CurrentValues.SetValues(issue);

                var modifiedProperties = _context.Entry(existingIssue).Properties
                    .Where(p => p.Metadata.Name != "Timestamp" && p.Metadata.Name != "EventId")
                    .ToList();

                var hasModifiedProperties = modifiedProperties
                    .Any(p => !Equals(p.OriginalValue, p.CurrentValue));

                if (hasModifiedProperties)
                    existingIssue.Timestamp = DateTime.UtcNow;
            }

            await _context.SaveChangesAsync();
        }

        public async Task Delete(List<int> idList)
        {
            var issues = await _context.Set<Issue>().Where(issue => idList.Contains(issue.EventId)).ToListAsync();
            _context.Set<Issue>().RemoveRange(issues);
            await _context.SaveChangesAsync();
        }
    }

}
