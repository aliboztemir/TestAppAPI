using NUnit.Framework;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using TestAppAPI.Controllers;
using TestAppAPI.Data;
using TestAppAPI.Repositories;
using TestAppAPI.Models;

namespace TestAppAPI.Tests
{
    [TestFixture]
    public class StudyGroupControllerTests
    {
        private AppDbContext _dbContext;
        private StudyGroupRepository _repository;
        private StudyGroupController _controller;

        [SetUp]
        public void Setup()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseSqlite("Filename=:memory:")  // ✅ SQLite in-memory veritabanı kullan
                .Options;

            _dbContext = new AppDbContext(options);
            _dbContext.Database.OpenConnection();
            _dbContext.Database.EnsureCreated();

            _repository = new StudyGroupRepository(_dbContext);
            _controller = new StudyGroupController(_repository);
        }

        [TearDown]
        public void TearDown()
        {
            _dbContext.Database.CloseConnection();
            _dbContext.Dispose();
        }

        // ✅ 1️⃣ StudyGroup oluşturma testleri
        [Test]
        public async Task CreateStudyGroup_Should_Save_To_Database()
        {
            var studyGroup = new StudyGroup(1, "Math Club", Subject.Math, DateTime.Now, new List<User>());

            await _controller.CreateStudyGroup(studyGroup);

            var savedGroup = await _dbContext.StudyGroups.FirstOrDefaultAsync(sg => sg.StudyGroupId == 1);

            Assert.IsNotNull(savedGroup);
            Assert.AreEqual("Math Club", savedGroup.Name);
        }

        [Test]
        public async Task CreateStudyGroup_Should_Return_BadRequest_If_Name_Is_Too_Short()
        {
            var studyGroup = new StudyGroup(2, "Math", Subject.Math, DateTime.Now, new List<User>());

            var result = await _controller.CreateStudyGroup(studyGroup) as BadRequestObjectResult;

            Assert.IsNotNull(result);
            Assert.AreEqual(400, result.StatusCode);
        }

        // ✅ 2️⃣ StudyGroup listeleme testleri
        [Test]
        public async Task GetStudyGroups_Should_Return_All_Groups()
        {
            var studyGroups = new List<StudyGroup>
            {
                new StudyGroup(1, "Math Club", Subject.Math, DateTime.Now, new List<User>()),
                new StudyGroup(2, "Physics Group", Subject.Physics, DateTime.Now, new List<User>())
            };

            await _dbContext.StudyGroups.AddRangeAsync(studyGroups);
            await _dbContext.SaveChangesAsync();

            var result = await _controller.GetStudyGroups() as OkObjectResult;

            Assert.IsNotNull(result);
            Assert.AreEqual(200, result.StatusCode);
            Assert.AreEqual(2, ((List<StudyGroup>)result.Value).Count);
        }

        // ✅ 3️⃣ StudyGroup arama testleri
        [Test]
        public async Task SearchStudyGroups_Should_Return_Filtered_Groups()
        {
            var studyGroups = new List<StudyGroup>
            {
                new StudyGroup(1, "Math Club", Subject.Math, DateTime.Now, new List<User>()),
                new StudyGroup(2, "Physics Group", Subject.Physics, DateTime.Now, new List<User>())
            };

            await _dbContext.StudyGroups.AddRangeAsync(studyGroups);
            await _dbContext.SaveChangesAsync();

            var result = await _controller.SearchStudyGroups("Math") as OkObjectResult;

            Assert.IsNotNull(result);
            Assert.AreEqual(200, result.StatusCode);
            Assert.AreEqual(1, ((List<StudyGroup>)result.Value).Count);
        }

        [Test]
        public async Task SearchStudyGroups_Should_Return_Empty_If_No_Match()
        {
            var result = await _controller.SearchStudyGroups("Biology") as OkObjectResult;

            Assert.IsNotNull(result);
            Assert.AreEqual(200, result.StatusCode);
            Assert.AreEqual(0, ((List<StudyGroup>)result.Value).Count);
        }

        // ✅ 4️⃣ Kullanıcının StudyGroup’a katılması (Join)
        [Test]
        public async Task JoinStudyGroup_Should_Add_User_To_Group()
        {
            var user = new User(1, "Alice");
            var studyGroup = new StudyGroup(1, "Math Club", Subject.Math, DateTime.Now, new List<User>());

            await _dbContext.Users.AddAsync(user);
            await _dbContext.StudyGroups.AddAsync(studyGroup);
            await _dbContext.SaveChangesAsync();

            await _controller.JoinStudyGroup(1, 1);

            var updatedGroup = await _dbContext.StudyGroups.Include(sg => sg.Users).FirstOrDefaultAsync(sg => sg.StudyGroupId == 1);

            Assert.IsNotNull(updatedGroup);
            Assert.AreEqual(1, updatedGroup.Users.Count);
        }

        [Test]
        public async Task JoinStudyGroup_Should_Return_NotFound_If_StudyGroup_Not_Exist()
        {
            var result = await _controller.JoinStudyGroup(999, 1) as NotFoundResult;

            Assert.IsNotNull(result);
            Assert.AreEqual(404, result.StatusCode);
        }

        // ✅ 5️⃣ Kullanıcının StudyGroup’tan çıkması (Leave)
        [Test]
        public async Task LeaveStudyGroup_Should_Remove_User_From_Group()
        {
            var user = new User(2, "Bob");
            var studyGroup = new StudyGroup(2, "Physics Group", Subject.Physics, DateTime.Now, new List<User> { user });

            await _dbContext.Users.AddAsync(user);
            await _dbContext.StudyGroups.AddAsync(studyGroup);
            await _dbContext.SaveChangesAsync();

            await _controller.LeaveStudyGroup(2, 2);

            var updatedGroup = await _dbContext.StudyGroups.Include(sg => sg.Users).FirstOrDefaultAsync(sg => sg.StudyGroupId == 2);

            Assert.IsNotNull(updatedGroup);
            Assert.AreEqual(0, updatedGroup.Users.Count);
        }

        [Test]
        public async Task LeaveStudyGroup_Should_Return_BadRequest_If_User_Not_In_Group()
        {
            var user = new User(3, "Charlie");
            var studyGroup = new StudyGroup(3, "Chemistry Club", Subject.Chemistry, DateTime.Now, new List<User>());

            await _dbContext.Users.AddAsync(user);
            await _dbContext.StudyGroups.AddAsync(studyGroup);
            await _dbContext.SaveChangesAsync();

            var result = await _controller.LeaveStudyGroup(3, 3) as BadRequestResult;

            Assert.IsNotNull(result);
            Assert.AreEqual(400, result.StatusCode);
        }
    }
}
