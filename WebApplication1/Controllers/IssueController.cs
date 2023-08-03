using AutoMapper;
using Microsoft.AspNetCore.Mvc;

using WebApplication1.AccessAttributes;
using WebApplication1.Entities;
using WebApplication1.IssueDispatcher;
using WebApplication1.Models;
using WebApplication1.Services;

namespace WebApplication1.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class IssueController : ControllerBase
    {
        private readonly IIssueServices _issueRepository;
        private readonly IMapper _mapper;
       



        public IssueController(IIssueServices issueRepository, IMapper mapper)
        {
            _issueRepository = issueRepository;
            _mapper = mapper;
        }

        [HttpGet]
        [ServiceAndUserAccess]
        public async Task<IEnumerable<Issue>> Get()
        {
            return await _issueRepository.GetAll();
        }

        [HttpGet("{id}")]
        [UserAccess]
        [ProducesResponseType(typeof(Issue), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetById(int id)
        {
            var issue = await _issueRepository.GetById(id);
            return issue == null ? NotFound() : Ok(issue);
        }

        

        [HttpPost]
        [UserAccess]
        [ProducesResponseType(StatusCodes.Status201Created)]
        public async Task<IActionResult> Create(List<IssueInput> issues)
        {


            if (issues == null || issues.Count == 0)
            {
                return BadRequest();
            }

            //modified to take in multiple issues at once and add them
            List<Issue> createdList = new List<Issue>();

            foreach (var issue in issues)
            {
                //seperate db input from user input 
                var created = _mapper.Map<Issue>(issue);
                created.EventId = 0;
                created.Timestamp = DateTime.UtcNow;
                createdList.Add(created);

            }


            await _issueRepository.Add(createdList);
            return CreatedAtAction(nameof(GetById), new { id = 0 }, issues);

        }

        [HttpPut]
        [ServiceAccess]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Update(List<Issue> issues)
        {

            var newIssues = new List<Issue>();

            foreach (var issue in issues)
            {
                var currentIssue = await _issueRepository.GetById(issue.EventId);

                if (currentIssue == null)
                {
                    return BadRequest();
                }

                _mapper.Map(issue, currentIssue);
                newIssues.Add(currentIssue);

            }

            await _issueRepository.Update(newIssues);
            return NoContent();
        }

        [HttpDelete]
        [ServiceAccess]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Delete(List<int> id)
        {
            foreach (var i in id)
            {
                var issue = await _issueRepository.GetById(i);
                if (issue != null)
                {
                    await _issueRepository.Delete(id);
                    return NoContent();
                }
            }
            return NotFound();

        }

        [HttpPut]
        [ServiceAccess]
        [Route("UpdateByIds")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UpdateByIds([FromQuery] int[] ids, IssueBulkUpdateInput issue)
        {
            //seperate db input from user input
            var issueNew = _mapper.Map<Issue>(issue);
            issueNew.EventId = 0;
            issueNew.Timestamp = DateTime.UtcNow;

            foreach (var i in ids)
            {
                var current = await _issueRepository.GetById(i);
                if (current != null)
                {
                    await _issueRepository.UpdateById(ids, issueNew);
                    return CreatedAtAction(nameof(GetById), new { id = 0 }, issueNew);
                }
            }
            return NotFound();

        }

    }

}
