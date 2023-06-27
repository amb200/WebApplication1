using WebApplication1.Entities;

namespace WebApplication1.Services
{
    public interface IIssueServices
    {
        Task<IEnumerable<Issue>> GetAll();
        Task<Issue> GetById(int id);
        Task Add(List<Issue> issue);
        Task Update(List<Issue> issue);
        Task Delete(List<int> id);
        Task UpdateById(int[]ids,Issue issues);
    }
}
