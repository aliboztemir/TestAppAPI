using TestAppAPI.Models;

namespace TestAppAPI.Tests.Unit
{
    [TestFixture]
    public class StudyGroupUnitTests
    {
        [Test]
        public void StudyGroup_Should_Set_Correct_Id()
        {
            var studyGroup = new StudyGroup(1, "Math Club", Subject.Math, DateTime.Now, new List<User>());
            Assert.AreEqual(1, studyGroup.StudyGroupId);
        }

        [Test]
        public void StudyGroup_Should_Set_Correct_Name()
        {
            var studyGroup = new StudyGroup(1, "Math Club", Subject.Math, DateTime.Now, new List<User>());
            Assert.AreEqual("Math Club", studyGroup.Name);
        }

        [Test]
        public void StudyGroup_Should_Set_Correct_Subject()
        {
            var studyGroup = new StudyGroup(1, "Math Club", Subject.Math, DateTime.Now, new List<User>());
            Assert.AreEqual(Subject.Math, studyGroup.Subject);
        }

        [Test]
        public void StudyGroup_Should_Set_Creation_Date()
        {
            var studyGroup = new StudyGroup(1, "Physics Group", Subject.Physics, DateTime.Now, new List<User>());
            Assert.AreNotEqual(default(DateTime), studyGroup.CreateDate);
        }

        [Test]
        public void StudyGroup_Should_Initialize_Empty_User_List_If_Null_Is_Provided()
        {
            var studyGroup = new StudyGroup(2, "Science Club", Subject.Chemistry, DateTime.Now, null);
            Assert.IsNotNull(studyGroup.Users);
            Assert.AreEqual(0, studyGroup.Users.Count);
        }

        [Test]
        public void StudyGroup_Should_Not_Allow_Negative_StudyGroupId()
        {
            Assert.Throws<ArgumentException>(() =>
                new StudyGroup(-1, "Negative ID Club", Subject.Physics, DateTime.Now, new List<User>())
            );
        }

        [Test]
        public void StudyGroup_Name_Should_Be_At_Least_5_Chars()
        {
            Assert.Throws<ArgumentException>(() =>
                new StudyGroup(1, "Math", Subject.Math, DateTime.Now, new List<User>())
            );
        }

        [Test]
        public void StudyGroup_Name_Should_Not_Exceed_30_Chars()
        {
            Assert.Throws<ArgumentException>(() =>
                new StudyGroup(2, "VeryLongStudyGroupNameMoreThan30Chars", Subject.Math, DateTime.Now, new List<User>())
            );
        }

        [Test]
        public void StudyGroup_Name_Should_Not_Be_Null_Or_Empty()
        {
            Assert.Throws<ArgumentException>(() =>
                new StudyGroup(3, "", Subject.Math, DateTime.Now, new List<User>())
            );

            Assert.Throws<ArgumentException>(() =>
                new StudyGroup(4, null, Subject.Math, DateTime.Now, new List<User>())
            );
        }

        [Test]
        public void StudyGroup_Name_Should_Allow_Exact_5_Chars()
        {
            var studyGroup = new StudyGroup(5, "Group", Subject.Math, DateTime.Now, new List<User>());
            Assert.AreEqual("Group", studyGroup.Name);
        }

        [Test]
        public void StudyGroup_Name_Should_Allow_Exact_30_Chars()
        {
            var validName = "ValidGroupNameWith30CharsLong";
            var studyGroup = new StudyGroup(6, validName, Subject.Math, DateTime.Now, new List<User>());
            Assert.AreEqual(validName, studyGroup.Name);
        }

        [Test]
        public void AddUser_Should_Increase_User_Count()
        {
            var studyGroup = new StudyGroup(4, "Chemistry Club", Subject.Chemistry, DateTime.Now, new List<User>());
            var user = new User(5, "Emily");

            studyGroup.AddUser(user);

            Assert.AreEqual(1, studyGroup.Users.Count);
        }

        [Test]
        public void RemoveUser_Should_Decrease_User_Count()
        {
            var user = new User(6, "John");
            var studyGroup = new StudyGroup(5, "Physics Club", Subject.Physics, DateTime.Now, new List<User> { user });

            studyGroup.RemoveUser(user);

            Assert.AreEqual(0, studyGroup.Users.Count);
        }

        [Test]
        public void StudyGroup_Should_Not_Allow_Duplicate_Users()
        {
            var studyGroup = new StudyGroup(1, "Math Club", Subject.Math, DateTime.Now, new List<User>());
            var user = new User(1, "Michael");

            studyGroup.AddUser(user);

            Assert.Throws<InvalidOperationException>(() => studyGroup.AddUser(user));
        }

        [Test]
        public void StudyGroup_Should_Not_Remove_Non_Existing_User()
        {
            var studyGroup = new StudyGroup(1, "Math Club", Subject.Math, DateTime.Now, new List<User>());
            var user = new User(2, "Sarah");

            Assert.Throws<InvalidOperationException>(() => studyGroup.RemoveUser(user));
        }

        [Test]
        public void AddUser_Should_Throw_Exception_If_User_Is_Null()
        {
            var studyGroup = new StudyGroup(1, "Math Club", Subject.Math, DateTime.Now, new List<User>());
            Assert.Throws<ArgumentNullException>(() => studyGroup.AddUser(null));
        }

        [Test]
        public void RemoveUser_Should_Throw_Exception_If_User_Is_Null()
        {
            var studyGroup = new StudyGroup(1, "Math Club", Subject.Math, DateTime.Now, new List<User>());
            Assert.Throws<ArgumentNullException>(() => studyGroup.RemoveUser(null));
        }

        [Test]
        public void StudyGroup_Should_Only_Allow_Valid_Subjects()
        {
            Assert.Throws<ArgumentException>(() =>
                new StudyGroup(3, "Biology Club", (Subject)999, DateTime.Now, new List<User>())
            );
        }

        [Test]
        public void StudyGroup_Should_Not_Allow_Past_Creation_Date()
        {
            Assert.Throws<ArgumentException>(() =>
                new StudyGroup(1, "History Club", Subject.Chemistry, DateTime.Now.AddDays(-1), new List<User>())
            );
        }

        [Test]
        public void StudyGroup_Should_Have_Valid_Initial_State()
        {
            var users = new List<User> { new User(1, "Alice"), new User(2, "Bob") };
            var studyGroup = new StudyGroup(10, "History Club", Subject.Chemistry, DateTime.Now, users);

            Assert.Multiple(() =>
            {
                Assert.AreEqual(10, studyGroup.StudyGroupId);
                Assert.AreEqual("History Club", studyGroup.Name);
                Assert.AreEqual(Subject.Chemistry, studyGroup.Subject);
                Assert.AreEqual(2, studyGroup.Users.Count);
            });
        }
    }
}
