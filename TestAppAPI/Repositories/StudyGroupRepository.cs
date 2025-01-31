using System.Collections.Generic;
using System.Threading.Tasks;
using TestAppAPI.Models;

namespace TestAppAPI.Repositories
{
    public class StudyGroupRepository : IStudyGroupRepository
    {
        private readonly List<StudyGroup> _studyGroups = new();

        // Adds a new StudyGroup to the list
        public Task CreateStudyGroup(StudyGroup studyGroup)
        {
            _studyGroups.Add(studyGroup);
            return Task.CompletedTask;
        }

        // Returns all StudyGroups
        public Task<List<StudyGroup>> GetStudyGroups()
        {
            return Task.FromResult(_studyGroups);
        }

        // Returns StudyGroups filtered by subject
        public Task<List<StudyGroup>> SearchStudyGroups(string subject)
        {
            var filteredGroups = _studyGroups.FindAll(sg => sg.Subject.ToString() == subject);
            return Task.FromResult(filteredGroups);
        }

        // Allows a user to join a StudyGroup
        public Task JoinStudyGroup(int studyGroupId, int userId)
        {
            var group = _studyGroups.Find(sg => sg.StudyGroupId == studyGroupId);
            if (group == null)
                throw new KeyNotFoundException("Study Group not found");

            if (group.Users.Exists(u => u.UserId == userId))
                throw new InvalidOperationException("User already in group");

            group.Users.Add(new User(userId, $"User{userId}"));
            return Task.CompletedTask;
        }

        // Allows a user to leave a StudyGroup
        public Task LeaveStudyGroup(int studyGroupId, int userId)
        {
            var group = _studyGroups.Find(sg => sg.StudyGroupId == studyGroupId);
            if (group == null)
                throw new KeyNotFoundException("Study Group not found");

            var user = group.Users.Find(u => u.UserId == userId);
            if (user == null)
                throw new InvalidOperationException("User not in group");

            group.Users.Remove(user);
            return Task.CompletedTask;
        }
    }
}
