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
    public class StudyGroupControllerSQLiteTests
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

        [Test, Category("CreateStudyGroup")]
        public async Task CreateStudyGroup_Should_Save_To_Database()
        {
            // Arrange: Create a StudyGroup with one user
            var user = new User(1, "TestUser");
            var studyGroup = new StudyGroup(1, "Math Club", Subject.Math, DateTime.Now, new List<User> { user });

            // Act: Call the controller method to create the StudyGroup
            await _controller.CreateStudyGroup(studyGroup);

            // Assert: Check if the StudyGroup was saved to the database
            var savedGroup = await _dbContext.StudyGroups.FirstOrDefaultAsync(sg => sg.StudyGroupId == 1);

            Assert.IsNotNull(savedGroup);
            Assert.AreEqual("Math Club", savedGroup.Name);
            Assert.AreEqual(1, savedGroup.Users.Count); // Ensure user is added
        }

        [Test, Category("CreateStudyGroup")]
        public async Task CreateStudyGroup_Should_Return_BadRequest_If_Name_Is_Too_Short()
        {
            // Arrange: Create a StudyGroup with a name that is too short
            var user = new User(2, "TestUser");
            var studyGroup = new StudyGroup(2, "Math", Subject.Math, DateTime.Now, new List<User> { user });

            // Act: Call the controller method
            var result = await _controller.CreateStudyGroup(studyGroup) as BadRequestObjectResult;

            // Assert: The result should be BadRequest (400)
            Assert.IsNotNull(result);
            Assert.AreEqual(400, result.StatusCode);
        }

        [Test, Category("CreateStudyGroup")]
        public async Task CreateStudyGroup_Should_Return_BadRequest_If_Name_Is_Too_Long()
        {
            // Arrange: Create a StudyGroup with a name longer than 30 characters
            var user = new User(3, "TestUser");
            var studyGroup = new StudyGroup(3, "ThisIsAVeryLongStudyGroupNameThatExceeds30Chars", Subject.Math, DateTime.Now, new List<User> { user });

            // Act: Attempt to create the StudyGroup
            await _controller.CreateStudyGroup(studyGroup);

            // Assert: Ensure it was NOT added to the database
            var count = await _dbContext.StudyGroups.CountAsync();
            Assert.AreEqual(0, count, "Database should not store a StudyGroup with a name longer than 30 characters.");
        }

        [Test, Category("CreateStudyGroup")]
        public async Task CreateStudyGroup_Should_Return_BadRequest_If_StudyGroup_Is_Null()
        {
            // Act: Try to create a null StudyGroup
            var result = await _controller.CreateStudyGroup(null) as BadRequestResult;

            // Assert: The result should be BadRequest (400)
            Assert.IsNotNull(result);
            Assert.AreEqual(400, result.StatusCode);
        }

        [Test, Category("CreateStudyGroup"), Ignore("Clarification needed - Can a StudyGroup be created with an empty user list?")]
        public async Task CreateStudyGroup_Should_Allow_Empty_User_List()
        {
            // Arrange: Create a StudyGroup with an empty user list
            var studyGroup = new StudyGroup(4, "Empty Users Group", Subject.Chemistry, DateTime.Now, new List<User>());

            // Act: Save the StudyGroup
            await _controller.CreateStudyGroup(studyGroup);

            // Assert: Ensure the StudyGroup was saved with an empty user list
            var savedGroup = await _dbContext.StudyGroups.FirstOrDefaultAsync(sg => sg.StudyGroupId == 4);

            Assert.IsNotNull(savedGroup);
            Assert.AreEqual("Empty Users Group", savedGroup.Name);
            Assert.AreEqual(0, savedGroup.Users.Count); // Ensure user list is empty
        }


        [Test, Category("CreateStudyGroup")]
        public async Task CreateStudyGroup_Should_Return_BadRequest_If_Same_User_Creates_Second_Group_With_Same_Subject()
        {
            // Arrange
            var user = new User(1, "Alice");
            var firstStudyGroup = new StudyGroup(1, "Math Club", Subject.Math, DateTime.Now, new List<User> { user });
            var secondStudyGroup = new StudyGroup(2, "Advanced Math", Subject.Math, DateTime.Now, new List<User> { user });

            await _dbContext.Users.AddAsync(user);
            await _dbContext.StudyGroups.AddAsync(firstStudyGroup);
            await _dbContext.SaveChangesAsync();

            // Act - Kullanıcı aynı subject ile ikinci grup oluşturuyor
            var result = await _controller.CreateStudyGroup(secondStudyGroup) as BadRequestObjectResult;

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(400, result.StatusCode);
        }

        [Test, Category("CreateStudyGroup"), Ignore("Clarification needed - Should the system allow multiple StudyGroups with the same Subject?")]
        public async Task CreateStudyGroup_Should_Return_BadRequest_If_Same_Subject_Already_Exists_SystemWide()
        {
            // Arrange
            var user1 = new User(1, "Alice");
            var user2 = new User(2, "Bob");

            var firstStudyGroup = new StudyGroup(1, "Math Club", Subject.Math, DateTime.Now, new List<User> { user1 });
            var secondStudyGroup = new StudyGroup(2, "Math Experts", Subject.Math, DateTime.Now, new List<User> { user2 });

            await _dbContext.Users.AddRangeAsync(user1, user2);
            await _dbContext.StudyGroups.AddAsync(firstStudyGroup);
            await _dbContext.SaveChangesAsync();

            // Act - Başka bir kullanıcı, aynı subject ile yeni bir grup oluşturuyor
            var result = await _controller.CreateStudyGroup(secondStudyGroup) as BadRequestObjectResult;

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(400, result.StatusCode);
        }

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

            await _dbContext.Users.AddRangeAsync(user1, user2);
            await _dbContext.StudyGroups.AddRangeAsync(studyGroups);
            await _dbContext.SaveChangesAsync();

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

        [Test, Category("GetStudyGroups"), Ignore("Clarification needed - Should study groups be sorted by creation date?")]
        public async Task GetStudyGroups_NeedsClarification_Should_Return_Sorted_By_CreationDate()
        {
            // Awaiting clarification on sorting behavior
        }

        [Test, Category("GetStudyGroups")]
        public async Task GetStudyGroups_Should_Return_Sorted_By_CreationDate()
        {
            // Arrange - Create study groups with different creation dates
            var oldGroup = new StudyGroup(1, "Chemistry Club", Subject.Chemistry, DateTime.Now.AddDays(-5), new List<User>());
            var newGroup = new StudyGroup(2, "Physics Club", Subject.Physics, DateTime.Now, new List<User>());

            await _dbContext.StudyGroups.AddRangeAsync(oldGroup, newGroup);
            await _dbContext.SaveChangesAsync();

            // Act - Fetch study groups
            var result = await _controller.GetStudyGroups() as OkObjectResult;
            var studyGroups = result?.Value as List<StudyGroup>;

            // Assert - Ensure the latest group is first
            Assert.IsNotNull(studyGroups);
            Assert.AreEqual(2, studyGroups.Count);
            Assert.AreEqual("Chemistry Club", studyGroups[0].Name);
            Assert.AreEqual("Physics Club", studyGroups[1].Name);
        }

        [Test, Category("SearchStudyGroups")]
        public async Task SearchStudyGroups_Should_Return_Filtered_Groups()
        {
            // Arrange: Create study groups with users
            var studyGroups = new List<StudyGroup>
            {
                new StudyGroup(1, "Math Club", Subject.Math, DateTime.Now, new List<User> { new User(1, "Alice") }),
                new StudyGroup(2, "Physics Group", Subject.Physics, DateTime.Now, new List<User> { new User(2, "Bob") })
            };

            await _dbContext.StudyGroups.AddRangeAsync(studyGroups);
            await _dbContext.SaveChangesAsync();

            // Act: Search for "Math" study groups
            var result = await _controller.SearchStudyGroups("Math") as OkObjectResult;

            // Assert: Ensure only Math Club is returned
            Assert.IsNotNull(result);
            Assert.AreEqual(200, result.StatusCode);
            Assert.AreEqual(1, ((List<StudyGroup>)result.Value).Count);
        }

        [Test, Category("SearchStudyGroups")]
        public async Task SearchStudyGroups_Should_Return_Empty_If_No_Match()
        {
            // Act: Search for a non-existent study group
            var result = await _controller.SearchStudyGroups("Biology") as OkObjectResult;

            // Assert: No results should be returned
            Assert.IsNotNull(result);
            Assert.AreEqual(200, result.StatusCode);
            Assert.AreEqual(0, ((List<StudyGroup>)result.Value).Count);
        }

        [Test, Category("JoinStudyGroup")]
        public async Task JoinStudyGroup_Should_Add_User_To_Group()
        {
            // Arrange: Create a user and a study group
            var user = new User(1, "Alice");
            var studyGroup = new StudyGroup(1, "Math Club", Subject.Math, DateTime.Now, new List<User>());

            await _dbContext.Users.AddAsync(user);
            await _dbContext.StudyGroups.AddAsync(studyGroup);
            await _dbContext.SaveChangesAsync();

            // Act: User joins the study group
            await _controller.JoinStudyGroup(1, 1);

            // Assert: Verify the user is added to the study group
            var updatedGroup = await _dbContext.StudyGroups.Include(sg => sg.Users)
                                 .FirstOrDefaultAsync(sg => sg.StudyGroupId == 1);

            Assert.IsNotNull(updatedGroup);
            Assert.AreEqual(1, updatedGroup.Users.Count);
        }

        [Test, Category("JoinStudyGroup")]
        public async Task JoinStudyGroup_Should_Allow_User_To_Join_Different_Subjects()
        {
            // Arrange - Create user and two study groups with different subjects
            var user = new User(1, "Alice");

            var mathStudyGroup = new StudyGroup(1, "Math Club", Subject.Math, DateTime.Now, new List<User>());
            var physicsStudyGroup = new StudyGroup(2, "Physics Club", Subject.Physics, DateTime.Now, new List<User>());

            await _dbContext.Users.AddAsync(user);
            await _dbContext.StudyGroups.AddRangeAsync(mathStudyGroup, physicsStudyGroup);
            await _dbContext.SaveChangesAsync();

            // Act - User joins both study groups
            await _controller.JoinStudyGroup(1, 1);
            await _controller.JoinStudyGroup(2, 1);

            // Assert - User should be in both study groups
            var mathGroup = await _dbContext.StudyGroups.Include(sg => sg.Users).FirstOrDefaultAsync(sg => sg.StudyGroupId == 1);
            var physicsGroup = await _dbContext.StudyGroups.Include(sg => sg.Users).FirstOrDefaultAsync(sg => sg.StudyGroupId == 2);

            Assert.IsNotNull(mathGroup);
            Assert.IsNotNull(physicsGroup);
            Assert.IsTrue(mathGroup.Users.Any(u => u.UserId == 1));
            Assert.IsTrue(physicsGroup.Users.Any(u => u.UserId == 1));
        }


        [Test, Category("JoinStudyGroup")]
        public async Task JoinStudyGroup_Should_Return_NotFound_If_StudyGroup_Not_Exist()
        {
            // Act: Try joining a non-existent study group
            var result = await _controller.JoinStudyGroup(999, 1) as NotFoundResult;

            // Assert: Ensure NotFound (404) response
            Assert.IsNotNull(result);
            Assert.AreEqual(404, result.StatusCode);
        }


        [Test, Category("LeaveStudyGroup")]
        public async Task LeaveStudyGroup_Should_Remove_User_From_Group()
        {
            // Arrange: Create a user and a study group that contains the user
            var user = new User(2, "Bob");
            var studyGroup = new StudyGroup(2, "Physics Group", Subject.Physics, DateTime.Now, new List<User> { user });

            await _dbContext.Users.AddAsync(user);
            await _dbContext.StudyGroups.AddAsync(studyGroup);
            await _dbContext.SaveChangesAsync();

            // Act: User leaves the study group
            await _controller.LeaveStudyGroup(2, 2);

            // Assert: Ensure the user is removed from the group
            var updatedGroup = await _dbContext.StudyGroups.Include(sg => sg.Users)
                                 .FirstOrDefaultAsync(sg => sg.StudyGroupId == 2);

            Assert.IsNotNull(updatedGroup);
            Assert.AreEqual(0, updatedGroup.Users.Count);
        }

        [Test, Category("LeaveStudyGroup")]
        public async Task LeaveStudyGroup_Should_Return_BadRequest_If_User_Not_In_Group()
        {
            // Arrange: Create a user who is NOT part of the study group
            var user = new User(3, "Charlie");
            var studyGroup = new StudyGroup(3, "Chemistry Club", Subject.Chemistry, DateTime.Now, new List<User>());

            await _dbContext.Users.AddAsync(user);
            await _dbContext.StudyGroups.AddAsync(studyGroup);
            await _dbContext.SaveChangesAsync();

            // Act: User tries to leave a study group they are not a part of
            var result = await _controller.LeaveStudyGroup(3, 3) as BadRequestResult;

            // Assert: Ensure BadRequest (400) response is returned
            Assert.IsNotNull(result);
            Assert.AreEqual(400, result.StatusCode);
        }

        [Test, Category("LeaveStudyGroup"), Ignore("Clarification needed - Can a user rejoin a group after leaving?")]
        public async Task LeaveStudyGroup_Should_Allow_User_To_Rejoin_After_Leaving()
        {
            // Arrange - Create user and study group
            var user = new User(1, "Alice");
            var studyGroup = new StudyGroup(1, "Math Club", Subject.Math, DateTime.Now, new List<User>());

            await _dbContext.Users.AddAsync(user);
            await _dbContext.StudyGroups.AddAsync(studyGroup);
            await _dbContext.SaveChangesAsync();

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
