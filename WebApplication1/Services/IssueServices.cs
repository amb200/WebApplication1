using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using System.Data;
using WebApplication1.Data;
using WebApplication1.Entities;
using Z.EntityFramework.Extensions;
using Z.EntityFramework.Plus;



namespace WebApplication1.Services
{
    public class IssueServices : IIssueServices
    {


        private readonly DbContext _context;
        private readonly IMapper _mapper;


        public IssueServices(DbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<IEnumerable<Issue>> GetAll()
        {
            if (_context is SQLServerDbContext)
                return await (_context as SQLServerDbContext).Models.ToListAsync();
            else if (_context is PostgreSQLDbContext)
                return await (_context as PostgreSQLDbContext).Models.ToListAsync();
            else
                throw new NotSupportedException("Unsupported DbContext type");
        }

        public async Task<Issue> GetById(int id)
        {
            if (_context is SQLServerDbContext)
                return await (_context as SQLServerDbContext).Models.FindAsync(id);
            else if (_context is PostgreSQLDbContext)
                return await (_context as PostgreSQLDbContext).Models.FindAsync(id);
            else
                throw new NotSupportedException("Unsupported DbContext type");
        }

        public async Task Add(List<Issue> issue)
        {
            if (_context is SQLServerDbContext)
                await (_context as SQLServerDbContext).Models.AddRangeAsync(issue);
            else if (_context is PostgreSQLDbContext)
                await (_context as PostgreSQLDbContext).Models.AddRangeAsync(issue);
            else
                throw new NotSupportedException("Unsupported DbContext type");

            await _context.SaveChangesAsync();
        }


        public async Task UpdateById(int[] i, Issue issue)
        {

            if (_context is PostgreSQLDbContext)
            {
                var dbContext = (PostgreSQLDbContext)_context;
                var models = dbContext.Models.Where(m => i.Contains(m.EventId));
                foreach (var model in models)
                {
                    model.MetricType = issue.MetricType ?? model.MetricType;
                    if (issue.MetricValue != 0)
                    {
                        model.MetricValue = issue.MetricValue;
                    }
                    model.TenantId = issue.TenantId ?? model.TenantId;
                    model.JsonField = issue.JsonField ?? model.JsonField;

                }
            }
            else if (_context is SQLServerDbContext)
            {
                var dbContext = (SQLServerDbContext)_context;
                var models = dbContext.Models.Where(m => i.Contains(m.EventId));
                foreach (var model in models)
                {
                    model.MetricType = issue.MetricType ?? model.MetricType;
                    if (issue.MetricValue != 0)
                    {
                        model.MetricValue = issue.MetricValue;
                    }
                    model.TenantId = issue.TenantId ?? model.TenantId;
                    model.JsonField = issue.JsonField ?? model.JsonField;
                }
            }
            else
            {
                // Handle unsupported DbContext type or missing issueDict initialization
                throw new InvalidOperationException("Unsupported DbContext type");
            }

            await _context.BulkSaveChangesAsync();
        }



        public async Task Update(List<Issue> issues)
        {

            Dictionary<int, Issue> issueDict = null;

            if (_context is PostgreSQLDbContext)
            {
                issueDict = (_context as PostgreSQLDbContext).Models
                .Where(e => issues.Select(v => v.EventId).Contains(e.EventId))
                .ToDictionary(e => e.EventId);
            }
            else if (_context is SQLServerDbContext)
            {
                issueDict = (_context as SQLServerDbContext).Models
               .Where(e => issues.Select(v => v.EventId).Contains(e.EventId))
               .ToDictionary(e => e.EventId);
            }
            else
            {
                // Handle unsupported DbContext type or missing issueDict initialization
                throw new InvalidOperationException("Unsupported DbContext type");
            }


            foreach (var issue in issues)
            {

                var existingIssue = issueDict[issue.EventId];
                _context.Entry(existingIssue).CurrentValues.SetValues(issue);

                // Exclude Timestamp and EventId from the check for modifications
                var modifiedProperties = _context.Entry(existingIssue).Properties
                    .Where(p => p.Metadata.Name != "Timestamp" && p.Metadata.Name != "EventId")
                    .ToList();

                // Check if any non-excluded property has an actual change
                var hasModifiedProperties = modifiedProperties
                    .Any(p => !Equals(p.OriginalValue, p.CurrentValue));

                // Update the Timestamp property only if there is an actual change in other properties
                if (hasModifiedProperties)
                    existingIssue.Timestamp = DateTime.UtcNow;

            }
            await _context.SaveChangesAsync();

        }

        public async Task Delete(List<int> idList)
        {

            foreach (var id in idList)
            {
                var issue = await GetById(id);
                if (issue == null)
                    return;

                if (_context is SQLServerDbContext)
                    (_context as SQLServerDbContext).Models.Remove(issue);
                else if (_context is PostgreSQLDbContext)
                    (_context as PostgreSQLDbContext).Models.Remove(issue);
                else
                    throw new NotSupportedException("Unsupported DbContext type");
            }


            await _context.SaveChangesAsync();

        }
    }

}
