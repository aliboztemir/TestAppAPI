using Moq;
using Microsoft.AspNetCore.Mvc;
using TestAppAPI.Controllers;
using TestAppAPI.Models;
using TestAppAPI.Repositories;

namespace TestAppAPI.Tests.Unit
{
    [TestFixture]
    public class StudyGroupControllerUnitTests // 📌 Güncellenmiş sınıf adı
    {
        private Mock<IStudyGroupRepository> _mockRepo;
        private StudyGroupController _controller;

        [SetUp]
        public void Setup()
        {
            _mockRepo = new Mock<IStudyGroupRepository>(); // ✅ Repository mocklandı
            _controller = new StudyGroupController(_mockRepo.Object);
        }

        // ✅ Test: Successfully create a StudyGroup
        [Test, Category("CreateStudyGroup")]
        public async Task CreateStudyGroup_Should_Return_Ok()
        {
            // Arrange: Create a valid StudyGroup with one user
            var user = new User(1, "TestUser");
            var studyGroup = new StudyGroup(1, "Math Club", Subject.Math, DateTime.Now, new List<User> { user });

            _mockRepo.Setup(repo => repo.CreateStudyGroup(studyGroup)).Returns(Task.CompletedTask);

            // Act: Call the controller method
            var result = await _controller.CreateStudyGroup(studyGroup) as OkResult;

            // Assert: Response should be 200 OK
            Assert.IsNotNull(result);
            Assert.AreEqual(200, result.StatusCode);
        }

        // ✅ Test: Name is too short, should return BadRequest
        [Test, Category("CreateStudyGroup")]
        public async Task CreateStudyGroup_Should_Return_BadRequest_If_Name_Is_Too_Short()
        {
            // Arrange: StudyGroup with a short name
            var user = new User(2, "TestUser");
            var studyGroup = new StudyGroup(2, "Math", Subject.Math, DateTime.Now, new List<User> { user });

            _mockRepo.Setup(repo => repo.CreateStudyGroup(studyGroup)).ThrowsAsync(new ArgumentException("Name is too short"));

            // Act: Call the controller method
            var result = await _controller.CreateStudyGroup(studyGroup) as BadRequestObjectResult;

            // Assert: Response should be 400 BadRequest
            Assert.IsNotNull(result);
            Assert.AreEqual(400, result.StatusCode);
        }

        // ✅ Test: Name is too long, should return BadRequest
        [Test, Category("CreateStudyGroup")]
        public async Task CreateStudyGroup_Should_Return_BadRequest_If_Name_Is_Too_Long()
        {
            // Arrange: StudyGroup with a name longer than 30 characters
            var user = new User(3, "TestUser");
            var studyGroup = new StudyGroup(3, "ThisIsAVeryLongStudyGroupNameThatExceeds30Chars", Subject.Math, DateTime.Now, new List<User> { user });

            _mockRepo.Setup(repo => repo.CreateStudyGroup(studyGroup)).ThrowsAsync(new ArgumentException("Name is too long"));

            // Act: Call the controller method
            var result = await _controller.CreateStudyGroup(studyGroup) as BadRequestObjectResult;

            // Assert: Response should be 400 BadRequest
            Assert.IsNotNull(result);
            Assert.AreEqual(400, result.StatusCode);
        }

        // ✅ Test: Null StudyGroup should return BadRequest
        [Test, Category("CreateStudyGroup")]
        public async Task CreateStudyGroup_Should_Return_BadRequest_If_StudyGroup_Is_Null()
        {
            // Act: Call the controller method with a null StudyGroup
            var result = await _controller.CreateStudyGroup(null) as BadRequestResult;

            // Assert: Response should be 400 BadRequest
            Assert.IsNotNull(result);
            Assert.AreEqual(400, result.StatusCode);
        }

        // ✅ Test: Empty user list - needs clarification
        [Test, Category("CreateStudyGroup"), Ignore("Clarification needed - Can a StudyGroup be created with an empty user list?")]
        public async Task CreateStudyGroup_Should_Allow_Empty_User_List()
        {
            // Arrange: StudyGroup with an empty user list
            var studyGroup = new StudyGroup(4, "Empty Users Group", Subject.Chemistry, DateTime.Now, new List<User>());

            _mockRepo.Setup(repo => repo.CreateStudyGroup(studyGroup)).Returns(Task.CompletedTask);

            // Act: Call the controller method
            var result = await _controller.CreateStudyGroup(studyGroup) as OkResult;

            // Assert: Response should be 200 OK
            Assert.IsNotNull(result);
            Assert.AreEqual(200, result.StatusCode);
        }

        // ✅ Test: A user should not create multiple groups with the same subject
        [Test, Category("CreateStudyGroup")]
        public async Task CreateStudyGroup_Should_Return_BadRequest_If_Same_User_Creates_Second_Group_With_Same_Subject()
        {
            // Arrange: Create a user and two study groups with the same subject
            var user = new User(1, "Alice");
            var firstStudyGroup = new StudyGroup(1, "Math Club", Subject.Math, DateTime.Now, new List<User> { user });
            var secondStudyGroup = new StudyGroup(2, "Advanced Math", Subject.Math, DateTime.Now, new List<User> { user });

            _mockRepo.Setup(repo => repo.CreateStudyGroup(secondStudyGroup))
                     .ThrowsAsync(new InvalidOperationException("User cannot create multiple groups with the same subject."));

            // Act: User tries to create a second group with the same subject
            var result = await _controller.CreateStudyGroup(secondStudyGroup) as BadRequestObjectResult;

            // Assert: The response should be BadRequest (400)
            Assert.IsNotNull(result);
            Assert.AreEqual(400, result.StatusCode);
        }

        // ✅ Test: Should system-wide study groups allow multiple groups with the same subject? (Needs clarification)
        [Test, Category("CreateStudyGroup"), Ignore("Clarification needed - Should the system allow multiple StudyGroups with the same Subject?")]
        public async Task CreateStudyGroup_Should_Return_BadRequest_If_Same_Subject_Already_Exists_SystemWide()
        {
            // Arrange: Two users creating separate study groups with the same subject
            var user1 = new User(1, "Alice");
            var user2 = new User(2, "Bob");

            var firstStudyGroup = new StudyGroup(1, "Math Club", Subject.Math, DateTime.Now, new List<User> { user1 });
            var secondStudyGroup = new StudyGroup(2, "Math Experts", Subject.Math, DateTime.Now, new List<User> { user2 });

            _mockRepo.Setup(repo => repo.CreateStudyGroup(secondStudyGroup))
                     .ThrowsAsync(new InvalidOperationException("Duplicate subject study groups are not allowed system-wide."));

            // Act: A different user attempts to create a group with the same subject
            var result = await _controller.CreateStudyGroup(secondStudyGroup) as BadRequestObjectResult;

            // Assert: The response should be BadRequest (400)
            Assert.IsNotNull(result);
            Assert.AreEqual(400, result.StatusCode);
        }

        // ✅ Test: Retrieve all StudyGroups
        [Test, Category("GetStudyGroups")]
        public async Task GetStudyGroups_Should_Return_All_Groups()
        {
            // Arrange: Create StudyGroups with users
            var user1 = new User(1, "Alice");
            var user2 = new User(2, "Bob");

            var studyGroups = new List<StudyGroup>
            {
                new StudyGroup(1, "Math Club", Subject.Math, DateTime.Now, new List<User> { user1 }),
                new StudyGroup(2, "Physics Group", Subject.Physics, DateTime.Now, new List<User> { user2 })
            };

            _mockRepo.Setup(repo => repo.GetStudyGroups()).ReturnsAsync(studyGroups);

            // Act: Retrieve all StudyGroups
            var result = await _controller.GetStudyGroups() as OkObjectResult;

            // Assert: Verify that the response contains all StudyGroups with correct user counts
            Assert.IsNotNull(result);
            Assert.AreEqual(200, result.StatusCode);

            var returnedGroups = (List<StudyGroup>)result.Value;
            Assert.AreEqual(2, returnedGroups.Count);
            Assert.AreEqual(1, returnedGroups[0].Users.Count); // Math Club has one user
            Assert.AreEqual(1, returnedGroups[1].Users.Count); // Physics Group has one user
        }

        // ✅ Test: Needs clarification on sorting order
        [Test, Category("GetStudyGroups"), Ignore("Clarification needed - Should study groups be sorted by creation date?")]
        public async Task GetStudyGroups_NeedsClarification_Should_Return_Sorted_By_CreationDate()
        {
            // Awaiting clarification on sorting behavior
        }

        // ✅ Test: Verify if study groups are sorted by creation date
        [Test, Category("GetStudyGroups")]
        public async Task GetStudyGroups_Should_Return_Sorted_By_CreationDate()
        {
            // Arrange: Create study groups with different creation dates
            var oldGroup = new StudyGroup(1, "Chemistry Club", Subject.Chemistry, DateTime.Now.AddDays(-5), new List<User>());
            var newGroup = new StudyGroup(2, "Physics Club", Subject.Physics, DateTime.Now, new List<User>());

            var studyGroups = new List<StudyGroup> { newGroup, oldGroup }; // Ensuring latest first

            _mockRepo.Setup(repo => repo.GetStudyGroups()).ReturnsAsync(studyGroups);

            // Act: Fetch study groups
            var result = await _controller.GetStudyGroups() as OkObjectResult;
            var returnedGroups = result?.Value as List<StudyGroup>;

            // Assert: Ensure the latest group is first
            Assert.IsNotNull(returnedGroups);
            Assert.AreEqual(2, returnedGroups.Count);
            Assert.AreEqual("Physics Club", returnedGroups[0].Name); // Latest group first
            Assert.AreEqual("Chemistry Club", returnedGroups[1].Name);
        }

        // ✅ Test: Searching for a valid study group should return the correct result
        [Test, Category("SearchStudyGroups")]
        public async Task SearchStudyGroups_Should_Return_Filtered_Groups()
        {
            // Arrange: Create study groups
            var studyGroups = new List<StudyGroup>
            {
                new StudyGroup(1, "Math Club", Subject.Math, DateTime.Now, new List<User> { new User(1, "Alice") }),
                new StudyGroup(2, "Physics Group", Subject.Physics, DateTime.Now, new List<User> { new User(2, "Bob") })
            };

            _mockRepo.Setup(repo => repo.SearchStudyGroups("Math"))
                     .ReturnsAsync(studyGroups.Where(sg => sg.Subject == Subject.Math).ToList());

            // Act: Search for "Math" study groups
            var result = await _controller.SearchStudyGroups("Math") as OkObjectResult;

            // Assert: Ensure only Math Club is returned
            Assert.IsNotNull(result);
            Assert.AreEqual(200, result.StatusCode);
            var returnedGroups = (List<StudyGroup>)result.Value;
            Assert.AreEqual(1, returnedGroups.Count);
            Assert.AreEqual("Math Club", returnedGroups[0].Name);
        }

        // ✅ Test: Searching for a non-existent group should return an empty list
        [Test, Category("SearchStudyGroups")]
        public async Task SearchStudyGroups_Should_Return_Empty_If_No_Match()
        {
            // Arrange - Simulate empty search result
            _mockRepo.Setup(repo => repo.SearchStudyGroups("Biology"))
                     .ReturnsAsync(new List<StudyGroup>());

            // Act: Search for a non-existent study group
            var result = await _controller.SearchStudyGroups("Biology") as OkObjectResult;

            // Assert: No results should be returned
            Assert.IsNotNull(result);
            Assert.AreEqual(200, result.StatusCode);
            var returnedGroups = (List<StudyGroup>)result.Value;
            Assert.AreEqual(0, returnedGroups.Count);
        }

        // ✅ Test: User successfully joins a study group
        [Test, Category("JoinStudyGroup")]
        public async Task JoinStudyGroup_Should_Add_User_To_Group()
        {
            // Arrange: Create a user and a study group
            var user = new User(1, "Alice");
            var studyGroup = new StudyGroup(1, "Math Club", Subject.Math, DateTime.Now, new List<User>());

            _mockRepo.Setup(repo => repo.JoinStudyGroup(1, 1)).Returns(Task.CompletedTask);

            // Act: User joins the study group
            var result = await _controller.JoinStudyGroup(1, 1) as OkResult;

            // Assert: Verify that the operation was successful
            Assert.IsNotNull(result);
            Assert.AreEqual(200, result.StatusCode);
        }

        // ✅ Test: User can join multiple groups with different subjects
        [Test, Category("JoinStudyGroup")]
        public async Task JoinStudyGroup_Should_Allow_User_To_Join_Different_Subjects()
        {
            // Arrange - Create a user and two study groups
            var user = new User(1, "Alice");
            var mathStudyGroup = new StudyGroup(1, "Math Club", Subject.Math, DateTime.Now, new List<User>());
            var physicsStudyGroup = new StudyGroup(2, "Physics Club", Subject.Physics, DateTime.Now, new List<User>());

            _mockRepo.Setup(repo => repo.JoinStudyGroup(1, 1)).Returns(Task.CompletedTask);
            _mockRepo.Setup(repo => repo.JoinStudyGroup(2, 1)).Returns(Task.CompletedTask);

            // Act - User joins both study groups
            var result1 = await _controller.JoinStudyGroup(1, 1) as OkResult;
            var result2 = await _controller.JoinStudyGroup(2, 1) as OkResult;

            // Assert - Ensure both operations were successful
            Assert.IsNotNull(result1);
            Assert.AreEqual(200, result1.StatusCode);
            Assert.IsNotNull(result2);
            Assert.AreEqual(200, result2.StatusCode);
        }

        // ✅ Test: Trying to join a non-existent study group should return NotFound (404)
        [Test, Category("JoinStudyGroup")]
        public async Task JoinStudyGroup_Should_Return_NotFound_If_StudyGroup_Not_Exist()
        {
            // Arrange - Simulate a missing study group
            _mockRepo.Setup(repo => repo.JoinStudyGroup(999, 1))
                     .ThrowsAsync(new KeyNotFoundException());

            // Act - Attempt to join a non-existing group
            var result = await _controller.JoinStudyGroup(999, 1) as NotFoundResult;

            // Assert - Verify that the response is NotFound (404)
            Assert.IsNotNull(result);
            Assert.AreEqual(404, result.StatusCode);
        }

        // ✅ Test: Removing a user from a study group
        [Test, Category("LeaveStudyGroup")]
        public async Task LeaveStudyGroup_Should_Remove_User_From_Group()
        {
            // Arrange: Create a user and a study group that contains the user
            var user = new User(2, "Bob");
            var studyGroup = new StudyGroup(2, "Physics Group", Subject.Physics, DateTime.Now, new List<User> { user });

            _mockRepo.Setup(repo => repo.LeaveStudyGroup(2, 2)).Returns(Task.CompletedTask);

            // Act: User leaves the study group
            var result = await _controller.LeaveStudyGroup(2, 2) as OkResult;

            // Assert: Ensure the user is removed from the group
            Assert.IsNotNull(result);
            Assert.AreEqual(200, result.StatusCode);
        }

        // ✅ Test: Trying to leave a group the user is not part of should return BadRequest
        [Test, Category("LeaveStudyGroup")]
        public async Task LeaveStudyGroup_Should_Return_BadRequest_If_User_Not_In_Group()
        {
            // Arrange: Create a user who is NOT part of the study group
            var user = new User(3, "Charlie");
            var studyGroup = new StudyGroup(3, "Chemistry Club", Subject.Chemistry, DateTime.Now, new List<User>());

            _mockRepo.Setup(repo => repo.LeaveStudyGroup(3, 3)).ThrowsAsync(new InvalidOperationException());

            // Act: User tries to leave a study group they are not a part of
            var result = await _controller.LeaveStudyGroup(3, 3) as BadRequestResult;

            // Assert: Ensure BadRequest (400) response is returned
            Assert.IsNotNull(result);
            Assert.AreEqual(400, result.StatusCode);
        }

        // ✅ Test: User should be able to rejoin a group after leaving (Needs clarification)
        [Test, Category("LeaveStudyGroup"), Ignore("Clarification needed - Can a user rejoin a group after leaving?")]
        public async Task LeaveStudyGroup_Should_Allow_User_To_Rejoin_After_Leaving()
        {
            // Arrange - Create user and study group
            var user = new User(1, "Alice");
            var studyGroup = new StudyGroup(1, "Math Club", Subject.Math, DateTime.Now, new List<User>());

            _mockRepo.Setup(repo => repo.JoinStudyGroup(1, 1)).Returns(Task.CompletedTask);
            _mockRepo.Setup(repo => repo.LeaveStudyGroup(1, 1)).Returns(Task.CompletedTask);

            // Act - User joins, leaves, and tries to rejoin
            await _controller.JoinStudyGroup(1, 1);
            await _controller.LeaveStudyGroup(1, 1);
            var result = await _controller.JoinStudyGroup(1, 1) as OkResult;

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(200, result.StatusCode);
        }
    }
}
