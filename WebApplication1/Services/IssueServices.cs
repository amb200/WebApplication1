using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using Amazon.DynamoDBv2.DocumentModel;
using Microsoft.EntityFrameworkCore;
using Polly;
using System.Data;
using WebApplication1.Entities;



namespace WebApplication1.Services
{
    public class IssueServices<T> : IIssueServices where T : class
    {


        private readonly T _context;

        public IssueServices(T context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Issue>> GetAll()
        {
            if (_context is DbContext dbContext)
            {
                return await dbContext.Set<Issue>().ToListAsync();
            }
            else if (_context is IDynamoDBContext dynamoDBContext)
            {
                var table = dynamoDBContext.GetTargetTable<Issue>();
                var conditions = new List<ScanCondition>();
                var results = await dynamoDBContext.ScanAsync<Issue>(conditions).GetRemainingAsync();

                return results;
            }
            else
            {
                throw new InvalidOperationException("Unsupported database type.");
            }
        }

        public async Task<Issue> GetById(int id)
        {
            if (_context is DbContext dbContext)
            {
                return await dbContext.FindAsync<Issue>(id);
            }
            else if (_context is IAmazonDynamoDB dynamoDBClient)
            {
                throw new InvalidOperationException("Not Implemented");
            }
            else
            {
                throw new InvalidOperationException("Unsupported database type.");
            }
        }

        public async Task Add(List<Issue> issue)
        {
            if (_context is DbContext dbContext)
            {
                await dbContext.Set<Issue>().AddRangeAsync(issue);
                await dbContext.SaveChangesAsync();
            }
            else if (_context is IDynamoDBContext dynamoDBClient)
            {
                await dynamoDBClient.SaveAsync(issue[0]);
            }
            else
            {
                throw new InvalidOperationException("Unsupported database type.");
            }

        }


        public async Task UpdateById(int[] i, Issue issue)
        {

            if (_context is DbContext dbContext)
            {
                var models = dbContext.Set<Issue>().Where(m => i.Contains(m.EventId));
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
                await dbContext.BulkSaveChangesAsync();
            }
            else if (_context is IAmazonDynamoDB dynamoDBClient)
            {
                throw new InvalidOperationException("Not Implemented");
            }
            else
            {
                throw new InvalidOperationException("Unsupported database type.");
            }
        }



        public async Task Update(List<Issue> issues)
        {


            if (_context is DbContext dbContext)
            {
                Dictionary<int, Issue> issueDict = null;

                issueDict = dbContext.Set<Issue>()
                .Where(e => issues.Select(v => v.EventId).Contains(e.EventId))
                .ToDictionary(e => e.EventId);

                foreach (var issue in issues)
                {

                    var existingIssue = issueDict[issue.EventId];
                    dbContext.Entry(existingIssue).CurrentValues.SetValues(issue);

                    // Exclude Timestamp and EventId from the check for modifications
                    var modifiedProperties = dbContext.Entry(existingIssue).Properties
                        .Where(p => p.Metadata.Name != "Timestamp" && p.Metadata.Name != "EventId")
                        .ToList();

                    // Check if any non-excluded property has an actual change
                    var hasModifiedProperties = modifiedProperties
                        .Any(p => !Equals(p.OriginalValue, p.CurrentValue));

                    // Update the Timestamp property only if there is an actual change in other properties
                    if (hasModifiedProperties)
                        existingIssue.Timestamp = DateTime.UtcNow;

                }
                await dbContext.SaveChangesAsync();
            }
            else if (_context is IAmazonDynamoDB dynamoDBClient)
            {
                throw new InvalidOperationException("Not Implemented");
            }
            else
            {
                throw new InvalidOperationException("Unsupported database type.");
            }

        }

        public async Task Delete(List<int> idList)
        {
            if (_context is DbContext dbContext)
            {
                foreach (var id in idList)
                {
                    var issue = await GetById(id);
                    if (issue == null)
                        return;


                    dbContext.Set<Issue>().Remove(issue);



                    await dbContext.SaveChangesAsync();

                }
            }
            else if (_context is IAmazonDynamoDB dynamoDBClient)
            {
                throw new InvalidOperationException("Not Implemented");
            }
            else
            {
                throw new InvalidOperationException("Unsupported database type.");
            }
            
        }

    }
}
