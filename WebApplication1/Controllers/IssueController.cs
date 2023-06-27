using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApplication1.Data;
using WebApplication1.Entities;
using AutoMapper;
using WebApplication1.Services;
using WebApplication1.Models;

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
        public async Task<IEnumerable<Issue>> Get()
        {
            return await _issueRepository.GetAll();
        }

        [HttpGet("{id}")]
        [ProducesResponseType(typeof(Issue), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetById(int id)
        {
            var issue = await _issueRepository.GetById(id);
            return issue == null ? NotFound() : Ok(issue);
        }

        [HttpPost]
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
                    return NotFound();
                }

                _mapper.Map(issue, currentIssue);
                newIssues.Add(currentIssue);

            }

            await _issueRepository.Update(newIssues);
            return NoContent();
        }

        [HttpDelete]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Delete(List<int> id)
        {
            
               await _issueRepository.Delete(id);
            

            return NoContent();
        }

        [HttpPut]
        [Route("UpdateByIds")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> UpdateByIds([FromQuery] int[] ids, IssueBulkUpdateInput issue)
        {
            //seperate db input from user input
            var issueNew = _mapper.Map<Issue>(issue);
            issueNew.EventId = 0;
            issueNew.Timestamp = DateTime.UtcNow;


            await _issueRepository.UpdateById(ids, issueNew);
            return CreatedAtAction(nameof(GetById), new { id = 0 }, issueNew);


        }

    }

}
