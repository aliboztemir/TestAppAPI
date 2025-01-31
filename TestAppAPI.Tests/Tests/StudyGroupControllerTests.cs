using NUnit.Framework;
using Moq;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using TestAppAPI.Controllers;
using TestAppAPI.Models;
using TestAppAPI.Repositories;

namespace TestAppAPI.Tests
{
    [TestFixture]
    public class StudyGroupControllerTests
    {
        private Mock<IStudyGroupRepository> _mockRepo;
        private StudyGroupController _controller;

        [SetUp]
        public void Setup()
        {
            _mockRepo = new Mock<IStudyGroupRepository>();
            _controller = new StudyGroupController(_mockRepo.Object);
        }

        // ✅ 1️⃣ CreateStudyGroup Testleri
        [Test]
        public async Task CreateStudyGroup_Should_Return_Created()
        {
            var studyGroup = new StudyGroup(1, "Math Club", Subject.Math, DateTime.Now, new List<User>());
            _mockRepo.Setup(repo => repo.CreateStudyGroup(studyGroup)).Returns(Task.CompletedTask);

            var result = await _controller.CreateStudyGroup(studyGroup) as OkResult;

            Assert.IsNotNull(result);
            Assert.AreEqual(200, result.StatusCode);
        }

        [Test]
        public async Task CreateStudyGroup_Should_Return_BadRequest_If_StudyGroup_Is_Null()
        {
            var result = await _controller.CreateStudyGroup(null) as BadRequestResult;

            Assert.IsNotNull(result);
            Assert.AreEqual(400, result.StatusCode);
        }

        // ✅ 2️⃣ GetStudyGroups Testleri
        [Test]
        public async Task GetStudyGroups_Should_Return_List()
        {
            var studyGroups = new List<StudyGroup>
            {
                new StudyGroup(1, "Math Club", Subject.Math, DateTime.Now, new List<User>()),
                new StudyGroup(2, "Physics Group", Subject.Physics, DateTime.Now, new List<User>())
            };

            _mockRepo.Setup(repo => repo.GetStudyGroups()).ReturnsAsync(studyGroups);

            var result = await _controller.GetStudyGroups() as OkObjectResult;

            Assert.IsNotNull(result);
            Assert.AreEqual(200, result.StatusCode);
            Assert.AreEqual(2, ((List<StudyGroup>)result.Value).Count);
        }

        // ✅ 3️⃣ SearchStudyGroups Testleri
        [Test]
        public async Task SearchStudyGroups_Should_Return_Matching_StudyGroups()
        {
            var studyGroups = new List<StudyGroup>
            {
                new StudyGroup(1, "Math Club", Subject.Math, DateTime.Now, new List<User>())
            };

            _mockRepo.Setup(repo => repo.SearchStudyGroups("Math")).ReturnsAsync(studyGroups);

            var result = await _controller.SearchStudyGroups("Math") as OkObjectResult;

            Assert.IsNotNull(result);
            Assert.AreEqual(200, result.StatusCode);
            Assert.AreEqual(1, ((List<StudyGroup>)result.Value).Count);
        }

        [Test]
        public async Task SearchStudyGroups_Should_Return_Empty_List_If_Subject_Not_Exist()
        {
            _mockRepo.Setup(repo => repo.SearchStudyGroups("NonExistentSubject"))
                     .ReturnsAsync(new List<StudyGroup>());

            var result = await _controller.SearchStudyGroups("NonExistentSubject") as OkObjectResult;

            Assert.IsNotNull(result);
            Assert.AreEqual(200, result.StatusCode);
            Assert.AreEqual(0, ((List<StudyGroup>)result.Value).Count);
        }

        // ✅ 4️⃣ JoinStudyGroup Testleri
        [Test]
        public async Task JoinStudyGroup_Should_Return_OK()
        {
            _mockRepo.Setup(repo => repo.JoinStudyGroup(1, 5)).Returns(Task.CompletedTask);

            var result = await _controller.JoinStudyGroup(1, 5) as OkResult;

            Assert.IsNotNull(result);
            Assert.AreEqual(200, result.StatusCode);
        }

        [Test]
        public async Task JoinStudyGroup_Should_Return_Conflict_If_User_Already_In_Group()
        {
            _mockRepo.Setup(repo => repo.JoinStudyGroup(1, 5)).ThrowsAsync(new InvalidOperationException());

            var result = await _controller.JoinStudyGroup(1, 5) as ConflictResult;

            Assert.IsNotNull(result);
            Assert.AreEqual(409, result.StatusCode);
        }

        [Test]
        public async Task JoinStudyGroup_Should_Return_NotFound_If_StudyGroup_Not_Exist()
        {
            _mockRepo.Setup(repo => repo.JoinStudyGroup(999, 5)).ThrowsAsync(new KeyNotFoundException());

            var result = await _controller.JoinStudyGroup(999, 5) as NotFoundResult;

            Assert.IsNotNull(result);
            Assert.AreEqual(404, result.StatusCode);
        }

        // ✅ 5️⃣ LeaveStudyGroup Testleri
        [Test]
        public async Task LeaveStudyGroup_Should_Return_OK()
        {
            _mockRepo.Setup(repo => repo.LeaveStudyGroup(1, 5)).Returns(Task.CompletedTask);

            var result = await _controller.LeaveStudyGroup(1, 5) as OkResult;

            Assert.IsNotNull(result);
            Assert.AreEqual(200, result.StatusCode);
        }

        [Test]
        public async Task LeaveStudyGroup_Should_Return_BadRequest_If_User_Not_In_Group()
        {
            _mockRepo.Setup(repo => repo.LeaveStudyGroup(1, 5)).ThrowsAsync(new InvalidOperationException());

            var result = await _controller.LeaveStudyGroup(1, 5) as BadRequestResult;

            Assert.IsNotNull(result);
            Assert.AreEqual(400, result.StatusCode);
        }

        [Test]
        public async Task LeaveStudyGroup_Should_Return_NotFound_If_StudyGroup_Not_Exist()
        {
            _mockRepo.Setup(repo => repo.LeaveStudyGroup(999, 5)).ThrowsAsync(new KeyNotFoundException());

            var result = await _controller.LeaveStudyGroup(999, 5) as NotFoundResult;

            Assert.IsNotNull(result);
            Assert.AreEqual(404, result.StatusCode);
        }
    }
}
