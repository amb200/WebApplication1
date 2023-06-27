using Microsoft.AspNetCore.Http;
using Moq;
using NUnit.Framework;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace WebApplication1.Tests
{
    [TestFixture]
    public class IssueControllerTests
    {
        private IssueController _controller;
        private Mock<IIssueServices> _issueServicesMock;

        [SetUp]
        public void Setup()
        {
            _issueServicesMock = new Mock<IIssueServices>();
            _controller = new IssueController(_issueServicesMock.Object, Mock.Of<IMapper>());
        }

        [Test]
        public async Task Get_ReturnsListOfIssues()
        {
            // Arrange
            var expectedIssues = new List<Issue> { /* Add your test data */ };
            _issueServicesMock.Setup(s => s.GetAll())
                .ReturnsAsync(expectedIssues);

            // Act
            var result = await _controller.Get() as OkObjectResult;

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.StatusCode, Is.EqualTo(StatusCodes.Status200OK));

            var issues = result.Value as IEnumerable<Issue>;
            Assert.That(issues, Is.Not.Null);

        }
    }
}
