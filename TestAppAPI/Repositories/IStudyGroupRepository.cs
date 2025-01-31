using System.Collections.Generic;
using System.Threading.Tasks;
using TestAppAPI.Models;

namespace TestAppAPI.Repositories
{
    public interface IStudyGroupRepository
    {
        Task CreateStudyGroup(StudyGroup studyGroup);
        Task<List<StudyGroup>> GetStudyGroups();
        Task<List<StudyGroup>> SearchStudyGroups(string subject);
        Task JoinStudyGroup(int studyGroupId, int userId);
        Task LeaveStudyGroup(int studyGroupId, int userId);
    }
}
