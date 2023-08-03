using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using WebApplication1.Entities;
using WebApplication1.Models;
using WebApplication1.Services;

namespace WebApplication1.IssueDispatcher
{
    [Route("api/[controller]")]
    [ApiController]
    public class TestController : ControllerBase
    {
        private readonly IMapper _mapper;
        private readonly IIssueDispatcher _issueDispatcher;

        public TestController(IMapper mapper, IIssueDispatcher issueDispatcher)
        {
            _mapper = mapper;
            _issueDispatcher = issueDispatcher;
        }


        [HttpPost]
        public async Task<IActionResult> PostIssue(IssueInput issueInput)
        {
            // Convert issueInput to Issue entity
            var issue = _mapper.Map<Issue>(issueInput);
            issue.EventId = 0;
            issue.Timestamp = DateTime.UtcNow;

            // Enqueue the issue for processing
            _issueDispatcher.Send(issue);

            // Return your response
            return Ok("Issue enqueued for processing.");
        }
    }
}
