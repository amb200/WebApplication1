using AutoMapper;
using EFCore.BulkExtensions;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.Logging;
using Microsoft.SqlServer.TransactSql.ScriptDom;
using Moq;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WebApplication1;
using WebApplication1.Data;
using WebApplication1.Entities;
using WebApplication1.Services;
using Z.EntityFramework.Extensions;
using Z.EntityFramework.Plus; 
using TestDataUtilities;
using Microsoft.AspNetCore.Mvc;
using System.Linq.Expressions;

namespace TestProject1.Services
{
    [TestFixture]
    public class IssueServicesTests
    {
        private Mock<DbContext> _mockContext;
        private IMapper _mapper;
        private Mock<DbSet<Issue>> _dbSet;
        private Issue genericIssue;

        [SetUp]
        public void Setup()
        {

            _mockContext = new Mock<DbContext>();
            _mapper = new MapperConfiguration(cfg => { cfg.AddProfile<IssueMappingProfile>(); }).CreateMapper();
            _dbSet = new Mock<DbSet<Issue>>();

            genericIssue = new Issue { TenantId = "Issue 1", JsonField = "", MetricType = "", MetricValue = 7 };


        }

        [Test]
        public async Task GetAll_ReturnsAllIssues()
        {
            // Arrange
            var issues = new List<Issue> { genericIssue };
            //SQLServer context
            var dbContextOptionsSql = new DbContextOptionsBuilder<SQLServerDbContext>().UseInMemoryDatabase(databaseName: "GetAll_ReturnsAllIssues").Options;
            using (var dbContext = new SQLServerDbContext(dbContextOptionsSql))
            {
                dbContext.Models.AddRange(issues);
                dbContext.SaveChanges();
                var issueServices = new IssueServices(dbContext, _mapper);
                // Act
                var result = await issueServices.GetAll();
                // Assert
                Assert.AreEqual(issues, result);
            }
            //SQLServer context
            var dbContextOptionsPg = new DbContextOptionsBuilder<PostgreSQLDbContext>().UseInMemoryDatabase(databaseName: "GetAll_ReturnsAllIssues").Options;
            using (var dbContext = new PostgreSQLDbContext(dbContextOptionsPg))
            {
                dbContext.Models.AddRange(issues);
                dbContext.SaveChanges();
                var issueServices = new IssueServices(dbContext, _mapper);
                // Act
                var result = await issueServices.GetAll();
                // Assert
                Assert.AreEqual(issues, result);
            }
            //Bad context
            var dbContextOptions = new DbContextOptionsBuilder<DbContext>().UseInMemoryDatabase(databaseName: "GetAll_ReturnsAllIssues").Options;
            using (var dbContext = new DbContext(dbContextOptions))
            {
                var issueServices = new IssueServices(dbContext, _mapper);
                // Act and Assert
                Assert.ThrowsAsync<NotSupportedException>(async () => await issueServices.GetAll());

            }

        }
        [Test]
        public async Task GetById_ValidId_ReturnsIssue()
        {
            // Arrange
            var id = 1;
            var issues = new List<Issue> { genericIssue, genericIssue };
            //SQLServer context
            var dbContextOptionsSql = new DbContextOptionsBuilder<SQLServerDbContext>().UseInMemoryDatabase(databaseName: "GetById_ValidId_ReturnsIssue").Options;
            using (var dbContext = new SQLServerDbContext(dbContextOptionsSql))
            {
                dbContext.Models.AddRange(issues);
                dbContext.SaveChanges();
                var issueServices = new IssueServices(dbContext, _mapper);
                // Act
                var result = await issueServices.GetById(id);
                // Assert
                Assert.AreEqual(issues[0], result);
            }
            //Postgres context
            var dbContextOptionsPg = new DbContextOptionsBuilder<PostgreSQLDbContext>().UseInMemoryDatabase(databaseName: "GetById_ValidId_ReturnsIssue").Options;
            using (var dbContext = new PostgreSQLDbContext(dbContextOptionsPg))
            {
                dbContext.Models.AddRange(issues);
                dbContext.SaveChanges();
                var issueServices = new IssueServices(dbContext, _mapper);
                // Act
                var result = await issueServices.GetById(id);
                // Assert
                Assert.AreEqual(issues[0], result);
            }
            //Bad context
            var dbContextOptions = new DbContextOptionsBuilder<DbContext>().UseInMemoryDatabase(databaseName: "GetById_ValidId_ReturnsIssue").Options;
            using (var dbContext = new DbContext(dbContextOptions))
            {
                var issueServices = new IssueServices(dbContext, _mapper);
                // Act and Assert
                Assert.ThrowsAsync<NotSupportedException>(async () => await issueServices.GetAll());

            }
        }

        [Test]
        public async Task GetById_InvalidId_ReturnsDbUpdateException()
        {
            // Arrange
            var id = 1;
            var issues = new List<Issue> { new Issue() };
            //SQLServer context
            var dbContextOptionsSql = new DbContextOptionsBuilder<SQLServerDbContext>().UseInMemoryDatabase(databaseName: "GetById_InvalidId_ReturnsDbUpdateException").Options;
            using (var dbContext = new SQLServerDbContext(dbContextOptionsSql))
            {

                // Act
                Exception exception = null;
                try
                {
                    dbContext.Models.AddRange(issues);
                    dbContext.SaveChanges();

                }
                catch (Exception ex)
                {
                    exception = ex;
                }

                // Assert
                Assert.NotNull(exception);
                Assert.IsInstanceOf<DbUpdateException>(exception);

            }

        }

        [Test]
        public async Task Add_ValidIssues_UpdatesDb()
        {
            // Arrange
            var id = 1;
            var issues = new List<Issue> { genericIssue };
            //SQLServer context
            var dbContextOptionsSql = new DbContextOptionsBuilder<SQLServerDbContext>().UseInMemoryDatabase(databaseName: "Add_ValidIssues_UpdatesDb").Options;
            using (var dbContext = new SQLServerDbContext(dbContextOptionsSql))
            {
                dbContext.Models.AddRange(issues);
                dbContext.SaveChanges();
                var issueServices = new IssueServices(dbContext, _mapper);
                // Act
                var result = issueServices.Add(issues);

                // Assert
                CollectionAssert.AreEqual(issues, dbContext.Models.ToList());
            }
            //Postgres context
            var dbContextOptionsPg = new DbContextOptionsBuilder<PostgreSQLDbContext>().UseInMemoryDatabase(databaseName: "Add_ValidIssues_UpdatesDb").Options;
            using (var dbContext = new PostgreSQLDbContext(dbContextOptionsPg))
            {
                dbContext.Models.AddRange(issues);
                dbContext.SaveChanges();
                var issueServices = new IssueServices(dbContext, _mapper);
                // Act
                var result = issueServices.Add(issues);

                // Assert
                CollectionAssert.AreEqual(issues, dbContext.Models.ToList());
            }
            //Bad context
            var dbContextOptions = new DbContextOptionsBuilder<DbContext>().UseInMemoryDatabase(databaseName: "Add_ValidIssues_UpdatesDb").Options;
            using (var dbContext = new DbContext(dbContextOptions))
            {
                var issueServices = new IssueServices(dbContext, _mapper);
                // Act and Assert
                Assert.ThrowsAsync<NotSupportedException>(async () => await issueServices.GetAll());

            }
        }
        [Test]
        public async Task Add_InvalidIssues_ReturnsDbUpdateException()
        {
            // Arrange
            var id = 1;
            var issues = new List<Issue> { new Issue() };
            //SQLServer context
            var dbContextOptionsSql = new DbContextOptionsBuilder<SQLServerDbContext>().UseInMemoryDatabase(databaseName: "Add_InvalidIssues_ReturnsDbUpdateException").Options;
            using (var dbContext = new SQLServerDbContext(dbContextOptionsSql))
            {

                // Act
                Exception exception = null;
                try
                {
                    dbContext.Models.AddRange(issues);
                    dbContext.SaveChanges();
                    var issueServices = new IssueServices(dbContext, _mapper);
                    // Act
                    var result = issueServices.Add(issues);

                }
                catch (Exception ex)
                {
                    exception = ex;
                }

                // Assert
                Assert.NotNull(exception);
                Assert.IsInstanceOf<DbUpdateException>(exception);

            }
        }

        [Test]
        public async Task UpdateById_ValidId_UpdatesDb()
        {
            // Arrange
            var id = 1;
            int[] ints = new int[] { 1, 2, 3 };
            var updatedModel = new Issue
            {
                TenantId = "7",
                MetricType = "cat",
                MetricValue = 69.420,
                JsonField = "{}"
            };

            //SQLServer context
            var dbContextOptionsSql = new DbContextOptionsBuilder<SQLServerDbContext>().UseInMemoryDatabase(databaseName: "UpdateById_ValidId_UpdatesDb").Options;
            using (var dbContext = new SQLServerDbContext(dbContextOptionsSql))
            {
                dbContext.Models.Add(genericIssue);
                dbContext.SaveChanges();
                var issueServices = new IssueServices(dbContext, _mapper);

                // Act

                await issueServices.UpdateById(ints, updatedModel);
                // Assert
                var updatedIssue = await dbContext.Models.FirstOrDefaultAsync(i => i.EventId == id);
                Assert.AreEqual(updatedModel.MetricType, updatedIssue.MetricType);
                Assert.AreEqual(updatedModel.MetricValue, updatedIssue.MetricValue);
                Assert.AreEqual(updatedModel.TenantId, updatedIssue.TenantId);
                Assert.AreEqual(updatedModel.JsonField, updatedIssue.JsonField);
            }
            //Postgres context
            var dbContextOptionsPg = new DbContextOptionsBuilder<PostgreSQLDbContext>().UseInMemoryDatabase(databaseName: "UpdateById_ValidId_UpdatesDb").Options;
            using (var dbContext = new PostgreSQLDbContext(dbContextOptionsPg))
            {
                dbContext.Models.Add(genericIssue);
                dbContext.SaveChanges();
                var issueServices = new IssueServices(dbContext, _mapper);

                // Act
                var result = issueServices.UpdateById(ints, updatedModel);
                // Assert
                var updatedIssue = dbContext.Models.FirstOrDefault(i => i.EventId == id);
                Assert.AreEqual(updatedModel.MetricType, updatedIssue.MetricType);
                Assert.AreEqual(updatedModel.MetricValue, updatedIssue.MetricValue);
                Assert.AreEqual(updatedModel.TenantId, updatedIssue.TenantId);
                Assert.AreEqual(updatedModel.JsonField, updatedIssue.JsonField);
            }
            //Bad context
            var dbContextOptions = new DbContextOptionsBuilder<DbContext>().UseInMemoryDatabase(databaseName: "UpdateById_ValidId_UpdatesDb").Options;
            using (var dbContext = new DbContext(dbContextOptions))
            {
                var issueServices = new IssueServices(dbContext, _mapper);
                // Act and Assert
                Assert.ThrowsAsync<InvalidOperationException>(async () => await issueServices.UpdateById(ints, updatedModel));

            }
        }

        [Test]
        public async Task Update_ValidIssues_UpdatesDb()
        {
            //Arrange
            var issues = new List<Issue> { new Issue{TenantId = "six", MetricType = "dog", JsonField = "{}", MetricValue = 69.6, EventId = 1 }
            };
            //SQLServer context
            var dbContextOptionsSql = new DbContextOptionsBuilder<SQLServerDbContext>().UseInMemoryDatabase(databaseName: "Update_ValidIssues_UpdatesDb").Options;
            using (var dbContext = new SQLServerDbContext(dbContextOptionsSql))
            {
                dbContext.Models.Add(genericIssue);
                dbContext.SaveChanges();
                var issueServices = new IssueServices(dbContext);

                // Act
                await issueServices.Update(issues);
                // Assert
                var updatedIssue = await dbContext.Models.FirstOrDefaultAsync();
                Assert.NotNull(updatedIssue);
                //Assert.AreEqual(issues[0], updatedIssue);
                Assert.AreEqual(issues[0].TenantId, updatedIssue.TenantId);
                Assert.AreEqual(issues[0].MetricType, updatedIssue.MetricType);
                Assert.AreEqual(issues[0].JsonField, updatedIssue.JsonField);
                Assert.AreEqual(issues[0].MetricValue, updatedIssue.MetricValue);
            }
            //Postgre context
            var dbContextOptionsPg = new DbContextOptionsBuilder<PostgreSQLDbContext>().UseInMemoryDatabase(databaseName: "Update_ValidIssues_UpdatesDb").Options;
            using (var dbContext = new PostgreSQLDbContext(dbContextOptionsPg))
            {
                dbContext.Models.Add(genericIssue);
                dbContext.SaveChanges();
                var issueServices = new IssueServices(dbContext, _mapper);

                // Act
                await issueServices.Update(issues);
                // Assert
                var updatedIssue = await dbContext.Models.FirstOrDefaultAsync();
                Assert.NotNull(updatedIssue);
                //Assert.AreEqual(issues[0], updatedIssue);
                Assert.AreEqual(issues[0].TenantId, updatedIssue.TenantId);
                Assert.AreEqual(issues[0].MetricType, updatedIssue.MetricType);
                Assert.AreEqual(issues[0].JsonField, updatedIssue.JsonField);
                Assert.AreEqual(issues[0].MetricValue, updatedIssue.MetricValue);
            }
            //Bad context
            var dbContextOptions = new DbContextOptionsBuilder<DbContext>().UseInMemoryDatabase(databaseName: "Update_ValidIssues_UpdatesDb").Options;
            using (var dbContext = new DbContext(dbContextOptions))
            {
                var issueServices = new IssueServices(dbContext, _mapper);
                // Act and Assert
                Assert.ThrowsAsync<InvalidOperationException>(async () => await issueServices.Update(issues));

            }
        }
        [Test]
        public async Task Update_InvalidIssues_ReturnsDbUpdateException()
        {
            //Arrange
            var issues = new List<Issue> { new Issue{ }
            };
            var dbContextOptionsSql = new DbContextOptionsBuilder<SQLServerDbContext>().UseInMemoryDatabase(databaseName: "Update_InvalidIssues_ReturnsDbUpdateException").Options;
            using (var dbContext = new SQLServerDbContext(dbContextOptionsSql))
            {

                // Act
                Exception exception = null;
                try
                {
                    dbContext.Models.AddRange(issues);
                    dbContext.SaveChanges();
                    var issueServices = new IssueServices(dbContext, _mapper);
                    // Act
                    var result = issueServices.Update(issues);

                }
                catch (Exception ex)
                {
                    exception = ex;
                }

                // Assert
                Assert.NotNull(exception);
                Assert.IsInstanceOf<DbUpdateException>(exception);

            }
        }

        [Test]
        public async Task Delete_ValidIssues_UpdatesDb()
        {
            //Arrange
            List<int> ids = new List<int> { 1 };

            //SQLServer context
            var dbContextOptionsSql = new DbContextOptionsBuilder<SQLServerDbContext>().UseInMemoryDatabase(databaseName: "Delete_ValidIssues_UpdatesDb").Options;
            using (var dbContext = new SQLServerDbContext(dbContextOptionsSql))
            {
                dbContext.Models.Add(genericIssue);
                dbContext.SaveChanges();
                var issueServices = new IssueServices(dbContext, _mapper);

                // Act
                await issueServices.Delete(ids);
                // Assert
                Assert.That(!dbContext.Models.Contains(genericIssue));
            }
            //Postgres context
            var dbContextOptionsPg = new DbContextOptionsBuilder<PostgreSQLDbContext>().UseInMemoryDatabase(databaseName: "Delete_ValidIssues_UpdatesDb").Options;
            using (var dbContext = new PostgreSQLDbContext(dbContextOptionsPg))
            {
                dbContext.Models.Add(genericIssue);
                dbContext.SaveChanges();
                var issueServices = new IssueServices(dbContext, _mapper);

                // Act
                await issueServices.Delete(ids);
                // Assert
                Assert.That(!dbContext.Models.Contains(genericIssue));
            }
            //Bad context
            var dbContextOptions = new DbContextOptionsBuilder<DbContext>().UseInMemoryDatabase(databaseName: "Delete_ValidIssues_UpdatesDb").Options;
            using (var dbContext = new DbContext(dbContextOptions))
            {
                var issueServices = new IssueServices(dbContext, _mapper);
                // Act and Assert
                Assert.ThrowsAsync<NotSupportedException>(async () => await issueServices.Delete(ids));

            }
        }

        [Test]
        public async Task Delete_InvalidIssues_DoesntUpdateDb()
        {//Arrange
            List<int> ids = new List<int> { 2 };

            //SQLServer context
            var dbContextOptionsSql = new DbContextOptionsBuilder<SQLServerDbContext>().UseInMemoryDatabase(databaseName: "Delete_InvalidIssues_DoesntUpdateDb").Options;
            using (var dbContext = new SQLServerDbContext(dbContextOptionsSql))
            {
                dbContext.Models.Add(genericIssue);
                dbContext.SaveChanges();
                var issueServices = new IssueServices(dbContext, _mapper);

                // Act
                await issueServices.Delete(ids);
                // Assert
                Assert.That(dbContext.Models.Contains(genericIssue));
            }
            //Postgres context
            var dbContextOptionsPg = new DbContextOptionsBuilder<PostgreSQLDbContext>().UseInMemoryDatabase(databaseName: "Delete_InvalidIssues_DoesntUpdateDb").Options;
            using (var dbContext = new PostgreSQLDbContext(dbContextOptionsPg))
            {
                dbContext.Models.Add(genericIssue);
                dbContext.SaveChanges();
                var issueServices = new IssueServices(dbContext, _mapper);

                // Act
                await issueServices.Delete(ids);
                // Assert
                Assert.That(dbContext.Models.Contains(genericIssue));
            }
            //Bad context
            var dbContextOptions = new DbContextOptionsBuilder<DbContext>().UseInMemoryDatabase(databaseName: "Delete_InvalidIssues_DoesntUpdateDb").Options;
            using (var dbContext = new DbContext(dbContextOptions))
            {
                var issueServices = new IssueServices(dbContext, _mapper);
                // Act and Assert
                Assert.ThrowsAsync<NotSupportedException>(async () => await issueServices.Delete(ids));

            }

        }


    }
}
