using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using WebApplication1;
using WebApplication1.Controllers;
using WebApplication1.Entities;
using WebApplication1.Models;
using WebApplication1.Services;

namespace TestProject1
{
    public class IssueControllerTests
    {
        private IssueController _controller;
        private Mock<IIssueServices> _issueServicesMock;
        private IMapper _mapper = new MapperConfiguration(cfg =>
        {
            cfg.AddProfile<IssueMappingProfile>();
        }).CreateMapper();
        private Issue genericIssue;

        [SetUp]
        public void Setup()
        {
            _issueServicesMock = new Mock<IIssueServices>();
            _controller = new IssueController(_issueServicesMock.Object, _mapper);
            genericIssue = new Issue { };
        }

        [Test]
        public async Task Get_ReturnsListOfIssues()
        {
            // Arrange
            var expectedIssues = new List<Issue> { genericIssue, genericIssue };
            _issueServicesMock.Setup(s => s.GetAll())
                .ReturnsAsync(expectedIssues);

            // Act
            var result = await _controller.Get();
            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result, Is.InstanceOf<IEnumerable<Issue>>());
            Assert.That(result, Is.EqualTo(expectedIssues));
        }


        [Test]
        public async Task GetById_ExistingId_ReturnsIssue()
        {
            // Arrange
            var id = 1;
            _issueServicesMock.Setup(s => s.GetById(id))
                .ReturnsAsync(genericIssue);

            // Act
            var result = await _controller.GetById(id) as OkObjectResult;

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.StatusCode, Is.EqualTo(StatusCodes.Status200OK));

            var issue = result.Value as Issue;
            Assert.That(issue, Is.EqualTo(genericIssue));
        }

        [Test]
        public async Task GetById_NotExistingId_RetrurnsNotFoundResult()
        {
            //Arrange
            var id = 1;
            _issueServicesMock.Setup(s => s.GetById(id)).ReturnsAsync((Issue)null);

            //Act
            var result = await _controller.GetById(id) as NotFoundResult;

            //Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.StatusCode, Is.EqualTo(StatusCodes.Status404NotFound));
        }

        [Test]
        public async Task Create_ValidIssues_ReturnsCreatedAtAction()
        {
            // Arrange
            var issues = new List<IssueInput> { new IssueInput { MetricType = "cat", JsonField = "{}", MetricValue = 2, TenantId = "Ok" } };
            var createdIssues = new List<Issue> { };
             _mapper.Map<Issue>(issues[0]);
            _issueServicesMock.Setup(s => s.Add(createdIssues))
                .Returns(Task.CompletedTask);

            // Act
            var result = await _controller.Create(issues) as CreatedAtActionResult;

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.StatusCode, Is.EqualTo(StatusCodes.Status201Created));
            Assert.That(result.ActionName, Is.EqualTo(nameof(IssueController.GetById)));
            Assert.That(result.RouteValues["id"], Is.EqualTo(0));
            Assert.That(result.Value, Is.EqualTo(issues));
        }
        [Test]
        public async Task Create_InvalidIssues_ReturnsBadRequest()
        {
            // Arrange
            var invalidIssues = new List<IssueInput>();

            // Act

            var result = await _controller.Create(invalidIssues) as BadRequestResult;

            // Assert
            Assert.That(result.StatusCode, Is.EqualTo(StatusCodes.Status400BadRequest));

        }

        [Test]
        public async Task Update_ValidIssues_ReturnsNoContent()
        {
            //Arrange
            var issues = new List<Issue> { new Issue { MetricType = "cat" } };
            var newIssues = new List<Issue> { };


            _mapper.Map<Issue>(issues[0]);
            _issueServicesMock.Setup(s => s.Add(newIssues)).Returns(Task.CompletedTask);

            //Act
            var result = await _controller.Update(newIssues) as NoContentResult;

            //Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.StatusCode, Is.EqualTo(StatusCodes.Status204NoContent));
            

        }

        [Test]
        public async Task Update_InvalidIssues_ReturnsNotFound()
        {
            // Arrange

            var invalidIssues = new List<Issue>
            {
                new Issue{MetricType = "cat"},
                new Issue{MetricValue = 420.69}
            };
            _mapper.Map<Issue>(invalidIssues[0]);
            _issueServicesMock.Setup(s => s.GetById(It.IsAny<int>())).ReturnsAsync((Issue)null);

            // Act
            var result = await _controller.Update(invalidIssues) as NotFoundResult;

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.StatusCode, Is.EqualTo(StatusCodes.Status404NotFound));
        }

        [Test]
        public async Task Delete_ValidIssues_ReturnsNoContent()
        {
            var id = new List<int>();

            //Arrange 
            _issueServicesMock.Setup(s => s.Delete(id)).Verifiable();

            //Act
            var result = await _controller.Delete(id) as NoContentResult;

            //Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.StatusCode, Is.EqualTo(StatusCodes.Status204NoContent));
            _issueServicesMock.Verify(r => r.Delete(id), Times.Once);
        }

        [Test]
        public async Task UpdateByIds_ValidInput_ReturnsCreatedAtAction()
        {

            // Arrange
            var ids = new int[] { 1, 2, 3 };
            var issue = new IssueBulkUpdateInput
            {
                JsonField = "id",
                MetricType = "",

            };

            var issueMapped = _mapper.Map<Issue>(issue);
            issueMapped.EventId = 0;
            issueMapped.Timestamp = DateTime.UtcNow;

            _issueServicesMock.Setup(r => r.UpdateById(ids, It.IsAny<Issue>())).Returns(Task.CompletedTask);

            // Act
            var result = await _controller.UpdateByIds(ids, issue) as CreatedAtActionResult;

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.StatusCode, Is.EqualTo(StatusCodes.Status201Created));
            Assert.That(result.ActionName, Is.EqualTo(nameof(IssueController.GetById)));
            Assert.That(result.RouteValues["id"], Is.EqualTo(0));
        }


    }
}