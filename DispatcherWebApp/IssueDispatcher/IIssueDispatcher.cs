using WebApplication1.Entities;

namespace WebApplication1.IssueDispatcher
{
    public interface IIssueDispatcher
    {
        void Send(Issue issue);
    }
}
